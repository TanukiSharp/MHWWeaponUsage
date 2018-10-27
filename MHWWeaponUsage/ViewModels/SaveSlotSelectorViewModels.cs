using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MHWSaveUtils;

namespace MHWWeaponUsage.ViewModels
{
    public class SelectorSaveDataSlotViewModel : ViewModelBase
    {
        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set { SetValue(ref isSelected, value); }
        }

        public int SlotNumber { get; }
        public string Name { get; }
        public uint Rank { get; }
        public string Playtime { get; }
        public uint Zeni { get; }

        public ICommand SelectionCommand { get; }

        private readonly Action onSelection;

        public WeaponUsageSaveSlotInfo WeaponUsage { get; }

        public SelectorSaveDataSlotViewModel(Action onSelection, WeaponUsageSaveSlotInfo saveDataInfo)
        {
            this.onSelection = onSelection;

            WeaponUsage = saveDataInfo;

            SlotNumber = saveDataInfo.SlotNumber;
            Name = saveDataInfo.Name;
            Rank = saveDataInfo.Rank;
            Playtime = MiscUtils.PlaytimeToGameString(saveDataInfo.Playtime);
            Zeni = saveDataInfo.Zeni;

            SelectionCommand = new AnonymousCommand(OnSelection);
        }

        private void OnSelection()
        {
            IsSelected = true;
            onSelection();
        }
    }

    public class SelectorAccountViewModel : ViewModelBase
    {
        public string UserId { get; }
        public IReadOnlyCollection<SelectorSaveDataSlotViewModel> SaveDataSlots { get; }

        public SelectorAccountViewModel(Action onSelection, string userId, IEnumerable<WeaponUsageSaveSlotInfo> saveDataSlots)
        {
            UserId = userId;

            var list = saveDataSlots.Select(x => new SelectorSaveDataSlotViewModel(onSelection, x)).ToList();
            SaveDataSlots = new ReadOnlyCollection<SelectorSaveDataSlotViewModel>(list);
        }
    }

    public class SaveSlotSelectorViewModel : ViewModelBase
    {
        private readonly ObservableCollection<SelectorAccountViewModel> accounts = new ObservableCollection<SelectorAccountViewModel>();
        public ReadOnlyObservableCollection<SelectorAccountViewModel> Accounts { get; }

        public event EventHandler SelectionDone;

        public WeaponUsageSaveSlotInfo SelectedWeaponUsage
        {
            get
            {
                SelectorSaveDataSlotViewModel selectedSaveDataSlot = null;

                foreach (SelectorAccountViewModel account in Accounts)
                {
                    foreach (SelectorSaveDataSlotViewModel saveDataSlot in account.SaveDataSlots)
                    {
                        if (saveDataSlot.IsSelected)
                        {
                            selectedSaveDataSlot = saveDataSlot;
                            break;
                        }
                    }
                    if (selectedSaveDataSlot != null)
                        break;
                }

                return selectedSaveDataSlot?.WeaponUsage;
            }
        }

        public SaveSlotSelectorViewModel()
        {
            Accounts = new ReadOnlyObservableCollection<SelectorAccountViewModel>(accounts);
        }

        public void ClearSelection()
        {
            foreach (SelectorAccountViewModel account in Accounts)
            {
                foreach (SelectorSaveDataSlotViewModel saveDataSlot in account.SaveDataSlots)
                    saveDataSlot.IsSelected = false;
            }
        }

        public void AddSaveData(string userId, IEnumerable<WeaponUsageSaveSlotInfo> saveDataSlotItems)
        {
            accounts.Add(new SelectorAccountViewModel(OnSelection, userId, saveDataSlotItems));
        }

        private void OnSelection()
        {
            SelectionDone?.Invoke(this, EventArgs.Empty);
        }

        public void Clear()
        {
            accounts.Clear();
        }
    }
}
