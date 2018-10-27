using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using MHWSaveUtils;

namespace MHWWeaponUsage.ViewModels
{
    public enum Sorting
    {
        Tree,
        UsageAscending,
        UsageDescending
    }

    public enum ViewType
    {
        LowRank,
        HighRank,
        Investigations,
        Total
    }

    public class RootViewModel : ViewModelBase
    {
        private readonly ObservableCollection<AccountViewModel> accounts = new ObservableCollection<AccountViewModel>();
        public ReadOnlyObservableCollection<AccountViewModel> Accounts { get; }

        public event EventHandler MiniModeChanged;

        private bool isMiniMode;
        public bool IsMiniMode
        {
            get { return isMiniMode; }
            set
            {
                if (SetValue(ref isMiniMode, value))
                    MiniModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public ICommand CloseCommand { get; }
        public ICommand MiniModeToggleCommand { get; }

        public int SortingIndex
        {
            get { return (int)Sorting; }
            set { Sorting = (Sorting)value; }
        }

        private Sorting sorting = Sorting.Tree;
        public Sorting Sorting
        {
            get { return sorting; }
            set
            {
                if (SetValue(ref sorting, value))
                    SortingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler SortingChanged;

        public int ViewTypeIndex
        {
            get { return (int)ViewType; }
            set { ViewType = (ViewType)value; }
        }

        private ViewType viewType = ViewType.Total;
        public ViewType ViewType
        {
            get { return viewType; }
            set
            {
                if (SetValue(ref viewType, value))
                    ViewTypeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public event EventHandler ViewTypeChanged;

        public SaveSlotSelectorViewModel SelectorViewModel { get; } = new SaveSlotSelectorViewModel();

        private readonly Func<Task<WeaponUsageSaveSlotInfo>> onBeginMiniMode;

        public RootViewModel(Func<Task<WeaponUsageSaveSlotInfo>> onBeginMiniMode)
        {
            this.onBeginMiniMode = onBeginMiniMode;

            Accounts = new ReadOnlyObservableCollection<AccountViewModel>(accounts);

            MiniModeToggleCommand = new AnonymousCommand(OnMiniMode);
            CloseCommand = new AnonymousCommand(() => App.Current.Shutdown());
        }

        private async void OnMiniMode()
        {
            ((AnonymousCommand)MiniModeToggleCommand).IsEnabled = false;

            try
            {
                if (IsMiniMode == false)
                    await TurnMiniModeOn();
                else
                    await TurnMiniModeOff();
            }
            finally
            {
                ((AnonymousCommand)MiniModeToggleCommand).IsEnabled = true;
            }
        }

        private async Task TurnMiniModeOn()
        {
            WeaponUsageSaveSlotInfo result = await onBeginMiniMode();

            if (result == null)
                return;

            foreach (AccountViewModel account in Accounts)
            {
                if (account.UserId == result.SaveDataInfo.UserId)
                {
                    account.IsVisible = true;
                    foreach (SaveDataSlotViewModel saveDataItem in account.SaveDataItems)
                        saveDataItem.IsVisible = saveDataItem.SlotNumber == result.SlotNumber;
                }
                else
                    account.IsVisible = false;
            }


            IsMiniMode = true;
        }

        private Task TurnMiniModeOff()
        {
            foreach (AccountViewModel account in Accounts)
            {
                foreach (SaveDataSlotViewModel saveDataItem in account.SaveDataItems)
                    saveDataItem.IsVisible = true;
                account.IsVisible = true;
            }

            IsMiniMode = false;

            return Task.CompletedTask;
        }

        public async Task Reload()
        {
            foreach (AccountViewModel account in accounts)
                account.Dispose();

            accounts.Clear();

            var taskList = new List<Task>();

            foreach (SaveDataInfo saveDataInfo in FileSystemUtils.EnumerateSaveDataInfo())
            {
                var accountViewModel = new AccountViewModel(this, saveDataInfo);
                accounts.Add(accountViewModel);
                taskList.Add(accountViewModel.InitializeAsync());
            }

            await Task.WhenAll(taskList);

            SelectorViewModel.Clear();
            foreach (AccountViewModel account in accounts)
                SelectorViewModel.AddSaveData(account.UserId, account.SaveDataItems.Select(x => x.SaveSlotInfo));
        }
    }
}
