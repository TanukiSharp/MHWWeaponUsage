﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace MHWWeaponUsage
{
    public struct GameSaveInfo
    {
        public string UserId { get; }
        public string SaveDataFullFilename { get; }

        public GameSaveInfo(string userId, string saveDataFullFilename)
        {
            if (userId == null)
                throw new ArgumentNullException(nameof(userId));
            if (saveDataFullFilename == null)
                throw new ArgumentNullException(nameof(saveDataFullFilename));

            UserId = userId;
            SaveDataFullFilename = saveDataFullFilename;
        }
    }

    public static class Utils
    {
        // Code copied from this repository: git@github.com:Nexusphobiker/MHWSaveEditor.git

        public const string GameId = "582010";
        public const string GameSaveDataFilename = "SAVEDATA1000";
        public static readonly string SteamPath;

        static Utils()
        {
            SteamPath = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null);
        }

        public static IEnumerable<GameSaveInfo> EnumerateGameSaveDataInfo()
        {
            if (SteamPath == null)
                yield break;

            string userDataPath = Path.Combine(SteamPath, "userdata");

            foreach (string userDirectory in Directory.GetDirectories(userDataPath))
            {
                foreach (string gameDirectory in Directory.GetDirectories(userDirectory))
                {
                    if (Path.GetFileName(gameDirectory) == GameId)
                    {
                        string saveDataFullFilename = Path.GetFullPath(Path.Combine(gameDirectory, "remote", GameSaveDataFilename));

                        if (File.Exists(saveDataFullFilename))
                        {
                            string userId = Path.GetFileName(userDirectory);
                            yield return new GameSaveInfo(userId, saveDataFullFilename);
                        }
                    }
                }
            }
        }
    }
}
