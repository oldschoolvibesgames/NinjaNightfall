using System.IO;
using RK_Studios.AI_Sound_Generator.Editor.Types;
using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Utilities {
    public static class SfxGenSettingsUtility {
        private static readonly string SettingsPath =
            "Assets/RK Studios/AI Sound Generator/Editor/Data/settings.json";

        private static SfxGenUserSettings _current;
        public static SfxGenUserSettings Current => _current ?? LoadSettings();

        public static SfxGenUserSettings LoadSettings() {
            if (!File.Exists(SettingsPath)) {
                _current = new SfxGenUserSettings();
                SaveSettings(_current);
                return _current;
            }

            var json = File.ReadAllText(SettingsPath);
            _current = JsonUtility.FromJson<SfxGenUserSettings>(json);
            return _current;
        }

        public static void ReloadSettings() {
            _current = null;
            LoadSettings();
        }

        public static void SaveSettings(SfxGenUserSettings settings) {
            var json = JsonUtility.ToJson(settings, true);
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, json);
            AssetDatabase.Refresh();
            _current = settings;
        }
    }
}