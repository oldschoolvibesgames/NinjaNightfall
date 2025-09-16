using System;
using System.Collections.Generic;
using System.IO;
using RK_Studios.AI_Sound_Generator.Editor.Components;
using RK_Studios.AI_Sound_Generator.Editor.Types;
using RK_Studios.AI_Sound_Generator.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Pages {
    public class SfxGenSearchPage : ISfxGenPage {
        private const float SearchDelay = 0.5f;
        private double _lastEditTime = -10;
        private List<SoundEffect> _results = new();
        private string _searchQuery = "";

        public void Reset() {
            _searchQuery = "";
            _results.Clear();
            _lastEditTime = -10;
        }

        public void Draw(SfxGen window) {
            SfxGenPageLayout.DrawPage(
                window,
                () => {
                    SfxGenPageHeading.Draw("Search");
                    SfxGenPageSubHeading.Draw("Find sound effects in your library quickly.");

                    GUILayout.Space(8);

                    var inputStyle = new GUIStyle(EditorStyles.textField) {
                        fontSize = 12,
                        fixedHeight = 26,
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset(8, 8, 0, 0)
                    };

                    EditorGUI.BeginChangeCheck();
                    _searchQuery = EditorGUILayout.TextField("", _searchQuery, inputStyle, GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                        _lastEditTime = EditorApplication.timeSinceStartup;

                    GUILayout.Space(8);
                },
                () => {
                    GUILayout.Space(12);
                    var noResultsStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 14,
                        wordWrap = true,
                        alignment = TextAnchor.MiddleCenter
                    };
                    var timeSinceLastEdit = EditorApplication.timeSinceStartup - _lastEditTime;
                    if (_searchQuery.Length >= 3 && timeSinceLastEdit > SearchDelay)
                        _results = SfxGenDatabase.SearchByName(_searchQuery);
                    else
                        _results.Clear();

                    if (string.IsNullOrWhiteSpace(_searchQuery) || _searchQuery.Length < 3) {
                        // GUILayout.Space(40);
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();


                        GUILayout.Label("Sound effects will appear here once you search something 3+ characters above.",
                            noResultsStyle);
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        return;
                    }

                    if (_results.Count == 0) {
                        GUILayout.Label("No results found.", noResultsStyle);
                        return;
                    }

                    foreach (var sound in _results) {
                        var isFavorited = SfxGenDatabase.IsFavorite(sound.id);

                        SfxGenSoundComponent.Draw(
                            sound.name,
                            $"{sound.category}",
                            isFavorited,
                            () => { SfxAudioPlayerUtility.PlaySound(sound.path); },
                            () => {
                                SfxGenDatabase.ToggleFavorite(sound.id);
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
                            }
                        );

                        GUILayout.Space(10);
                    }
                },
                () => { window.SetPage(SfxGenPage.Home); }
            );
        }
    }
}