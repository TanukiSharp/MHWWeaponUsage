using System;
using System.Collections.Generic;
using System.Text;

namespace MHWSaveUtils
{
    public static class Constants
    {
        public const uint Section3Signature = 0xAD35B985;

        public const long WeaponUsageStructureSize = 14 * 2;
        public const long HunterEquipmentStructureSize = 18 * 4;
        public const long PalicoEquipmentStructureSize = 8 * 4;
        public const long PalicoStructureSize = 119 + PalicoEquipmentStructureSize;
        public const long ArenaRecordStructureSize = 60;
        public const long ArenaStatsStructSize = 2 + ArenaRecordStructureSize * 5;
        public const long Creatures8StructSize = 64;
        public const long Creatures16StructSize = 64 * 2;
        public const long GuildCardStructureSize =
            1020 +
            HunterEquipmentStructureSize +
            PalicoStructureSize +
            3 * WeaponUsageStructureSize +
            10 * ArenaStatsStructSize +
            4 * Creatures16StructSize +
            Creatures8StructSize;

        public const long ItemLoadoutStructureSize = 1128;
        public const long ItemLoadoutsStructureSize = ItemLoadoutStructureSize * 56 + 56;
        public const long ItemPouchStructureSize = 24 * 8 + 16 * 8 + 256 + 4 * 8;
        public const long ItemBoxStructureSize = 8 * 200 + 8 * 200 + 8 * 800 + 8 * 200;
        public const long EquipLoadoutStructureSize = 544;
        public const long EquipLoadoutsStructureSize = EquipLoadoutStructureSize * 112;
        public const long DlcTypeSize = 2;
    }
}
