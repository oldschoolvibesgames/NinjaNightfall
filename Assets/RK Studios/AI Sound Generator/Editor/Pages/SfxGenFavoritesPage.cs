using System;
using System.Collections.Generic;
using System.IO;
using RK_Studios.AI_Sound_Generator.Editor.Components;
using RK_Studios.AI_Sound_Generator.Editor.Types;
using RK_Studios.AI_Sound_Generator.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Pages {
    public class SfxGenFavoritesPage : ISfxGenPage {
        private List<SoundEffect> _favorites;

        public void Reset() {
            _favorites = SfxGenDatabase.GetFavorites();
        }

        public void Draw(SfxGen window) {
            SfxGenPageLayout.DrawPage(
                window,
                () => {
                    SfxGenPageHeading.Draw("Favorites");
                    SfxGenPageSubHeading.Draw("Easy access to all the sound effects you've favorited.");
                },
                () => {
                    GUILayout.Space(10);

                    if (_favorites.Count == 0)
                        GUILayout.Label("You haven't favorited any sounds yet.", EditorStyles.helpBox);
                    else
                        foreach (var sound in _favorites) {
                            DrawSoundCard(sound);
                            GUILayout.Space(10);
                        }
                },
                () => { window.SetPage(SfxGenPage.Home); }
            );
        }

        private void DrawSoundCard(SoundEffect sound) {
            var isFavorited = SfxGenDatabase.IsFavorite(sound.id);
            var isUserGenerated = string.Equals(sound.category, "User Generated", StringComparison.OrdinalIgnoreCase);

            SfxGenSoundComponent.Draw(
                sound.name,
                $"{sound.category}",
                isFavorited,
                () => { SfxAudioPlayerUtility.PlaySound(sound.path); },
                () => {
                    SfxGenDatabase.ToggleFavorite(sound.id);
                    _favorites = SfxGenDatabase.GetFavorites();
                    EditorApplication.QueuePlayerLoopUpdate();
                },
                () => {
                    var settings = SfxGenSettingsUtility.LoadSettings();
                    var importPath = string.IsNullOrWhiteSpace(settings.importPath)
                        ? "Assets/RK Studios/Sound Effects"
                        : settings.importPath;

                    Directory.CreateDirectory(importPath);

                    var fileName = Path.GetFileName(sound.path);
                    var destPath = Path.Combine(importPath, fileName);

                    try {
                        File.Copy(sound.path, destPath, true);
                        AssetDatabase.ImportAsset(destPath);
                        EditorWindow.focusedWindow?.ShowNotification(
                            new GUIContent("Sound imported successfully!"));
                        Debug.Log($"Imported {fileName} to {importPath}");
                    }
                    catch (Exception ex) {
                        Debug.LogError($"Failed to import {fileName}: {ex.Message}");
                    }
                },
                isUserGenerated
                    ? () => {
                        var confirm = EditorUtility.DisplayDialog(
                            "Delete Sound",
                            $"Are you sure you want to delete '{sound.name}'?",
                            "Delete",
                            "Cancel"
                        );

                        if (!confirm) return;

                        if (File.Exists(sound.path))
                            try {
                                File.Delete(sound.path);
                                Debug.Log($"Deleted file: {sound.path}");
                            }
                            catch {
                                Debug.LogWarning($"Failed to delete file: {sound.path}");
                            }

                        SfxGenDatabase.DeleteUserSound(sound.id);

                        if (isFavorited)
                            SfxGenDatabase.ToggleFavorite(sound.id);

                        _favorites = SfxGenDatabase.GetFavorites();
                        EditorApplication.QueuePlayerLoopUpdate();
                    }
                    : null
            );
        }
    }
}