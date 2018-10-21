using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHWWeaponUsage
{
    public class WeaponUsageReader : IDisposable
    {
        private readonly BinaryReader reader;
        private readonly long saveDataLength;

        public WeaponUsageReader(Stream saveData)
        {
            if (saveData == null)
                throw new ArgumentNullException(nameof(saveData));

            if (saveData.CanRead == false)
                throw new ArgumentException($"Argument '{nameof(saveData)}' must be readable, but is not");
            if (saveData.CanSeek == false)
                throw new ArgumentException($"Argument '{nameof(saveData)}' must be seekable, but is not");

            // This call is very costly, so better cache it
            saveDataLength = saveData.Length;

            reader = new BinaryReader(saveData, Encoding.ASCII, true);
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        public const uint Section3Signature = 0xAD35B985;

        public IEnumerable<SaveSlotInfo> Read()
        {
            reader.BaseStream.Position =
                64 + // header
                8 * 3 // 3 first sectionIndex
            ;

            long section3Offset = reader.ReadInt64();

            if (section3Offset < 0 || section3Offset >= saveDataLength)
                throw new FormatException($"Invalid section 3 offset ({section3Offset})");

            reader.BaseStream.Position = section3Offset;

            // Here is the section 3

            uint section3Signature = reader.ReadUInt32();
            if (section3Signature != Section3Signature)
                throw new FormatException($"Invalid section 3 signature, expected {Section3Signature:X8} but read {section3Signature:X8}");

            Skip(
                4 + // unknown
                8 + // sectionSize
                4 // sectionData_3.unknown
            );

            for (int i = 0; i < 3; i++)
            {
                SaveSlotInfo saveSlotInfo = ReadSaveSlot();
                yield return saveSlotInfo;
            }
        }

        private const long WeaponUsageStructureSize = 14 * 2;
        private const long HunterEquipmentStructureSize = 18 * 4;
        private const long PalicoEquipmentStructureSize = 8 * 4;
        private const long PalicoStructureSize = 119 + PalicoEquipmentStructureSize;
        private const long ArenaRecordStructureSize = 60;
        private const long ArenaStatsStructSize = 2 + ArenaRecordStructureSize * 5;
        private const long Creatures8StructSize = 64;
        private const long Creatures16StructSize = 64 * 2;
        private const long GuildCardStructureSize =
            1020 +
            HunterEquipmentStructureSize +
            PalicoStructureSize +
            3 * WeaponUsageStructureSize +
            10 * ArenaStatsStructSize +
            4 * Creatures16StructSize +
            Creatures8StructSize;

        private const long ItemLoadoutStructureSize = 1128;
        private const long ItemLoadoutsStructureSize = ItemLoadoutStructureSize * 56 + 56;
        private const long ItemPouchStructureSize = 24 * 8 + 16 * 8 + 256 + 4 * 8;
        private const long ItemBoxStructureSize = 8 * 200 + 8 * 200 + 8 * 800 + 8 * 200;
        private const long EquipLoadoutStructureSize = 544;
        private const long EquipLoadoutsStructureSize = EquipLoadoutStructureSize * 112;
        private const long DlcTypeSize = 2;

        private SaveSlotInfo ReadSaveSlot()
        {
            byte[] hunterNameBytes = reader.ReadBytes(64);
            string hunterName = Encoding.UTF8.GetString(hunterNameBytes);

            uint hunterRank = reader.ReadUInt32();

            Skip(
                4 + // zeni
                4 + // researchPoints
                4 + // hunterXP
                4 + // playTime_s
                4 + // unknown
                120 + // H_APPEARANCE
                44 // P_APPEARANCE
            );

            // Here is struct GUILDCARD

            long startOffset = reader.BaseStream.Position;

            Skip(
                167 + // begining of GUILDCARD struct
                120 + // hunterAppearance (H_APPEARANCE)
                44 + // palicoAppearance (P_APPEARANCE)
                18 * 4 + // hunterEquipment
                92 + // unknown
                151 + // struct palico
                63 // remaining of the struct GUILDCARD until weapon usage
            );

            var lowRankWeaponUsage = WeaponUsage.Read(reader);
            var highRankWeaponUsage = WeaponUsage.Read(reader);
            var investigationsWeaponUsage = WeaponUsage.Read(reader);

            // Skip the remaining of the GUILDCARD structure
            Skip(
                1 + // poseID
                1 + // expressionID
                1 + // backgroundID
                1 + // stickerID
                256 + // greeting
                256 + // title
                2 + // titleFirst
                2 + // titleMiddle
                2 + // titleLast
                4 + // positionX
                4 + // positionY
                4 + // zoom
                10 * ArenaStatsStructSize + // arenaRecords
                4 *  Creatures16StructSize + // creatureStats
                Creatures8StructSize // researchLevel
            );

            long guildCardStructureLength = reader.BaseStream.Position - startOffset;

            // Skip the remaining of the saveSlot structure
            Skip(
                GuildCardStructureSize * 100 + // sharedGC
                0x019e36 + // unknown
                ItemLoadoutsStructureSize + // itemLoadouts
                8 + //  unknown
                ItemPouchStructureSize + // itemPouch
                ItemBoxStructureSize + // itemBox
                0x034E3C + // unknown
                42 * 250 + // investigations
                0x0FB9 + // unknown
                EquipLoadoutsStructureSize + // equipLoadout
                0x6521 + // unknown
                DlcTypeSize * 256 + // DLCClaimed
                0x2A5D // unknown
            );

            guildCardStructureLength = reader.BaseStream.Position - startOffset;

            return new SaveSlotInfo(
                hunterName,
                hunterRank,
                lowRankWeaponUsage,
                highRankWeaponUsage,
                investigationsWeaponUsage
            );
        }

        private void Skip(long count)
        {
            reader.BaseStream.Seek(count, SeekOrigin.Current);
        }
    }

    public class SaveSlotInfo
    {
        public string Name { get; }
        public uint Rank { get; }

        public WeaponUsage LowRank { get; }
        public WeaponUsage HighRank { get; }
        public WeaponUsage Investigations { get; }

        public SaveSlotInfo(
            string name, uint rank,
            WeaponUsage lowRank, WeaponUsage highRank, WeaponUsage investigations)
        {
            Name = name;
            Rank = rank;
            LowRank = lowRank;
            HighRank = highRank;
            Investigations = investigations;
        }
    }

#pragma warning disable CA1815 // Override equals and operator equals on value types
    public struct WeaponUsage
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        public ushort GreatSword { get; private set; }
        public ushort LongSword { get; private set; }
        public ushort SwordAndShield { get; private set; }
        public ushort DualBlades { get; private set; }
        public ushort Hammer { get; private set; }
        public ushort HuntingHorn { get; private set; }
        public ushort Lance { get; private set; }
        public ushort Gunlance { get; private set; }
        public ushort SwitchAxe { get; private set; }
        public ushort ChargeBlade { get; private set; }
        public ushort InsectGlaive { get; private set; }
        public ushort LightBowgun { get; private set; }
        public ushort HeavyBowgun { get; private set; }
        public ushort Bow { get; private set; }

        public static WeaponUsage Read(BinaryReader reader)
        {
            return new WeaponUsage
            {
                GreatSword = reader.ReadUInt16(),
                LongSword = reader.ReadUInt16(),
                SwordAndShield = reader.ReadUInt16(),
                DualBlades = reader.ReadUInt16(),
                Hammer = reader.ReadUInt16(),
                HuntingHorn = reader.ReadUInt16(),
                Lance = reader.ReadUInt16(),
                Gunlance = reader.ReadUInt16(),
                SwitchAxe = reader.ReadUInt16(),
                ChargeBlade = reader.ReadUInt16(),
                InsectGlaive = reader.ReadUInt16(),
                LightBowgun = reader.ReadUInt16(),
                HeavyBowgun = reader.ReadUInt16(),
                Bow = reader.ReadUInt16()
            };
        }

        public static WeaponUsage operator +(WeaponUsage lhs, WeaponUsage rhs)
        {
            return new WeaponUsage
            {
                GreatSword = (ushort)(lhs.GreatSword + rhs.GreatSword),
                LongSword = (ushort)(lhs.LongSword + rhs.LongSword),
                SwordAndShield = (ushort)(lhs.SwordAndShield + rhs.SwordAndShield),
                DualBlades = (ushort)(lhs.DualBlades + rhs.DualBlades),
                Hammer = (ushort)(lhs.Hammer + rhs.Hammer),
                HuntingHorn = (ushort)(lhs.HuntingHorn + rhs.HuntingHorn),
                Lance = (ushort)(lhs.Lance + rhs.Lance),
                Gunlance = (ushort)(lhs.Gunlance + rhs.Gunlance),
                SwitchAxe = (ushort)(lhs.SwitchAxe + rhs.SwitchAxe),
                ChargeBlade = (ushort)(lhs.ChargeBlade + rhs.ChargeBlade),
                InsectGlaive = (ushort)(lhs.InsectGlaive + rhs.InsectGlaive),
                LightBowgun = (ushort)(lhs.LightBowgun + rhs.LightBowgun),
                HeavyBowgun = (ushort)(lhs.HeavyBowgun + rhs.HeavyBowgun),
                Bow = (ushort)(lhs.Bow + rhs.Bow)
            };
        }
    }
}
