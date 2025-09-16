using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RK_Studios.AI_Sound_Generator.Editor.Types;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor.Utilities {
    public static class SfxGenApiUtility {
        private const string FreeApiUrl = "https://api.elevenlabs.io/sound-generation";
        private const string PaidApiUrl = "https://api.elevenlabs.io/v1/sound-generation";
        private const string ApiKeyHeader = "xi-api-key";
        private const int MaxPromptLength = 30;

        // Default folders
        private static readonly string UserOutputFolder =
            "Assets/RK Studios/AI Sound Generator/Editor/Sound Effects/User";

        private static readonly string LibraryOutputFolder =
            "Assets/RK Studios/AI Sound Generator/Editor/Sound Effects/Library";

        private static readonly string UserSoundsPath =
            "Assets/RK Studios/AI Sound Generator/Editor/Data/user_sounds.json";

        public static async Task<(List<SoundEffect> sounds, string error)> CreateSFX(
            string prompt,
            bool storeInLibrary = false
        ) {
            // Decide which folder to place files in
            var outputFolder = storeInLibrary ? LibraryOutputFolder : UserOutputFolder;
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            var generated = new List<SoundEffect>();
            var apiKey = GetApiKey();

            try {
                if (string.IsNullOrWhiteSpace(apiKey)) {
                    // Free version (base64-encoded .wav)
                    var payload = new { text = prompt };
                    var jsonString = JsonConvert.SerializeObject(payload);

                    using (var client = new HttpClient()) {
                        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(FreeApiUrl, content);
                        var responseBody = await response.Content.ReadAsStringAsync();

                        if (!response.IsSuccessStatusCode) {
                            Debug.LogError($"Free API Error: {responseBody}");

                            try {
                                var wrapped = JsonConvert.DeserializeObject<WrappedApiError>(responseBody);
                                var error = wrapped?.detail;

                                if (error?.status == "quota_exceeded")
                                    return (generated, "Please add your API key on the settings page.");

                                return (generated, $"Error: {error?.message ?? "Unknown error"}");
                            }
                            catch (Exception parseEx) {
                                Debug.LogError(
                                    $"Error parsing free API error response: {parseEx}\nRaw body: {responseBody}");
                                return (generated, "An unknown error occurred.");
                            }
                        }

                        var result = JsonConvert.DeserializeObject<SoundGenerationResponse>(responseBody);
                        if (result?.sound_generations_with_waveforms == null ||
                            result.sound_generations_with_waveforms.Length == 0)
                            return (generated, "No sound generations returned by the API.");

                        await ProcessBase64Results(prompt, result, generated, outputFolder);
                    }
                }
                else {
                    // Paid version (MP3)
                    var payload = new {
                        text = prompt,
                        output_format = "mp3_44100_128"
                    };
                    var jsonString = JsonConvert.SerializeObject(payload);

                    using (var client = new HttpClient()) {
                        client.DefaultRequestHeaders.Add(ApiKeyHeader, apiKey);
                        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(PaidApiUrl, content);

                        if (!response.IsSuccessStatusCode) {
                            var errorText = await response.Content.ReadAsStringAsync();
                            Debug.LogError($"Paid API Error: {errorText}");
                            return (generated, $"Error: {errorText}");
                        }

                        var timeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                        var safePrompt = MakeSafeFileName(prompt);
                        var fileName = $"{timeStamp}_{safePrompt}_1.mp3";
                        var outputPath = Path.Combine(outputFolder, fileName);

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = File.Create(outputPath)) {
                            await stream.CopyToAsync(fileStream);
                        }

                        AssetDatabase.ImportAsset(outputPath);

                        var sound = new SoundEffect {
                            id = Path.GetFileNameWithoutExtension(fileName),
                            name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(prompt),
                            duration = 0f,
                            path = outputPath,
                            category = storeInLibrary ? "Library" : "user generated"
                        };

                        // If storing in library, we won't save to user_sounds.json
                        // because your library is presumably stored in library.json
                        // We'll just return the object for the caller to handle.
                        if (!storeInLibrary) SaveUserSound(sound);

                        generated.Add(sound);
                    }

                    AssetDatabase.Refresh();
                }
            }
            catch (Exception ex) {
                Debug.LogError("Unhandled exception generating sound: " + ex);
                return (generated, "Error generating sound: " + ex.Message);
            }

            return (generated, null);
        }

        private static string GetApiKey() {
            var settings = SfxGenSettingsUtility.LoadSettings();
            return string.IsNullOrWhiteSpace(settings?.elevenApiKey) ? null : settings.elevenApiKey;
        }

        private static async Task ProcessBase64Results(
            string prompt,
            SoundGenerationResponse result,
            List<SoundEffect> generated,
            string outputFolder
        ) {
            var existing = LoadExistingUserSounds();

            for (var i = 0; i < result.sound_generations_with_waveforms.Length; i++) {
                var base64Data = result.sound_generations_with_waveforms[i].waveform_base_64;
                var timeStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var safePrompt = MakeSafeFileName(prompt);
                var fileName = $"{timeStamp}_{safePrompt}_{i + 1}.wav";
                var outputPath = Path.Combine(outputFolder, fileName);

                Base64ToWav(base64Data, outputPath);
                AssetDatabase.ImportAsset(outputPath);

                var sound = new SoundEffect {
                    id = Path.GetFileNameWithoutExtension(fileName),
                    name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(prompt),
                    duration = 0f,
                    path = outputPath,
                    category = outputFolder.Contains("Library") ? "Library" : "user generated"
                };

                // If it's going to "Library", skip adding to user_sounds.json
                if (!outputFolder.Contains("Library")) existing.Add(sound);

                generated.Add(sound);
            }

            if (!outputFolder.Contains("Library"))
                File.WriteAllText(UserSoundsPath, JsonConvert.SerializeObject(existing, Formatting.Indented));

            AssetDatabase.Refresh();
        }

        private static List<SoundEffect> LoadExistingUserSounds() {
            try {
                if (File.Exists(UserSoundsPath)) {
                    var json = File.ReadAllText(UserSoundsPath);
                    return JsonConvert.DeserializeObject<List<SoundEffect>>(json) ?? new List<SoundEffect>();
                }
            }
            catch (Exception ex) {
                Debug.LogWarning("Could not parse user_sounds.json. Starting fresh. Reason: " + ex.Message);
            }

            return new List<SoundEffect>();
        }

        private static void SaveUserSound(SoundEffect sound) {
            var existing = LoadExistingUserSounds();
            existing.Add(sound);
            File.WriteAllText(UserSoundsPath, JsonConvert.SerializeObject(existing, Formatting.Indented));
            SfxGenDatabase.ReloadUserSounds(); // <-- Refresh in-memory list
        }

        private static void Base64ToWav(string base64Data, string outputPath) {
            const string base64Header = "data:audio/wav;base64,";
            if (base64Data.StartsWith(base64Header, StringComparison.OrdinalIgnoreCase))
                base64Data = base64Data.Substring(base64Header.Length);

            var audioBuffer = Convert.FromBase64String(base64Data);
            File.WriteAllBytes(outputPath, audioBuffer);
        }

        private static string MakeSafeFileName(string text) {
            text = text.ToLowerInvariant();
            foreach (var c in Path.GetInvalidFileNameChars())
                text = text.Replace(c.ToString(), "");
            text = text.Replace(" ", "_");
            text = Regex.Replace(text, @"[^a-z0-9_]", "_");
            text = Regex.Replace(text, @"_+", "_");
            text = text.Trim('_');

            if (text.Length > MaxPromptLength)
                text = text.Substring(0, MaxPromptLength);

            return text;
        }

        [Serializable]
        private class SoundGenerationResponse {
            [JsonProperty("sound_generations_with_waveforms")]
            public SoundGenerationWithWaveform[] sound_generations_with_waveforms;
        }

        [Serializable]
        private class SoundGenerationWithWaveform {
            [JsonProperty("waveform_base_64")] public string waveform_base_64;
        }

        [Serializable]
        private class ApiError {
            public string status;
            public string message;
        }

        [Serializable]
        private class WrappedApiError {
            public ApiError detail;
        }
    }
}