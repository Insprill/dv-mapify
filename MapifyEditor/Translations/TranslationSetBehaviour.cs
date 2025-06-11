using DVLangHelper.Data;
using UnityEngine;

namespace Mapify.Editor
{
    //this is a hack. we are using this because serializing Lists of serializable classes just doesn't work i guess
    public class TranslationSetBehaviour : MonoBehaviour
    {
        public DVLanguage language;
        public string translation;

        public TranslationSet ToOriginal()
        {
            return new TranslationSet {
                language = language,
                translation = translation
            };
        }
    }
}
