using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Components {
    public static class SfxGenPrimaryButton {
        public static bool Draw(string text, float height = 40f) {
            var content = new GUIContent(text);
            var rect = GUILayoutUtility.GetRect(content, GUIStyle.none, GUILayout.Height(height),
                GUILayout.ExpandWidth(true));

            // Hover detection
            var isHover = rect.Contains(Event.current.mousePosition);

            // Draw blue background (hover = lighter)
            var baseColor = new Color(0.2f, 0.45f, 0.9f);
            var hoverColor = new Color(0.25f, 0.5f, 1f);
            EditorGUI.DrawRect(rect, isHover ? hoverColor : baseColor);

            // Text style
            var textStyle = new GUIStyle(EditorStyles.boldLabel) {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                normal = { textColor = Color.white },
                hover = { textColor = Color.white }
            };

            // Add pointer cursor
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            // Handle click
            if (GUI.Button(rect, GUIContent.none, GUIStyle.none)) return true;

            // Draw the text over the background
            GUI.Label(rect, text, textStyle);

            return false;
        }
    }
}