using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Mapify.BuildMode
{

    public class AssetArea : MonoBehaviour
    {
        private List<GameObject> areaObjects = new List<GameObject>();
        private InputField inputField;

        private void Start()
        {
            inputField = GetComponentInParent<CanvasScaler>().gameObject.GetComponentInChildren<InputField>();
            if (inputField == null)
            {
                Mapify.LogError($"{nameof(AssetArea)}: could not find inputField");
                return;
            }

            inputField.onValueChanged.AddListener(OnSearchTextChanged);
        }

        private void OnSearchTextChanged(string searchTerm)
        {
            foreach (var obj in areaObjects)
            {
                // ToLower to make the search case-insensitive.
                obj.SetActive(obj.name.ToLower().Contains(searchTerm.ToLower()));
            }
        }

        public void CreateAreaObject(GameObject prefab, string objectName, Texture2D texture, UnityAction onClick)
        {
            var areaObject = Instantiate(prefab, transform);
            areaObject.name = objectName;

            var imageComponent = areaObject.GetComponentInChildren<RawImage>();
            imageComponent.texture = texture;

            var button = areaObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(onClick);

            var textComponent = areaObject.GetComponentInChildren<Text>();
            textComponent.text = objectName;

            areaObject.SetActive(true);
            areaObjects.Add(areaObject);
        }
    }
}
