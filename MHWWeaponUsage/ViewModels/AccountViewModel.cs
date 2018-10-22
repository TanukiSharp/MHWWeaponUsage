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
    public class AccountViewModel : ViewModelBase, IDisposable
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

            InitializeAsync().ForgetRethrowOnError();
        }

        private async Task InitializeAsync()
        {
            await LoadSaveDataAsync(CancellationToken.None);

            var fsw = new FileSystemWatcher(
                Path.GetDirectoryName(saveDataFullFilename),
                Path.GetFileName(saveDataFullFilename)
            )
            {
                IncludeSubdirectories = false
            };

            fsw.Renamed += OnSaveDataChanged;

            fsw.EnableRaisingEvents = true;
        }

        private CancellationTokenSource cancellationTokenSource;

        private void OnSaveDataChanged(object sender, RenamedEventArgs e)
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }

            cancellationTokenSource = new CancellationTokenSource();

            Dispatcher.Invoke(() => LoadSaveDataAsync(cancellationTokenSource.Token).ForgetRethrowOnError());
        }

        private async Task LoadSaveDataAsync(CancellationToken cancellationToken)
        {
            var crypto = new Crypto();

            using (Stream inputStream = File.OpenRead(saveDataFullFilename))
            {
                var ms = new MemoryStream();

                int thread = Thread.CurrentThread.ManagedThreadId;

                await crypto.DecryptAsync(inputStream, ms, cancellationToken);

                thread = Thread.CurrentThread.ManagedThreadId;

                if (cancellationToken.IsCancellationRequested)
                    return;

                var weaponUsageReader = new WeaponUsageReader(ms);

                //string targetFilename = $"{saveDataFullFilename}.decrypted.bin";
                //File.WriteAllBytes(targetFilename, ms.ToArray());

                foreach (SaveDataSlotViewModel saveDataItem in saveDataItems)
                    saveDataItem.Dispose();

                saveDataItems.Clear();

                foreach (SaveSlotInfo saveSlotInfo in weaponUsageReader.Read())
                    saveDataItems.Add(new SaveDataSlotViewModel(rootViewModel, saveSlotInfo));
            }
        }

        public void Dispose()
        {
            foreach (SaveDataSlotViewModel saveDataItem in saveDataItems)
                saveDataItem.Dispose();

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }
    }
}
