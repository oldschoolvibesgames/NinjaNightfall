using System;
using System.Collections.Generic;
using System.IO;
using RK_Studios.AI_Sound_Generator.Editor.Components;
using RK_Studios.AI_Sound_Generator.Editor.Types;
using RK_Studios.AI_Sound_Generator.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Pages {
    public class SfxGenTopPage : ISfxGenPage {
        // Renamed: now stores the “top” sounds
        private List<SoundEffect> _topSounds;

        public void Reset() {
            // Pull the top sound effects from the database
            _topSounds = SfxGenDatabase.GetTopSounds();
        }

        public void Draw(SfxGen window) {
            SfxGenPageLayout.DrawPage(
                window,
                // Header area
                () => {
                    SfxGenPageHeading.Draw("Top Sound Effects");
                    SfxGenPageSubHeading.Draw("Check out the best from the library.");
                },

                // Main content area
                () => {
                    GUILayout.Space(10);

                    if (_topSounds.Count == 0)
                        GUILayout.Label("There are no top sounds yet.", EditorStyles.helpBox);
                    else
                        foreach (var sound in _topSounds) {
                            DrawSoundCard(sound);
                            GUILayout.Space(10);
                        }


                    if (SfxGenPrimaryButton.Draw("View all sound effects"))
                        window.SetPage(SfxGenPage.SoundEffectCategories);
                },

                // “Back” or “Home” button
                () => { window.SetPage(SfxGenPage.Home); }
            );
        }

        private void DrawSoundCard(SoundEffect sound) {
            // You can decide whether you still want the 'favorite' logic here
            var isFavorited = SfxGenDatabase.IsFavorite(sound.id);
            var isUserGenerated = string.Equals(sound.category, "User Generated", StringComparison.OrdinalIgnoreCase);

            SfxGenSoundComponent.Draw(
                sound.name,
                $"{sound.category}",
                isFavorited,
                // onPlayPressed
                () => { SfxAudioPlayerUtility.PlaySound(sound.path); },

                // onFavoritePressed
                () => {
                    SfxGenDatabase.ToggleFavorite(sound.id);
                    // If you want to update the list upon favoriting, re-fetch top sounds
                    _topSounds = SfxGenDatabase.GetTopSounds();
                    EditorApplication.QueuePlayerLoopUpdate();
                },

                // onImportPressed
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

                // onDeletePressed (only if user generated)
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

                        // Refresh the top sounds if needed
                        _topSounds = SfxGenDatabase.GetTopSounds();
                        EditorApplication.QueuePlayerLoopUpdate();
                    }
                    : null
            );
        }
    }
}