using System.Collections.Generic;
using RK_Studios.AI_Sound_Generator.Editor.Components;
using RK_Studios.AI_Sound_Generator.Editor.Pages;
using RK_Studios.AI_Sound_Generator.Editor.Types;
using RK_Studios.AI_Sound_Generator.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace RK_Studios.AI_Sound_Generator.Editor {
    public class SfxGen : EditorWindow {
        private readonly Dictionary<SfxGenPage, ISfxGenPage> _pages = new() {
            { SfxGenPage.Home, new SfxGenHomePage() },
            { SfxGenPage.SoundEffectCategories, new SfxGenSoundEffectCategoriesPage() },
            { SfxGenPage.SoundEffects, new SfxGenSoundEffectsPage() },
            { SfxGenPage.Generate, new SfxGenGeneratePage() },
            { SfxGenPage.Settings, new SfxGenSettingsPage() },
            { SfxGenPage.Search, new SfxGenSearchPage() },
            { SfxGenPage.Favorites, new SfxGenFavoritesPage() },
            { SfxGenPage.Top, new SfxGenTopPage() }

        };

        private SfxGenPage _page = SfxGenPage.Home;
        private Vector2 _scrollPos;

        private void Update() {
            Repaint();
        }

        private void OnEnable() {
            SfxGenDatabase.LoadAll();
            SfxGenSettingsUtility.LoadSettings();
        }

        private void OnGUI() {
            EditorGUILayout.BeginHorizontal();
            SfxGenSidebar.DrawSidebar(this, _page);
            DrawMainContent();
            EditorGUILayout.EndHorizontal();
        }

        [MenuItem("Tools/AI Sound Generator")]
        public static void ShowWindow() {
            GetWindow<SfxGen>("AI Sound Generator");
        }

        public void SetPage(SfxGenPage page) {
            if (_page != page && _pages.TryGetValue(_page, out var oldPage))
                oldPage.Reset(); // reset old page

            _page = page;
            _scrollPos = Vector2.zero; // <-- Reset scroll

            if (_pages.TryGetValue(_page, out var newPage))
                newPage.Reset(); // reset new page
        }


        private void DrawMainContent() {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            if (_pages.TryGetValue(_page, out var currentPage))
                currentPage.Draw(this);
            else
                GUILayout.Label("Page not found.", EditorStyles.boldLabel);

            EditorGUILayout.EndScrollView();
        }
    }
}