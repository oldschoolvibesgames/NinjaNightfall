using System;
using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Components {
    public static class SfxGenSoundComponent {
        private static readonly Texture2D NoteIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/RK Studios/AI Sound Generator/Editor/Images/note-icon.png"
        );

        private static readonly Texture2D PlayIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/RK Studios/AI Sound Generator/Editor/Images/play-icon.png"
        );

        private static readonly Texture2D StarIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/RK Studios/AI Sound Generator/Editor/Images/star-icon.png"
        );

        private static readonly Texture2D StarFilledIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/RK Studios/AI Sound Generator/Editor/Images/star-filled-icon.png"
        );

        private static readonly Texture2D DownloadIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/RK Studios/AI Sound Generator/Editor/Images/download-icon.png"
        );

        private static readonly Texture2D DeleteIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/RK Studios/AI Sound Generator/Editor/Images/delete-icon.png"
        );

        private static readonly GUIStyle CardStyle = new() {
            normal = { background = MakeTex(1, 1, new Color(0.18f, 0.18f, 0.18f)) },
            margin = new RectOffset(0, 0, 4, 4),
            padding = new RectOffset(0, 0, 0, 0)
        };

        private static readonly GUIStyle LeftBoxStyle = new() {
            normal = { background = MakeTex(1, 1, Color.black) },
            alignment = TextAnchor.MiddleCenter
        };

        private static readonly GUIStyle TitleStyle = new(EditorStyles.boldLabel) {
            fontSize = 13,
            normal = { textColor = Color.white },
            hover = { textColor = Color.white },
            alignment = TextAnchor.MiddleLeft
        };

        private static readonly GUIStyle DurationStyle = new(EditorStyles.label) {
            fontSize = 12,
            normal = { textColor = Color.gray },
            hover = { textColor = Color.gray },
            alignment = TextAnchor.MiddleLeft
        };

        public static void Draw(
            string title,
            string durationText,
            bool isFavorited,
            Action onPlayPressed,
            Action onFavoritePressed,
            Action onImportPressed,
            Action onDeletePressed = null
        ) {
            const int cardHeight = 64;
            const int iconBoxSize = cardHeight;
            var starIcon = isFavorited ? StarFilledIcon : StarIcon;
            const int rightColumnWidth = 88;

            // Begin the main horizontal “card”
            using (new EditorGUILayout.HorizontalScope(CardStyle, GUILayout.Height(cardHeight))) {
                // Left box with the note icon
                using (new EditorGUILayout.VerticalScope(LeftBoxStyle, GUILayout.Width(iconBoxSize),
                           GUILayout.Height(cardHeight))) {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.HorizontalScope()) {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(NoteIcon, GUILayout.Width(40), GUILayout.Height(40));
                        GUILayout.FlexibleSpace();
                    }

                    GUILayout.FlexibleSpace();
                }

                GUILayout.Space(16);

                // Middle (title/duration)
                using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true), GUILayout.Height(cardHeight))) {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(title, TitleStyle);
                    GUILayout.Label(durationText, DurationStyle);
                    GUILayout.FlexibleSpace();
                }

                // Right column (buttons)
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(rightColumnWidth),
                           GUILayout.Height(cardHeight))) {
                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.HorizontalScope()) {
                        if (onDeletePressed != null) {
                            GUILayout.Space(16);
                            DrawIconButton(DeleteIcon, 22, 22, onDeletePressed);
                        }

                        GUILayout.Space(16);
                        DrawIconButton(starIcon, 22, 22, onFavoritePressed);
                        GUILayout.Space(16);
                        DrawIconButton(DownloadIcon, 22, 22, onImportPressed);
                        GUILayout.Space(16);
                        DrawIconButton(PlayIcon, 22, 22, onPlayPressed);
                        GUILayout.Space(16);
                    }

                    GUILayout.FlexibleSpace();
                }
            }
        }

        private static void DrawIconButton(Texture2D icon, float width, float height, Action onClick) {
            var iconRect = GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(false));
            EditorGUIUtility.AddCursorRect(iconRect, MouseCursor.Link);

            var oldColor = GUI.color;
            var hover = iconRect.Contains(Event.current.mousePosition);

            if (hover) GUI.color = new Color(oldColor.r, oldColor.g, oldColor.b, 0.5f);

            if (GUI.Button(iconRect, icon, GUIStyle.none) && onClick != null) onClick();

            GUI.color = oldColor;
        }

        private static Texture2D MakeTex(int width, int height, Color col) {
            var pix = new Color[width * height];
            for (var i = 0; i < pix.Length; ++i) pix[i] = col;

            var tex = new Texture2D(width, height);
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }
    }
}