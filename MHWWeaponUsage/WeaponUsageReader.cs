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

        public void Read()
        {
            reader.BaseStream.Position = 64 + 8 * 3; // skip header (64) + skip 3 first sections (8 * 3)

            long section3Offset = reader.ReadInt64();

            if (section3Offset < 0 || section3Offset >= saveDataLength)
                throw new FormatException($"Invalid section 3 offset ({section3Offset})");

            reader.BaseStream.Position = section3Offset;

            uint section3Signature = reader.ReadUInt32();
            if (section3Signature != Section3Signature)
                throw new FormatException($"Invalid section 3 signature, expected {Section3Signature:X8} but read {section3Signature:X8}");

            Skip(4); // skip section.unknown (4)

            long sectionSize = reader.ReadInt64();

            Skip(92 + 120 + 44); // skip to hunterAppearance (92) + H_APPEARANCE (120) + P_APPEARANCE (44)

            // Here is struct GUILDCARD

            ulong steamId = reader.ReadUInt64();

            //reader.BaseStream.Seek(159 + 120 + 44 + 18 * 4 + 92 + 151 + 63, SeekOrigin.Current); // begining of GUILDCARD struct (159) + H_APPEARANCE (120) + P_APPEARANCE (44) + hunterEquipment (18 * 4) + unknown (92) + struct palico (151) + remaining of the struct GUILDCARD until weapon usage (63)

            reader.BaseStream.Seek(9, SeekOrigin.Current);
            uint hunterRank = reader.ReadUInt32();
            reader.BaseStream.Seek(12, SeekOrigin.Current);

            byte[] hunterName = reader.ReadBytes(64);
            string hunterNameStr = Encoding.ASCII.GetString(hunterName);

            byte[] primaryGroup = reader.ReadBytes(54);
            string primaryGroupStr = Encoding.ASCII.GetString(primaryGroup);

            Skip(16); // unknown
            Skip(120); // H_APPEARANCE
            Skip(44); // P_APPEARANCE
            Skip(18 * 4); // hunterEquipment
            Skip(92); // blob 0x5C

            byte[] palicoName = reader.ReadBytes(64);
            string palicoNameStr = Encoding.UTF8.GetString(palicoName);

            //Skip(20 * 4 + 1);
            //byte[] palicoGadgets = reader.ReadBytes(6);

            uint palicoRank_Minus_1 = reader.ReadUInt32();
            uint palicoHealth = reader.ReadUInt32();
            uint palicoAttM = reader.ReadUInt32();
            uint palicoAttR = reader.ReadUInt32();
            uint palicoAffinity = reader.ReadUInt32();
            uint palicoDef = reader.ReadUInt32();
            int palicoVsFire = reader.ReadInt32();
            int palicoVsWater = reader.ReadInt32();
            int palicoVsThunder = reader.ReadInt32();
            int palicoVsIce = reader.ReadInt32();
            int palicoVsDragon = reader.ReadInt32();
            byte unknown = reader.ReadByte();

            //struct palicoEquipment
            //{
            uint palicoWeaponType = reader.ReadUInt32();
            uint palicoWeaponID = reader.ReadUInt32();
            uint palicoHeadArmorType = reader.ReadUInt32();
            uint palicoHeadArmorID = reader.ReadUInt32();
            uint palicoBodyArmorType = reader.ReadUInt32();
            uint palicoBodyArmorID = reader.ReadUInt32();
            uint palicoGadgetType = reader.ReadUInt32();
            uint palicoGadgetID = reader.ReadUInt32();
            //};
            byte[] unknown_ = reader.ReadBytes(4);
            byte palicoG1 = reader.ReadByte();
            byte palicoG2 = reader.ReadByte();
            byte palicoG3 = reader.ReadByte();
            byte palicoG4 = reader.ReadByte();
            byte palicoG5 = reader.ReadByte();
            byte palicoG6 = reader.ReadByte();





            //var lowRank = WeaponUsage.Read(reader);
            //var highRank = WeaponUsage.Read(reader);
            //var investigations = WeaponUsage.Read(reader);

            //WeaponUsage totalWeaponUsage = lowRank + highRank + investigations;
        }

        private void Skip(long count)
        {
            reader.BaseStream.Seek(count, SeekOrigin.Current);
        }
    }

    public struct WeaponUsage
    {
        public ushort GreatSword;
        public ushort LongSword;
        public ushort SwordAndShield;
        public ushort DualBlades;
        public ushort Hammer;
        public ushort HuntingHorn;
        public ushort Lance;
        public ushort Gunlance;
        public ushort SwitchAxe;
        public ushort ChargeBlade;
        public ushort InsectGlaive;
        public ushort LightBowgun;
        public ushort HeavyBowgun;
        public ushort Bow;

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
