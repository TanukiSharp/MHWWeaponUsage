using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MHWWeaponUsage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            IList<GameSaveInfo> gameSaveInfo = Utils.EnumerateGameSaveDataInfo().ToList();

            if (gameSaveInfo.Count == 0)
            {
                MessageBox.Show("Could not determine location of save data.", "Save data not found");
            }
            else if (gameSaveInfo.Count == 1)
            {
                var crypto = new Crypto();
                using (Stream inputStream = File.OpenRead(gameSaveInfo[0].SaveDataFullFilename))
                {
                    var ms = new MemoryStream();

                    crypto.Decrypt(inputStream, ms);

                    var weaponUsageReader = new WeaponUsageReader(ms);

                    //string targetFilename = $"{gameSaveInfo[0].SaveDataFullFilename}.decrypted.bin";
                    //File.WriteAllBytes(targetFilename, ms.ToArray());

                    weaponUsageReader.Read();
                }
            }
            else
            {
                // display user selector
            }
        }
    }
}
