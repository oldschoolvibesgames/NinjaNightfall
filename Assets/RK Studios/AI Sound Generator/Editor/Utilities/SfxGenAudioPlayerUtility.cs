using System.IO;
using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Utilities {
    public static class SfxAudioPlayerUtility {
        private static AudioSource _previewSource;

        public static void PlaySound(string path) {
            if (!File.Exists(path)) {
                Debug.LogWarning("Sound file not found: " + path);
                return;
            }

            AssetDatabase.ImportAsset(path); // Ensure Unity imports the .wav file

            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null) {
                Debug.LogWarning("Couldn't load AudioClip from path: " + path);
                return;
            }

            EnsureAudioSource();
            _previewSource.Stop();
            _previewSource.clip = clip;
            _previewSource.Play();
        }

        public static void Stop() {
            if (_previewSource != null && _previewSource.isPlaying) _previewSource.Stop();
        }

        private static void EnsureAudioSource() {
            if (_previewSource == null) {
                var go = EditorUtility.CreateGameObjectWithHideFlags(
                    "SFXPreviewAudio",
                    HideFlags.HideAndDontSave,
                    typeof(AudioSource)
                );
                _previewSource = go.GetComponent<AudioSource>();
            }
        }
    }
}