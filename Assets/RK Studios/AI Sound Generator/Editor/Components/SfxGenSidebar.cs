using System;
using RK_Studios.AI_Sound_Generator.Editor.Settings;
using RK_Studios.AI_Sound_Generator.Editor.Types;
using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Components {
    public static class SfxGenSidebar {
        public static void DrawSidebar(SfxGen window, SfxGenPage page) {
            var headingStyle = new GUIStyle(EditorStyles.boldLabel) {
                fontSize = 11,
                normal = { textColor = SfxGenSettings.SidebarHeadingColor },
                hover = { textColor = SfxGenSettings.SidebarHeadingColor },
                active = { textColor = SfxGenSettings.SidebarHeadingColor },
                margin = new RectOffset(
                    SfxGenSettings.SidebarButtonXPadding,
                    0,
                    0,
                    SfxGenSettings.SidebarButtonXPadding
                )
            };

            var sidebarButtonStyle = new GUIStyle(GUIStyle.none) {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 12,
                padding = new RectOffset(
                    SfxGenSettings.SidebarButtonXPadding,
                    SfxGenSettings.SidebarButtonXPadding,
                    SfxGenSettings.SidebarButtonYPadding,
                    SfxGenSettings.SidebarButtonYPadding
                ),
                normal = { textColor = Color.white },
                hover = { textColor = Color.gray },
                active = { textColor = Color.white }
            };

            // Sidebar background
            var sidebarRect = new Rect(0, 0, SfxGenSettings.SidebarWidth, window.position.height);
            EditorGUI.DrawRect(sidebarRect, SfxGenSettings.SidebarColor);

            // Sidebar border
            EditorGUI.DrawRect(new Rect(SfxGenSettings.SidebarWidth, 0, 1, window.position.height),
                SfxGenSettings.SidebarColor);

            // Layout container
            GUILayout.BeginVertical(GUILayout.Width(SfxGenSettings.SidebarWidth));

            GUILayout.Space(SfxGenSettings.SidebarYPadding);

            GUILayout.BeginHorizontal();
            GUILayout.Space(SfxGenSettings.SidebarXPadding);

            GUILayout.BeginVertical();

            GUILayout.Label("MAIN", headingStyle);

            var homeIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/RK Studios/AI Sound Generator/Editor/Images/home-icon.png"
            );

            var generateIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/RK Studios/AI Sound Generator/Editor/Images/create-icon.png"
            );

            var soundsIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/RK Studios/AI Sound Generator/Editor/Images/sounds-icon.png"
            );

            var settingsIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/RK Studios/AI Sound Generator/Editor/Images/settings-icon.png"
            );

            var searchIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/RK Studios/AI Sound Generator/Editor/Images/search-icon.png"
            );

            var starIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/RK Studios/AI Sound Generator/Editor/Images/star-icon.png"
            );

            DrawSidebarNavButton("Home", homeIcon, () => window.SetPage(SfxGenPage.Home), sidebarButtonStyle,
                page == SfxGenPage.Home);
            GUILayout.Space(SfxGenSettings.SidebarButtonSpacing);
            DrawSidebarNavButton("Sound Effects", soundsIcon, () => window.SetPage(SfxGenPage.Top),
                sidebarButtonStyle,
                page == SfxGenPage.SoundEffectCategories || page == SfxGenPage.SoundEffects || page == SfxGenPage.Top);
            GUILayout.Space(SfxGenSettings.SidebarButtonSpacing);
            DrawSidebarNavButton("Generate", generateIcon, () => window.SetPage(SfxGenPage.Generate),
                sidebarButtonStyle, page == SfxGenPage.Generate);
            GUILayout.Space(SfxGenSettings.SidebarButtonSpacing);
            DrawSidebarNavButton("Favorites", starIcon, () => window.SetPage(SfxGenPage.Favorites),
                sidebarButtonStyle, page == SfxGenPage.Favorites);
            GUILayout.Space(SfxGenSettings.SidebarButtonSpacing);
            DrawSidebarNavButton("Search", searchIcon, () => window.SetPage(SfxGenPage.Search),
                sidebarButtonStyle, page == SfxGenPage.Search);
            GUILayout.FlexibleSpace();
            DrawSidebarNavButton("Settings", settingsIcon, () => window.SetPage(SfxGenPage.Settings),
                sidebarButtonStyle, page == SfxGenPage.Settings);

            GUILayout.EndVertical();
            GUILayout.Space(SfxGenSettings.SidebarXPadding);
            GUILayout.EndHorizontal();

            GUILayout.Space(SfxGenSettings.SidebarYPadding - SfxGenSettings.SidebarButtonYPadding);
            GUILayout.EndVertical();
        }

        private static void DrawSidebarNavButton(string label, Texture icon, Action onClick, GUIStyle style,
            bool isActive) {
            var height = 36f;
            var width = SfxGenSettings.SidebarWidth - SfxGenSettings.SidebarXPadding * 2;

            var content = new GUIContent("  " + label, icon);

            // Lock to fixed size
            var rect = GUILayoutUtility.GetRect(content, style, GUILayout.Height(height), GUILayout.Width(width));

            var evt = Event.current;

            // Active background
            if (isActive && evt.type == EventType.Repaint)
                EditorGUI.DrawRect(rect, new Color(0.125f, 0.125f, 0.125f));
            // Hover background
            else if (rect.Contains(evt.mousePosition) && evt.type == EventType.Repaint)
                EditorGUI.DrawRect(rect, new Color(1f, 1f, 1f, 0.025f));

            // Hover cursor (only if not active)
            if (!isActive && rect.Contains(evt.mousePosition)) EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            // Click
            if (GUI.Button(rect, content, style)) onClick?.Invoke();
        }
    }
}