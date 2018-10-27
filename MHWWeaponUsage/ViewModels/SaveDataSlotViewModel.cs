using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MHWSaveUtils;

namespace MHWWeaponUsage.ViewModels
{
    public sealed class SaveDataSlotViewModel : ViewModelBase, IDisposable
    {
        public int SlotNumber { get; }
        public string Name { get; }
        public uint Rank { get; }
        public string Playtime { get; }

        private bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible; }
            set { SetValue(ref isVisible, value); }
        }

        public WeaponUsageViewModel LowRank { get; }
        public WeaponUsageViewModel HighRank { get; }
        public WeaponUsageViewModel Investigations { get; }
        public WeaponUsageViewModel Total { get; }

        public WeaponUsageSaveSlotInfo SaveSlotInfo { get; }

        public SaveDataSlotViewModel(RootViewModel rootViewModel, WeaponUsageSaveSlotInfo saveSlotInfo)
        {
            SaveSlotInfo = saveSlotInfo;

            SlotNumber = saveSlotInfo.SlotNumber;
            Name = saveSlotInfo.Name;
            Rank = saveSlotInfo.Rank;
            Playtime = MiscUtils.PlaytimeToGameString(saveSlotInfo.Playtime);

            LowRank = new WeaponUsageViewModel(rootViewModel, ViewType.LowRank, saveSlotInfo.LowRank);
            HighRank = new WeaponUsageViewModel(rootViewModel, ViewType.HighRank, saveSlotInfo.HighRank);
            Investigations = new WeaponUsageViewModel(rootViewModel, ViewType.Investigations, saveSlotInfo.Investigations);

            Total = new WeaponUsageViewModel(rootViewModel, ViewType.Total, saveSlotInfo.LowRank + saveSlotInfo.HighRank + saveSlotInfo.Investigations);
        }

        public void Dispose()
        {
            LowRank.Dispose();
            HighRank.Dispose();
            Investigations.Dispose();
            Total.Dispose();
        }
    }
}
