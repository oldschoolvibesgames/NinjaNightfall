using System;

namespace RK_Studios.AI_Sound_Generator.Editor.Types {
    [Serializable]
    public class SoundEffect {
        public string id;
        public string name;
        public float duration;
        public string path;
        public string category;
    }
}