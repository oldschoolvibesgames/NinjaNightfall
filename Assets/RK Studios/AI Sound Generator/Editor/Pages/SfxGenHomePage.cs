using System;
using System.IO;
using System.Linq;
using RK_Studios.AI_Sound_Generator.Editor.Components;
using RK_Studios.AI_Sound_Generator.Editor.Types;
using RK_Studios.AI_Sound_Generator.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Pages {
    public class SfxGenHomePage : ISfxGenPage {
        private Texture2D _videoThumbnail;

        public void Reset() {
            SfxGenDatabase.LoadAll();
            _videoThumbnail = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/RK Studios/AI Sound Generator/Editor/Images/thumbnail.png"
            );
        }

        public void Draw(SfxGen window) {
            if (_videoThumbnail == null) Reset();

            SfxGenPageLayout.DrawPage(
                window,
                () => {
                    SfxGenPageHeading.Draw("AI Sound Generator");
                    SfxGenPageSubHeading.Draw("Learn how to get started below with the app.");
                },
                () => {
                    GUILayout.Space(10);

                    var headingStyle = new GUIStyle(EditorStyles.boldLabel) {
                        fontSize = 14,
                        normal = { textColor = Color.white },
                        hover = { textColor = Color.white }
                    };
                    var bodyStyle = new GUIStyle(EditorStyles.label) {
                        fontSize = 13,
                        wordWrap = true,
                        normal = { textColor = Color.gray },
                        hover = { textColor = Color.gray }
                    };

                    EditorGUILayout.LabelField("Thanks for downloading AI Sound Generator!", headingStyle);
                    GUILayout.Space(4);
                    EditorGUILayout.LabelField(
                        "AI Sound Generator allows you to quickly transform a description into a real sound effect. " +
                        "With over 1000+ built-in sounds and an intuitive interface, it makes sound creation fast and fun. " +
                        "For help and a tour of the app, check out the Getting Started video below.",
                        bodyStyle
                    );

                    GUILayout.Space(16);

                    DrawVideoCard(
                        _videoThumbnail,
                        "Getting Started with AI Sound Generator",
                        "Watch a quick walkthrough to learn how to use the app, generate sounds, and explore features.",
                        "https://www.youtube.com/watch?v=gFkfLRGllKI"
                    );

                    GUILayout.Space(24);

                    EditorGUILayout.LabelField("Examples", headingStyle);
                    GUILayout.Space(8);

                    DrawExampleSounds();
                    DrawIconLegend();

                    GUILayout.Space(24);
                    var linkStyle = new GUIStyle(EditorStyles.linkLabel) {
                        fontSize = 13,
                        alignment = TextAnchor.MiddleLeft,
                        margin = new RectOffset(3, 0, 0, 0),
                        padding = new RectOffset(0, 0, 0, 0),
                        contentOffset = Vector2.zero
                    };

                    GUILayout.Label("Support", headingStyle);
                    GUILayout.Space(4);
                    var rect = GUILayoutUtility.GetRect(new GUIContent("Click here to contact us"), linkStyle);
                    EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
                    if (GUI.Button(rect, "Click here to contact us", linkStyle))
                        Application.OpenURL("mailto:rkstudiosdev@gmail.com");
                }
            );
        }

        private void DrawIconLegend() {
            var starIcon =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Assets/RK Studios/AI Sound Generator/Editor/Images/star-icon.png");
            var downloadIcon =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Assets/RK Studios/AI Sound Generator/Editor/Images/download-icon.png");
            var playIcon =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Assets/RK Studios/AI Sound Generator/Editor/Images/play-icon.png");
            var deleteIcon =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Assets/RK Studios/AI Sound Generator/Editor/Images/delete-icon.png");

            var labelStyle = new GUIStyle(EditorStyles.label) {
                fontSize = 13,
                normal = { textColor = Color.white },
                hover = { textColor = Color.white }
            };

            EditorGUILayout.BeginHorizontal();
            DrawIconWithLabel(starIcon, "Favorite", labelStyle, 60f);
            DrawIconWithLabel(downloadIcon, "Import", labelStyle, 50f);
            DrawIconWithLabel(playIcon, "Play", labelStyle, 40f);
            DrawIconWithLabel(deleteIcon, "Delete", labelStyle, 50f);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawIconWithLabel(Texture2D icon, string text, GUIStyle style, float labelWidth) {
            const float iconSize = 24f;
            var totalWidth = iconSize + labelWidth;

            GUILayout.BeginHorizontal(GUILayout.Width(totalWidth), GUILayout.Height(iconSize));
            GUILayout.Label(icon, GUILayout.Width(iconSize), GUILayout.Height(iconSize));
            GUILayout.Space(2);
            var textHeight = style.CalcHeight(new GUIContent(text), labelWidth);
            var textOffset = (iconSize - textHeight) * 0.5f;
            var textRect = GUILayoutUtility.GetRect(new GUIContent(text), style, GUILayout.Width(labelWidth));
            textRect.y += textOffset;
            GUI.Label(textRect, text, style);
            GUILayout.EndHorizontal();
        }

        private void DrawExampleSounds() {
            string[] exampleIds = {
                "20250410000902_victory_fanfare_short_beep_tri_1",
                "20250409223449_crossbow_bolt_releasing_twang_1",
                "20250410000515_taxi_pulling_up_abrupt_tire_sq_1",
                "20250409234822_cattle_herd_moo_grazing_field_1"
            };

            foreach (var soundId in exampleIds) {
                var sound = SfxGenDatabase
                    .GetAllSounds()
                    .FirstOrDefault(s => s.id == soundId);

                if (sound == null) {
                    EditorGUILayout.HelpBox($"Sound with ID '{soundId}' not found in database.", MessageType.Warning);
                    GUILayout.Space(10);
                    continue;
                }

                DrawSoundCard(sound);
                GUILayout.Space(10);
            }
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

                        if (isFavorited) SfxGenDatabase.ToggleFavorite(sound.id);

                        EditorApplication.QueuePlayerLoopUpdate();
                    }
                    : null
            );
        }

        private void DrawVideoCard(Texture2D thumbnail, string title, string description, string videoUrl) {
            var cardStyle = new GUIStyle(GUI.skin.box) {
                padding = new RectOffset(12, 12, 12, 12),
                margin = new RectOffset(0, 0, 0, 0)
            };

            var thumbWidth = 160f;
            var thumbHeight = 90f;

            EditorGUILayout.BeginVertical(cardStyle);
            var rowRect = GUILayoutUtility.GetRect(1, thumbHeight);
            EditorGUILayout.EndVertical();

            EditorGUIUtility.AddCursorRect(rowRect, MouseCursor.Link);
            if (Event.current.type == EventType.MouseUp && rowRect.Contains(Event.current.mousePosition)) {
                Application.OpenURL(videoUrl);
                Event.current.Use();
            }

            GUI.BeginGroup(rowRect);
            var thumbRect = new Rect(0, 0, thumbWidth, thumbHeight);
            GUI.DrawTexture(thumbRect, thumbnail, ScaleMode.StretchToFill);

            var spacing = 16f;
            var textX = thumbRect.xMax + spacing;
            var textWidth = rowRect.width - textX;
            var textRect = new Rect(textX, 0, textWidth, thumbHeight);

            GUI.BeginGroup(textRect);

            var titleStyle = new GUIStyle(EditorStyles.boldLabel) {
                fontSize = 13,
                normal = { textColor = Color.white },
                hover = { textColor = Color.white }
            };
            var descStyle = new GUIStyle(EditorStyles.label) {
                fontSize = 13,
                wordWrap = true,
                normal = { textColor = Color.gray },
                hover = { textColor = Color.gray }
            };

            var titleHeight = titleStyle.CalcHeight(new GUIContent(title), textWidth);
            var descHeight = descStyle.CalcHeight(new GUIContent(description), textWidth);
            var totalTextHeight = titleHeight + descHeight;
            var textTop = (thumbHeight - totalTextHeight) * 0.5f;

            GUI.Label(new Rect(0, textTop, textWidth, titleHeight), title, titleStyle);
            var descTop = textTop + titleHeight + 4;
            GUI.Label(new Rect(0, descTop, textWidth, descHeight), description, descStyle);

            GUI.EndGroup();
            GUI.EndGroup();
        }
    }
}