using System;
using UnityEngine;

namespace Mapify.Editor
{
    //this is a hack. we are using this because serializing Lists of serializable classes just doesn't work i guess
    [ExecuteInEditMode]
    public class TranslationSetBehaviour : MonoBehaviour
    {
        public LanguageEnum language;
        public string translation;

        public TranslationSet ToOriginal()
        {
            return new TranslationSet {
                language = language,
                translation = translation
            };
        }

        private void OnEnable()
        {
            //hide this script in the editor
            hideFlags = HideFlags.HideInInspector;
        }
    }
}
