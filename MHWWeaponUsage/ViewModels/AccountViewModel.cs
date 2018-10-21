using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MHWWeaponUsage.ViewModels
{
    public class AccountViewModel : ViewModelBase
    {
        public string UserId { get; }

        private readonly string saveDataFullFilename;

        public AccountViewModel(SaveDataInfo saveDataInfo)
        {
            UserId = saveDataInfo.UserId;
            saveDataFullFilename = saveDataInfo.SaveDataFullFilename;

            LoadSaveData(CancellationToken.None).ForgetRethrowOnError();
        }

        private async Task LoadSaveData(CancellationToken cancellationToken)
        {
            var crypto = new Crypto();

            using (Stream inputStream = File.OpenRead(saveDataFullFilename))
            {
                var ms = new MemoryStream();

                await crypto.DecryptAsync(inputStream, ms, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                var weaponUsageReader = new WeaponUsageReader(ms);

                string targetFilename = $"{saveDataFullFilename}.decrypted.bin";
                File.WriteAllBytes(targetFilename, ms.ToArray());

                foreach (SaveSlotInfo saveSlotInfo in weaponUsageReader.Read())
                {
                }
            }
        }
    }
}
