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
    public sealed class AccountViewModel : ViewModelBase, IDisposable
    {
        public string UserId { get; }

        private readonly ObservableCollection<SaveDataSlotViewModel> saveDataItems = new ObservableCollection<SaveDataSlotViewModel>();
        public ReadOnlyObservableCollection<SaveDataSlotViewModel> SaveDataItems { get; }

        private readonly RootViewModel rootViewModel;
        private readonly string saveDataFullFilename;

        public AccountViewModel(RootViewModel rootViewModel, SaveDataInfo saveDataInfo)
        {
            if (rootViewModel == null)
                throw new ArgumentNullException(nameof(rootViewModel));
            if (saveDataInfo.SaveDataFullFilename == null)
                throw new ArgumentException($"Argument '{nameof(saveDataInfo)}' is invalid");

            this.rootViewModel = rootViewModel;

            UserId = saveDataInfo.UserId;
            saveDataFullFilename = saveDataInfo.SaveDataFullFilename;

            SaveDataItems = new ReadOnlyObservableCollection<SaveDataSlotViewModel>(saveDataItems);
        }

        private SaveDataFileMonitor saveDataFileMonitor;

        public async Task InitializeAsync()
        {
            await LoadSaveDataAsync(CancellationToken.None);

            saveDataFileMonitor = new SaveDataFileMonitor(saveDataFullFilename, 0);
            saveDataFileMonitor.SaveDataFileChanged += OnSaveDataFileChanged;
        }

        private void OnSaveDataFileChanged(object sender, SaveDataChangedEventArgs e)
        {
            LoadSaveDataAsync(e.CancellationToken).Forget();
        }

        private async Task LoadSaveDataAsync(CancellationToken cancellationToken)
        {
            var ms = new MemoryStream();

            using (Stream inputStream = File.OpenRead(saveDataFullFilename))
            {
                await Crypto.DecryptAsync(inputStream, ms, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
                return;

            using (var weaponUsageReader = new WeaponUsageReader(ms))
            {
                foreach (SaveDataSlotViewModel saveDataItem in saveDataItems)
                    saveDataItem.Dispose();

                saveDataItems.Clear();

                foreach (WeaponUsageSaveSlotInfo saveSlotInfo in weaponUsageReader.Read())
                    saveDataItems.Add(new SaveDataSlotViewModel(rootViewModel, saveSlotInfo));
            }
        }

        public void Dispose()
        {
            foreach (SaveDataSlotViewModel saveDataItem in saveDataItems)
                saveDataItem.Dispose();

            if (saveDataFileMonitor != null)
            {
                saveDataFileMonitor.Dispose();
                saveDataFileMonitor = null;
            }
        }
    }
}
