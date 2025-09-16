using System;
using System.Collections.Generic;
using System.IO;
using RK_Studios.AI_Sound_Generator.Editor.Components;
using RK_Studios.AI_Sound_Generator.Editor.State;
using RK_Studios.AI_Sound_Generator.Editor.Types;
using RK_Studios.AI_Sound_Generator.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Pages {
    public class SfxGenSoundEffectsPage : ISfxGenPage {
        private string _category;
        private List<SoundEffect> _categorySounds;

        public void Reset() {
            _category = SfxGenState.SelectedCategory;
            _categorySounds = SfxGenDatabase.GetSoundsInCategory(_category);
        }

        public void Draw(SfxGen window) {
            SfxGenPageLayout.DrawPage(
                window,
                () => {
                    SfxGenPageHeading.Draw(_category + " Sound Effects");
                    SfxGenPageSubHeading.Draw("Browse sounds in this category.");
                },
                () => {
                    GUILayout.Space(10);

                    foreach (var sound in _categorySounds) {
                        DrawSoundCard(sound);
                        GUILayout.Space(10);
                    }

                    if (_categorySounds.Count == 0) {
                        var headerStyle = new GUIStyle(EditorStyles.boldLabel) {
                            fontSize = 14,
                            alignment = TextAnchor.MiddleCenter,
                            normal = { textColor = Color.white },
                            hover = { textColor = Color.white }
                        };

                        var textStyle = new GUIStyle(EditorStyles.label) {
                            fontSize = 12,
                            wordWrap = true,
                            alignment = TextAnchor.MiddleCenter,
                            normal = { textColor = Color.gray },
                            hover = { textColor = Color.gray }
                        };

                        GUILayout.Space(20);
                        GUILayout.Label("No Sound Effects", headerStyle);
                        GUILayout.Space(6);
                        GUILayout.Label(
                            "Looks like you haven't generated any sound effects yet.\nClick the button below to create some.",
                            textStyle);

                        GUILayout.Space(12);

                        // Centered button
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();

                        var buttonStyle = new GUIStyle(GUI.skin.button) {
                            fontSize = 13,
                            fixedHeight = 32,
                            fixedWidth = 220,
                            alignment = TextAnchor.MiddleCenter,
                            padding = new RectOffset(12, 12, 4, 4)
                        };

                        if (GUILayout.Button("Generate Sound Effects", buttonStyle))
                            window.SetPage(SfxGenPage.Generate);

                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                },
                () => { window.SetPage(SfxGenPage.SoundEffectCategories); }
            );
        }

        private void DrawSoundCard(SoundEffect soundEffect) {
            var isFavorited = SfxGenDatabase.IsFavorite(soundEffect.id);
            var isUserGenerated =
                string.Equals(soundEffect.category, "User Generated", StringComparison.OrdinalIgnoreCase);

            SfxGenSoundComponent.Draw(
                soundEffect.name,
                $"{soundEffect.category}",
                isFavorited,
                () => { SfxAudioPlayerUtility.PlaySound(soundEffect.path); },
                () => {
                    SfxGenDatabase.ToggleFavorite(soundEffect.id);
                    EditorApplication.QueuePlayerLoopUpdate();
                },
                () => {
                    var settings = SfxGenSettingsUtility.LoadSettings();
                    var importPath = string.IsNullOrWhiteSpace(settings.importPath)
                        ? "Assets/RK Studios/Sound Effects"
                        : settings.importPath;

                    Directory.CreateDirectory(importPath);

                    var fileName = Path.GetFileName(soundEffect.path);
                    var destPath = Path.Combine(importPath, fileName);

                    try {
                        File.Copy(soundEffect.path, destPath, true);
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
                            $"Are you sure you want to delete '{soundEffect.name}'?",
                            "Delete",
                            "Cancel"
                        );

                        if (!confirm) return;

                        if (File.Exists(soundEffect.path))
                            try {
                                File.Delete(soundEffect.path);
                                Debug.Log($"Deleted file: {soundEffect.path}");
                            }
                            catch {
                                Debug.LogWarning($"Failed to delete file: {soundEffect.path}");
                            }

                        SfxGenDatabase.DeleteUserSound(soundEffect.id);
                        if (isFavorited)
                            SfxGenDatabase.ToggleFavorite(soundEffect.id);

                        Reset();
                        EditorApplication.QueuePlayerLoopUpdate();
                    }
                    : null
            );
        }
    }
}