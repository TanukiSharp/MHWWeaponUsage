using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHWWeaponUsage.ViewModels
{
    public class SaveDataSlotViewModel : ViewModelBase
    {
        public string Name { get; }
        public uint Rank { get; }
        public string Playtime { get; }

        public WeaponUsage LowRank { get; }
        public WeaponUsage HighRank { get; }
        public WeaponUsage Investigations { get; }
        public WeaponUsage Total { get; }

        public SaveDataSlotViewModel(SaveSlotInfo saveSlotInfo)
        {
            Name = saveSlotInfo.Name;
            Rank = saveSlotInfo.Rank;

            uint playtimeSeconds = saveSlotInfo.Playtime % 60;
            uint playtimeMinutes = saveSlotInfo.Playtime / 60 % 60;
            uint playtimeHours = saveSlotInfo.Playtime / 3600;
            Playtime = $"{playtimeHours:d02}:{playtimeMinutes:d02}:{playtimeSeconds:d02}";

            LowRank = saveSlotInfo.LowRank;
            HighRank = saveSlotInfo.HighRank;
            Investigations = saveSlotInfo.Investigations;

            Total = LowRank + HighRank + Investigations;
        }
    }
}
