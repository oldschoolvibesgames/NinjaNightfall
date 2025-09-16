using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Components {
    public static class SfxGenPageHeading {
        private static readonly GUIStyle TitleStyle = new(EditorStyles.label) {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white },
            hover = { textColor = Color.white, background = null },
            alignment = TextAnchor.UpperLeft,
            wordWrap = true,
            margin = new RectOffset(0, 0, 0, 0)
        };

        public static void Draw(string headingText) {
            GUILayout.Label(headingText, TitleStyle);
        }
    }
}