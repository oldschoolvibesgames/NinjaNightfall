using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Components {
    public static class SfxGenErrorComponent {
        public static void DrawError(string errorMessage) {
            if (string.IsNullOrEmpty(errorMessage)) return;

            var errorIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/RK Studios/AI Sound Generator/Editor/Images/error-icon.png"
            );

            var boxStyle = new GUIStyle(GUI.skin.box) {
                padding = new RectOffset(16, 16, 20, 20),
                margin = new RectOffset(0, 0, 0, 0)
            };

            var headingStyle = new GUIStyle(EditorStyles.boldLabel) {
                normal = { textColor = Color.white },
                fontSize = 12
            };

            var textStyle = new GUIStyle(EditorStyles.label) {
                wordWrap = true,
                fontSize = 11,
                normal = { textColor = Color.white }
            };

            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(errorIcon, GUILayout.Width(32), GUILayout.Height(32));
            GUILayout.Space(12);

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Error", headingStyle);
            GUILayout.Space(5);
            GUILayout.Label(errorMessage, textStyle);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private static Texture2D MakeTex(int width, int height, Color col) {
            var pix = new Color[width * height];
            for (var i = 0; i < pix.Length; ++i)
                pix[i] = col;

            var tex = new Texture2D(width, height);
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }
    }
}