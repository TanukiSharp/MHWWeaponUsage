using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHWSaveUtils
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

        public IEnumerable<WeaponUsageSaveSlotInfo> Read()
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
            if (section3Signature != Constants.Section3Signature)
                throw new FormatException($"Invalid section 3 signature, expected {Constants.Section3Signature:X8} but read {section3Signature:X8}");

            Skip(
                4 + // unknown
                8 + // sectionSize
                4 // sectionData_3.unknown
            );

            for (int i = 0; i < 3; i++)
            {
                WeaponUsageSaveSlotInfo saveSlotInfo = ReadSaveSlot();
                if (saveSlotInfo != null)
                    yield return saveSlotInfo;
            }
        }

        // Slot 0 Active @ 0x3F3D64
        // Slot 1 Active @ 0x4E9E74
        // Slot 2 Active @ 0x5DFF84

        private WeaponUsageSaveSlotInfo ReadSaveSlot()
        {
            byte[] hunterNameBytes = reader.ReadBytes(64);
            string hunterName = Encoding.UTF8.GetString(hunterNameBytes).TrimEnd('\0');

            uint hunterRank = reader.ReadUInt32();

            Skip(
                4 + // zeni
                4 + // researchPoints
                4 // hunterXP
            );

            uint playTime = reader.ReadUInt32();

            Skip(
                4 + // unknown
                120 + // H_APPEARANCE
                44 // P_APPEARANCE
            );

            // Here is struct GUILDCARD

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
                10 * Constants.ArenaStatsStructSize + // arenaRecords
                4 * Constants.Creatures16StructSize + // creatureStats
                Constants.Creatures8StructSize // researchLevel
            );

            // Skip the remaining of the saveSlot structure
            Skip(
                Constants.GuildCardStructureSize * 100 + // sharedGC
                0x019e36 + // unknown
                Constants.ItemLoadoutsStructureSize + // itemLoadouts
                8 + //  unknown
                Constants.ItemPouchStructureSize + // itemPouch
                Constants.ItemBoxStructureSize + // itemBox
                0x034E3C + // unknown
                42 * 250 + // investigations
                0x0FB9 + // unknown
                Constants.EquipLoadoutsStructureSize + // equipLoadout
                0x6521 + // unknown
                Constants.DlcTypeSize * 256 + // DLCClaimed
                0x2A5D // unknown
            );

            if (playTime == 0)
                return null;

            return new WeaponUsageSaveSlotInfo(
                hunterName,
                hunterRank,
                playTime,
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

    public class WeaponUsageSaveSlotInfo
    {
        public string Name { get; }
        public uint Rank { get; }
        public uint Playtime { get; }

        public WeaponUsage LowRank { get; }
        public WeaponUsage HighRank { get; }
        public WeaponUsage Investigations { get; }

        public WeaponUsageSaveSlotInfo(
            string name, uint rank, uint playtime,
            WeaponUsage lowRank, WeaponUsage highRank, WeaponUsage investigations)
        {
            Name = name;
            Rank = rank;
            Playtime = playtime;
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
