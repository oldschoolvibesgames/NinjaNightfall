using System.Collections.Generic;
using System.Linq;
using RK_Studios.AI_Sound_Generator.Editor.Components;
using RK_Studios.AI_Sound_Generator.Editor.Settings;
using RK_Studios.AI_Sound_Generator.Editor.State;
using RK_Studios.AI_Sound_Generator.Editor.Types;
using RK_Studios.AI_Sound_Generator.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Pages {
    public class SfxGenSoundEffectCategoriesPage : ISfxGenPage {
        private List<string> _categories;

        public void Reset() {
            var allSounds = SfxGenDatabase.GetAllSounds();

            _categories = allSounds
                .Select(s => s.category)
                .Where(c => !string.IsNullOrWhiteSpace(c) && c != "user generated")
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            // Optional: Insert "User Generated" at top
            if (!_categories.Contains("User Generated"))
                _categories.Insert(0, "User Generated");
        }

        public void Draw(SfxGen window) {
            SfxGenPageLayout.DrawPage(
                window,
                () => {
                    // Header content
                    SfxGenPageHeading.Draw("Sound Effects");
                    SfxGenPageSubHeading.Draw("1000+ sound effects at your fingertips. Select a category below.");
                },
                () => {
                    // Main content
                    GUILayout.Space(10);

                    // Calculate how much horizontal space is left after the sidebar + margins
                    var fullWindowWidth = EditorGUIUtility.currentViewWidth;
                    var sidebarWidth = SfxGenSettings.SidebarWidth;
                    var approximateExtraPadding = 125f;
                    var availableWidth = fullWindowWidth - sidebarWidth - approximateExtraPadding;
                    if (availableWidth < 0) availableWidth = 600; // fallback if window is super narrow

                    const int columns = 5;
                    const float spacing = 20f;
                    const float rowSpacing = 20f;

                    var totalSpacingWidth = (columns - 1) * spacing;
                    var itemWidth = (availableWidth - totalSpacingWidth) / columns;
                    if (itemWidth < 50) itemWidth = 50; // ensure a sane minimum

                    var rowCount = Mathf.CeilToInt(_categories.Count / (float)columns);
                    var index = 0;

                    for (var row = 0; row < rowCount; row++) {
                        using (new EditorGUILayout.HorizontalScope()) {
                            for (var col = 0; col < columns; col++) {
                                if (index >= _categories.Count) break;

                                DrawCategoryImage(_categories[index], window, itemWidth);
                                index++;

                                if (col < columns - 1)
                                    GUILayout.Space(spacing);
                            }
                        }

                        GUILayout.Space(rowSpacing);
                    }
                },
                () => {
                    // Footer/back button
                    window.SetPage(SfxGenPage.Top);
                }
            );
        }

        private void DrawCategoryImage(string category, SfxGen window, float width) {
            var safeName = category.ToLowerInvariant().Replace(" ", "_");
            var imagePath = $"Assets/RK Studios/AI Sound Generator/Editor/Images/Categories/{safeName}.png";
            var image = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);

            if (image == null) {
                // If missing, display fallback
                GUILayout.Label($"Missing: {category}", GUILayout.Width(width), GUILayout.Height(width * 0.75f));
                return;
            }

            // Maintain aspect ratio
            var aspect = (float)image.height / image.width;
            var height = width * aspect;

            var rect = GUILayoutUtility.GetRect(width, height,
                GUILayout.Width(width),
                GUILayout.Height(height));

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            // Hover effect
            var isHovering = rect.Contains(Event.current.mousePosition);
            if (isHovering && Event.current.type == EventType.Repaint) GUI.color = new Color(1f, 1f, 1f, 0.7f);

            GUI.DrawTexture(rect, image, ScaleMode.ScaleToFit);

            if (isHovering && Event.current.type == EventType.Repaint) GUI.color = Color.white;

            // Click handler
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition)) {
                SfxGenState.SelectedCategory = category;
                window.SetPage(SfxGenPage.SoundEffects);
                Event.current.Use();
            }
        }
    }
}