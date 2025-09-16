using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Components {
    public static class SfxGenTextField {
        private static readonly GUIStyle TextFieldStyle = new(EditorStyles.textField) {
            fontSize = 14,
            normal = { textColor = Color.white },
            hover = { textColor = Color.white, background = null },
            focused = { textColor = Color.white },
            alignment = TextAnchor.MiddleLeft,
            wordWrap = false,
            margin = new RectOffset(0, 0, 4, 4),
            padding = new RectOffset(6, 6, 4, 4)
        };

        public static string Draw(string label, string value) {
            GUILayout.BeginVertical();
            if (!string.IsNullOrEmpty(label)) GUILayout.Label(label, EditorStyles.boldLabel);
            var result = EditorGUILayout.TextField(value, TextFieldStyle);
            GUILayout.EndVertical();
            return result;
        }
    }
}