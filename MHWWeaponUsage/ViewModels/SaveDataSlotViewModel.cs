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
        public uint HR { get; }
        public uint MR { get; }
        public string Playtime { get; }

        private bool isVisible = true;
        public bool IsVisible
        {
            get { return isVisible; }
            set { SetValue(ref isVisible, value); }
        }

        public WeaponUsageViewModel LowRank { get; }
        public WeaponUsageViewModel HighRank { get; }
        public WeaponUsageViewModel MasterRank { get; }
        public WeaponUsageViewModel Investigations { get; }
        public WeaponUsageViewModel GuidingLands { get; }
        public WeaponUsageViewModel All { get; }

        public WeaponUsageSaveSlotInfo SaveSlotInfo { get; }

        public SaveDataSlotViewModel(RootViewModel rootViewModel, WeaponUsageSaveSlotInfo saveSlotInfo)
        {
            SaveSlotInfo = saveSlotInfo;

            SlotNumber = saveSlotInfo.SlotNumber;
            Name = saveSlotInfo.Name;
            HR = saveSlotInfo.HR;
            MR = saveSlotInfo.MR;
            Playtime = MiscUtils.PlaytimeToGameString(saveSlotInfo.Playtime);

            LowRank = new WeaponUsageViewModel(rootViewModel, ViewType.LowRank, saveSlotInfo.LowRank);
            HighRank = new WeaponUsageViewModel(rootViewModel, ViewType.HighRank, saveSlotInfo.HighRank);
            MasterRank = new WeaponUsageViewModel(rootViewModel, ViewType.MasterRank, saveSlotInfo.MasterRank);
            Investigations = new WeaponUsageViewModel(rootViewModel, ViewType.Investigations, saveSlotInfo.Investigations);
            GuidingLands = new WeaponUsageViewModel(rootViewModel, ViewType.GuidingLands, saveSlotInfo.GuidingLands);

            WeaponUsage all =
                saveSlotInfo.LowRank +
                saveSlotInfo.HighRank +
                saveSlotInfo.MasterRank +
                saveSlotInfo.Investigations +
                saveSlotInfo.GuidingLands;

            All = new WeaponUsageViewModel(rootViewModel, ViewType.All, all);
        }

        public void Dispose()
        {
            LowRank.Dispose();
            HighRank.Dispose();
            MasterRank.Dispose();
            Investigations.Dispose();
            GuidingLands.Dispose();
            All.Dispose();
        }
    }
}
