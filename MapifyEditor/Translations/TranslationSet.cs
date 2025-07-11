using System;
using UnityEngine;

namespace Mapify.Editor
{
    [Serializable]
    public class TranslationSet
    {
        public LanguageEnum language;
        public string translation;

        public TranslationSetBehaviour ToMonoBehaviour(GameObject gameObject)
        {
            var mb = gameObject.AddComponent<TranslationSetBehaviour>();
            mb.language = language;
            mb.translation = translation;
            return mb;
        }
    }
}
