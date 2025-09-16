using RK_Studios.AI_Sound_Generator.Editor.Components;
using RK_Studios.AI_Sound_Generator.Editor.Types;
using RK_Studios.AI_Sound_Generator.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Pages {
    public class SfxGenSettingsPage : ISfxGenPage {
        private const float SaveDelay = 1.0f;
        private const float CopiedDuration = 2.0f;

        private double _copiedTime = -10f;
        private double _lastImportChangeTime;
        private double _lastKeyTime;
        private string _pendingImportPath;

        private string _pendingKey;
        private bool _pendingSave;

        private SfxGenUserSettings _settings;

        public void Reset() {
            _settings = SfxGenSettingsUtility.LoadSettings();
            _pendingKey = _settings.elevenApiKey;
            _pendingImportPath = _settings.importPath;
            _lastKeyTime = 0;
            _lastImportChangeTime = 0;
            _pendingSave = false;
            _copiedTime = -10f;
        }

        public void Draw(SfxGen window) {
            if (_settings == null) Reset();

            SfxGenPageLayout.DrawPage(
                window,
                () => {
                    SfxGenPageHeading.Draw("Settings");
                    SfxGenPageSubHeading.Draw("Welcome to AI AI Sound Generator");
                },
                () => {
                    DrawGitIgnoreWarning();

                    GUILayout.Space(24);

                    // API Key
                    GUILayout.Label("Eleven Labs API Key", EditorStyles.boldLabel);
                    GUILayout.Space(5);

                    var apiFieldStyle = new GUIStyle(EditorStyles.textField) {
                        fontSize = 12,
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset(8, 8, 0, 0)
                    };

                    EditorGUI.BeginChangeCheck();
                    _pendingKey =
                        EditorGUILayout.TextField("", _pendingKey, apiFieldStyle, GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck()) {
                        _lastKeyTime = EditorApplication.timeSinceStartup;
                        _pendingSave = true;
                    }

                    GUILayout.Space(5);

                    // Inline label with clickable links using pointer cursor and matched color
                    var linkColor = EditorStyles.linkLabel.normal.textColor;
                    var hexColor = ColorUtility.ToHtmlStringRGB(linkColor);
                    var fullText =
                        $"Signup for a free API key <color=#{hexColor}><u>here</u></color>. Need help? Watch our <color=#{hexColor}><u>instructional video</u></color>.";
                    var richLabelStyle = new GUIStyle(EditorStyles.label) { richText = true };

                    GUILayout.Label(fullText, richLabelStyle);
                    var labelRect = GUILayoutUtility.GetLastRect();

                    var clickPos = Event.current.mousePosition;

                    var beforeHere = new GUIContent("Signup for a free API key ");
                    var here = new GUIContent("here");
                    var beforeVideo = new GUIContent(". Need help? Watch our ");
                    var video = new GUIContent("instructional video");

                    var x = labelRect.x;

                    var beforeHereWidth = richLabelStyle.CalcSize(beforeHere).x;
                    var hereWidth = richLabelStyle.CalcSize(here).x;
                    var beforeVideoWidth = richLabelStyle.CalcSize(beforeVideo).x;
                    var videoWidth = richLabelStyle.CalcSize(video).x;

                    var hereRect = new Rect(x + beforeHereWidth, labelRect.y, hereWidth, labelRect.height);
                    var videoRect = new Rect(x + beforeHereWidth + hereWidth + beforeVideoWidth, labelRect.y,
                        videoWidth, labelRect.height);

                    // Pointer cursor when hovering
                    EditorGUIUtility.AddCursorRect(hereRect, MouseCursor.Link);
                    EditorGUIUtility.AddCursorRect(videoRect, MouseCursor.Link);

                    if (Event.current.type == EventType.MouseUp && labelRect.Contains(clickPos)) {
                        if (hereRect.Contains(clickPos)) {
                            Application.OpenURL("https://try.elevenlabs.io/zof2ue412sd2");
                            Event.current.Use();
                        }
                        else if (videoRect.Contains(clickPos)) {
                            Application.OpenURL("https://youtu.be/px3GHgXK4ds");
                            Event.current.Use();
                        }
                    }

                    GUILayout.Space(20);

                    // Import Path
                    GUILayout.Label("Import Path", EditorStyles.boldLabel);
                    GUILayout.Space(5);

                    EditorGUI.BeginChangeCheck();
                    _pendingImportPath = EditorGUILayout.TextField("", _pendingImportPath, apiFieldStyle,
                        GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck()) _lastImportChangeTime = EditorApplication.timeSinceStartup;

                    // Save API key after delay
                    if (_pendingSave && EditorApplication.timeSinceStartup - _lastKeyTime > SaveDelay) {
                        _settings.elevenApiKey = _pendingKey;
                        SfxGenSettingsUtility.SaveSettings(_settings);
                        _pendingSave = false;
                        Debug.Log("API key saved.");
                    }

                    // Save Import Path after delay
                    if (_pendingImportPath != _settings.importPath &&
                        EditorApplication.timeSinceStartup - _lastImportChangeTime > SaveDelay) {
                        _settings.importPath = _pendingImportPath;
                        SfxGenSettingsUtility.SaveSettings(_settings);
                        Debug.Log("Import path saved.");
                    }
                },
                () => { window.SetPage(SfxGenPage.Home); }
            );
        }

        private void DrawGitIgnoreWarning() {
            const string ignorePath = "Assets/RK Studios/AI Sound Generator/Editor/Data/settings.json";

            var warningIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Assets/RK Studios/AI Sound Generator/Editor/Images/warning-icon.png"
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
                wordWrap = true
            };

            var centeredFieldStyle = new GUIStyle(EditorStyles.textField) {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Normal,
                fontSize = 11,
                padding = new RectOffset(8, 8, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };

            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(warningIcon, GUILayout.Width(32), GUILayout.Height(32));
            GUILayout.Space(12);

            EditorGUILayout.BeginVertical();
            GUILayout.Label("Warning", headingStyle);

            GUILayout.Space(5);
            GUILayout.Label("To prevent sensitive data from being committed, add this to your .gitignore:", textStyle);

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel(ignorePath, centeredFieldStyle, GUILayout.Height(24),
                GUILayout.ExpandWidth(true));
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            var buttonText = EditorApplication.timeSinceStartup - _copiedTime < CopiedDuration
                ? "Copied!"
                : "Copy to Clipboard";

            GUI.enabled = buttonText != "Copied!";
            if (GUILayout.Button(buttonText, GUILayout.Width(130))) {
                EditorGUIUtility.systemCopyBuffer = ignorePath;
                _copiedTime = EditorApplication.timeSinceStartup;
                Debug.Log("Path copied to clipboard");
            }

            GUI.enabled = true;

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }
}