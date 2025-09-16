using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using RK_Studios.AI_Sound_Generator.Editor.Components;
using RK_Studios.AI_Sound_Generator.Editor.Types;
using RK_Studios.AI_Sound_Generator.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Pages {
    public class SfxGenGeneratePage : ISfxGenPage {
        private readonly List<SoundEffect> _generatedSounds = new();
        private string _errorMessage;
        private string _generationPrompt = "";
        private bool _isGenerating;

        public void Draw(SfxGen window) {
            SfxGenPageLayout.DrawPage(
                window,
                () => {
                    SfxGenPageHeading.Draw("Generate Sounds");
                    SfxGenPageSubHeading.Draw("Welcome to AI AI Sound Generator");
                },
                () => {
                    GUILayout.Space(10);

                    var titleStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 16,
                        fontStyle = FontStyle.Bold,
                        normal = { textColor = Color.white }
                    };
                    GUILayout.Label("Description", titleStyle);

                    GUILayout.Space(8);

                    var helpStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 12,
                        wordWrap = true,
                        normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
                    };
                    GUILayout.Label(
                        "Enter a detailed description of the sound you want to generate. Example: “A futuristic robot voice saying hello”.",
                        helpStyle);

                    GUILayout.Space(8);

                    var textAreaStyle = new GUIStyle(EditorStyles.textArea) {
                        wordWrap = true,
                        fontSize = 12
                    };

                    GUI.SetNextControlName("GeneratePromptField");
                    EditorGUI.BeginDisabledGroup(_isGenerating);
                    _generationPrompt =
                        EditorGUILayout.TextArea(_generationPrompt, textAreaStyle, GUILayout.MinHeight(80));
                    EditorGUI.EndDisabledGroup();

                    GUILayout.Space(12);

                    EditorGUI.BeginDisabledGroup(_isGenerating);
                    var buttonLabel = _isGenerating ? "Generating..." : "Generate";
                    if (SfxGenPrimaryButton.Draw(buttonLabel)) {
                        _isGenerating = true;
                        _errorMessage = null;

                        var promptCopy = _generationPrompt;

                        SfxGenApiUtility.CreateSFX(promptCopy).ContinueWith(task => {
                            EditorApplication.delayCall += () => {
                                _generatedSounds.Clear();

                                if (task.Result.error != null) {
                                    _errorMessage = task.Result.error;
                                }
                                else {
                                    _generatedSounds.AddRange(task.Result.sounds);
                                    _generationPrompt = "";
                                }

                                _isGenerating = false;
                                GUI.FocusControl(null);
                            };
                        });
                    }

                    EditorGUI.EndDisabledGroup();

                    if (!string.IsNullOrEmpty(_errorMessage)) {
                        GUILayout.Space(12);
                        SfxGenErrorComponent.DrawError(_errorMessage);
                    }

                    GUILayout.Space(20);

                    if (_generatedSounds.Count > 0) {
                        foreach (var sound in _generatedSounds) {
                            DrawSoundCard(sound);
                            GUILayout.Space(10);
                        }

                        GUILayout.Space(20);
                    }

                    var exampleLabelStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 13,
                        fontStyle = FontStyle.Italic,
                        normal = { textColor = new Color(0.8f, 0.8f, 0.8f) }
                    };
                    GUILayout.Label("Try examples like:", exampleLabelStyle);
                    GUILayout.Space(4);
                    DrawExamplePrompt("A cartoon laser gun firing");
                    DrawExamplePrompt("Thunder crack with deep rumble");
                    DrawExamplePrompt("Voice saying 'Welcome to the jungle' with echo");
                    DrawExamplePrompt("Glass shatter sound effect");
                    DrawExamplePrompt("Creepy ambient wind blowing");

                    GUILayout.Space(20);
                },
                () => { window.SetPage(SfxGenPage.Home); }
            );
        }

        public void Reset() {
            _generationPrompt = "";
            _generatedSounds.Clear();
            _isGenerating = false;
            _errorMessage = null;
            EditorGUIUtility.editingTextField = false;
            GUI.FocusControl(null);
        }

        private void DrawSoundCard(SoundEffect sound) {
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
                    }
                    catch (Exception ex) {
                        Debug.LogError($"Failed to import {fileName}: {ex.Message}");
                    }
                }
            );
        }

        private void DrawExamplePrompt(string text) {
            var style = new GUIStyle(EditorStyles.linkLabel) {
                fontSize = 12
            };

            if (GUILayout.Button(text, style)) {
                _generationPrompt = text;
                GUI.FocusControl(null);
            }
        }

        private static string MakeSafeFileName(string text) {
            text = text.ToLowerInvariant();
            foreach (var c in Path.GetInvalidFileNameChars())
                text = text.Replace(c.ToString(), "");
            text = text.Replace(" ", "_");
            text = Regex.Replace(text, @"[^a-z0-9_]", "_");
            text = Regex.Replace(text, @"_+", "_");
            return text.Trim('_');
        }
    }
}