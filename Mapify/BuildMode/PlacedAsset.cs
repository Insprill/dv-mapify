using UnityEngine;

namespace Mapify.BuildMode
{
    public class PlacedAsset
    {
        public string Name;
        public Vector3 Position;
        public Quaternion Rotation;

        //this class can't be serialized without a parameterless constructor
        public PlacedAsset(){}

        public PlacedAsset(string name, Vector3 position, Quaternion rotation)
        {
            Name = name;
            Position = position;
            Rotation = rotation;
        }
    }
}
