using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        private bool isMiniMode;
        public bool IsMiniMode
        {
            get { return isMiniMode; }
            set { SetValue(ref isMiniMode, value); }
        }

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

        public RootViewModel()
        {
            Accounts = new ReadOnlyObservableCollection<AccountViewModel>(accounts);
        }

        public void Reload()
        {
            foreach (AccountViewModel account in accounts)
                account.Dispose();

            accounts.Clear();

            foreach (SaveDataInfo saveDataInfo in FileSystemUtils.EnumerateSaveDataInfo())
                accounts.Add(new AccountViewModel(this, saveDataInfo));
        }
    }
}
