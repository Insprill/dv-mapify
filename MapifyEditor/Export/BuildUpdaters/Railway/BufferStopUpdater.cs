#if UNITY_EDITOR
using Mapify.Editor.Utils;
using UnityEngine;

namespace Mapify.Editor.StateUpdaters
{
    public class BufferStopUpdater : BuildUpdater
    {
        protected override void Update(Scenes scenes)
        {
            foreach (BufferStop bufferStop in scenes.railwayScene.GetAllComponents<BufferStop>())
            {
                BoxCollider collider = bufferStop.GetComponent<BoxCollider>();
                collider.isTrigger = true;
            }
        }
    }
}
#endif
