using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RK_Studios.AI_Sound_Generator.Editor.Types;
using Unity.Plastic.Newtonsoft.Json;

namespace RK_Studios.AI_Sound_Generator.Editor.Utilities {
    public static class SfxGenDatabase {
        private static List<SoundEffect> _librarySounds;
        private static List<SoundEffect> _userSounds;
        private static HashSet<string> _favoriteIds;
        private static HashSet<string> _topIds;

        // Paths
        private static readonly string LibraryPath =
            "Assets/RK Studios/AI Sound Generator/Editor/Data/library.json";

        private static readonly string UserPath =
            "Assets/RK Studios/AI Sound Generator/Editor/Data/user_sounds.json";

        private static readonly string FavoritesPath =
            "Assets/RK Studios/AI Sound Generator/Editor/Data/favorites.json";

        private static readonly string TopPath =
            "Assets/RK Studios/AI Sound Generator/Editor/Data/top.json";

        public static void LoadAll() {
            _librarySounds = LoadFromFile(LibraryPath);
            _userSounds = LoadFromFile(UserPath);
            _favoriteIds = LoadFavorites(FavoritesPath);

            // NEW: Load top IDs
            _topIds = LoadTopIds(TopPath);
        }

        public static void ReloadUserSounds() {
            _userSounds = LoadFromFile(UserPath);
        }

        private static List<SoundEffect> LoadFromFile(string path) {
            if (!File.Exists(path)) return new List<SoundEffect>();
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<List<SoundEffect>>(json);
        }

        private static HashSet<string> LoadFavorites(string path) {
            if (!File.Exists(path)) return new HashSet<string>();
            var json = File.ReadAllText(path);
            var list = JsonConvert.DeserializeObject<List<string>>(json);
            return new HashSet<string>(list);
        }

        // NEW: Equivalent of LoadFavorites, but for top.json
        private static HashSet<string> LoadTopIds(string path) {
            if (!File.Exists(path)) return new HashSet<string>();
            var json = File.ReadAllText(path);
            var list = JsonConvert.DeserializeObject<List<string>>(json);
            return new HashSet<string>(list);
        }

        public static List<SoundEffect> GetAllSounds() {
            return _librarySounds.Concat(_userSounds).ToList();
        }

        public static List<SoundEffect> GetSoundsInCategory(string category) {
            return GetAllSounds()
                .Where(s => string.Equals(s.category, category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public static List<SoundEffect> GetUserSounds() {
            return _userSounds;
        }

        public static List<SoundEffect> GetFavorites() {
            return GetAllSounds()
                .Where(s => _favoriteIds.Contains(s.id))
                .ToList();
        }

        // NEW: Returns a list of SoundEffect objects whose IDs appear in top.json
        public static List<SoundEffect> GetTopSounds() {
            return GetAllSounds()
                .Where(s => _topIds.Contains(s.id))
                .ToList();
        }

        public static List<SoundEffect> SearchByName(string searchTerm) {
            if (string.IsNullOrWhiteSpace(searchTerm)) return new List<SoundEffect>();
            return GetAllSounds()
                .Where(s => s.name.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
        }

        public static bool IsFavorite(string id) {
            return _favoriteIds.Contains(id);
        }

        public static void ToggleFavorite(string id) {
            if (_favoriteIds.Contains(id))
                _favoriteIds.Remove(id);
            else
                _favoriteIds.Add(id);

            SaveFavorites();
        }

        private static void SaveFavorites() {
            var list = _favoriteIds.ToList();
            File.WriteAllText(FavoritesPath, JsonConvert.SerializeObject(list, Formatting.Indented));
        }

        public static void DeleteUserSound(string id) {
            if (_userSounds == null) return;

            _userSounds = _userSounds.Where(s => s.id != id).ToList();

            var json = JsonConvert.SerializeObject(_userSounds, Formatting.Indented);
            File.WriteAllText(UserPath, json);
        }
    }
}