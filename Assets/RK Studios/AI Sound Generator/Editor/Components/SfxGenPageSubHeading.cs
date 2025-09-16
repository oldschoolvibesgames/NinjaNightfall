using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Components {
    public static class SfxGenPageSubHeading {
        private static readonly GUIStyle TextStyle = new(EditorStyles.label) {
            fontSize = 14,
            normal = { textColor = Color.grey },
            hover = { textColor = Color.grey, background = null },
            alignment = TextAnchor.UpperLeft,
            wordWrap = true,
            margin = new RectOffset(0, 0, 8, 0)
        };

        public static void Draw(string headingText) {
            GUILayout.Label(headingText, TextStyle);
        }
    }
}