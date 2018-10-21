using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MHWWeaponUsage.ViewModels
{
    public class RootViewModel : ViewModelBase
    {
        private readonly ObservableCollection<AccountViewModel> accounts = new ObservableCollection<AccountViewModel>();
        public ReadOnlyObservableCollection<AccountViewModel> Accounts { get; }

        public RootViewModel()
        {
            Accounts = new ReadOnlyObservableCollection<AccountViewModel>(accounts);
        }

        public void Reload()
        {
            foreach (SaveDataInfo saveDataInfo in Utils.EnumerateSaveDataInfo())
                accounts.Add(new AccountViewModel(saveDataInfo));
        }
    }
}
