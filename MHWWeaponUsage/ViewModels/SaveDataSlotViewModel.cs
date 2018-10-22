using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHWWeaponUsage.ViewModels
{
    public class SaveDataSlotViewModel : ViewModelBase, IDisposable
    {
        public string Name { get; }
        public uint Rank { get; }
        public string Playtime { get; }

        public WeaponUsageViewModel LowRank { get; }
        public WeaponUsageViewModel HighRank { get; }
        public WeaponUsageViewModel Investigations { get; }
        public WeaponUsageViewModel Total { get; }

        public SaveDataSlotViewModel(RootViewModel rootViewModel, SaveSlotInfo saveSlotInfo)
        {
            Name = saveSlotInfo.Name;
            Rank = saveSlotInfo.Rank;

            uint playtimeSeconds = saveSlotInfo.Playtime % 60;
            uint playtimeMinutes = saveSlotInfo.Playtime / 60 % 60;
            uint playtimeHours = saveSlotInfo.Playtime / 3600;
            Playtime = $"{playtimeHours:d02}:{playtimeMinutes:d02}:{playtimeSeconds:d02}";

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
