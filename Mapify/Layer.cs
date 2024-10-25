using UnityEngine;

namespace Mapify
{
    public class Layer
    {
        public static readonly Layer Default = new Layer("Default");
        public static readonly Layer PostProcessing = new Layer("PostProcessing");
        public static readonly Layer Terrain = new Layer("Terrain");
        public static readonly Layer Train_Big_Collider = new Layer("Train_Big_Collider");

        private readonly int layer;

        private Layer(string name)
        {
            layer = LayerMask.NameToLayer(name);
        }

        public void Apply(GameObject gameObject)
        {
            gameObject.layer = layer;
        }

        public void Apply(Component component)
        {
            Apply(component.gameObject);
        }

        public void ApplyRecursive(GameObject gameObject)
        {
            gameObject.SetLayersRecursive(layer);
        }
    }
}
