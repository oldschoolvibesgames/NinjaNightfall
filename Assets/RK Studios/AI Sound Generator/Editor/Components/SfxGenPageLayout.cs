using System;
using RK_Studios.AI_Sound_Generator.Editor.Settings;
using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Components {
    public static class SfxGenPageLayout {
        public static void DrawPage(
            SfxGen window,
            Action header,
            Action body,
            Action onBack = null
        ) {
            var backIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/RK Studios/AI Sound Generator/Editor/Images/back-icon.png"
            );

            // X padding
            GUILayout.BeginHorizontal();
            GUILayout.Space(SfxGenSettings.MainXPadding);

            // Y Padding
            GUILayout.BeginVertical();
            GUILayout.Space(SfxGenSettings.MainYPadding);

            // Columns
            GUILayout.BeginHorizontal();

            // Back arrow column (optional)
            if (onBack != null) {
                GUILayout.BeginVertical(GUILayout.Width(24));
                var iconRect = GUILayoutUtility.GetRect(24, 24, GUILayout.ExpandWidth(false));
                var isHovering = iconRect.Contains(Event.current.mousePosition);
                var oldColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, isHovering ? 1f : 0.3f);
                GUI.DrawTexture(iconRect, backIcon, ScaleMode.ScaleToFit);
                GUI.color = oldColor;
                EditorGUIUtility.AddCursorRect(iconRect, MouseCursor.Link);
                if (Event.current.type == EventType.MouseDown && iconRect.Contains(Event.current.mousePosition)) {
                    onBack?.Invoke();
                    GUI.FocusControl(null);
                    Event.current.Use();
                }

                GUILayout.EndVertical();

                GUILayout.Space(SfxGenSettings.MainXPadding);
            }

            // Main content column
            GUILayout.BeginVertical();

            // Heading
            header?.Invoke();

            // Divider
            GUILayout.Space(12);
            var underline = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(1));
            EditorGUI.DrawRect(underline, new Color(1f, 1f, 1f, 0.1f));
            GUILayout.Space(12);

            body?.Invoke();

            GUILayout.EndVertical(); // Main column
            GUILayout.EndHorizontal(); // Columns

            // End Y Padding
            GUILayout.Space(SfxGenSettings.MainYPadding);
            GUILayout.EndVertical();

            // End X Padding
            GUILayout.Space(SfxGenSettings.MainXPadding);
            GUILayout.EndHorizontal();
        }
    }
}