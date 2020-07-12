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
    public sealed class AccountViewModel : ViewModelBase, IDisposable
    {
        public string UserId { get; }

        private readonly ObservableCollection<SaveDataSlotViewModel> saveDataItems = new ObservableCollection<SaveDataSlotViewModel>();
        public ReadOnlyObservableCollection<SaveDataSlotViewModel> SaveDataItems { get; }

        private bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible; }
            set { SetValue(ref isVisible, value); }
        }

        public ICommand MiniModeToggleCommand { get { return rootViewModel.MiniModeToggleCommand; } }

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

        private async void OnSaveDataFileChanged(object sender, SaveDataChangedEventArgs e)
        {
            while (true)
            {
                try
                {
                    await LoadSaveDataAsync(e.CancellationToken);
                    break;
                }
                catch (IOException)
                {
                    // Retry in 500 ms.
                    await Task.Delay(500);
                }
            }
        }

        private async Task LoadSaveDataAsync(CancellationToken cancellationToken)
        {
            var ms = new MemoryStream();
            var crypto = new Crypto();

            using (Stream inputStream = File.OpenRead(saveDataFullFilename))
            {
                byte[] buffer = new byte[inputStream.Length];
                await inputStream.ReadAsync(buffer, 0, buffer.Length);

                await crypto.DecryptAsync(buffer);

                await ms.WriteAsync(buffer, 0, buffer.Length);
            }

            if (cancellationToken.IsCancellationRequested)
                return;

            using (var weaponUsageReader = new WeaponUsageReader(ms))
            {
                foreach (SaveDataSlotViewModel saveDataItem in saveDataItems)
                    saveDataItem.Dispose();

                saveDataItems.Clear();

                foreach (WeaponUsageSaveSlotInfo saveSlotInfo in weaponUsageReader.Read())
                {
                    saveSlotInfo.SetSaveDataInfo(new SaveDataInfo(UserId, saveDataFullFilename));
                    saveDataItems.Add(new SaveDataSlotViewModel(rootViewModel, saveSlotInfo));
                }
            }

            rootViewModel.ApplyMiniModeVisibility();
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
