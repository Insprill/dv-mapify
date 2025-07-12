using Mapify.Editor;
using Mapify.Utils;
using UnityEngine;

namespace Mapify.SceneInitializers.Railway
{
    public class BufferStopSetup : SceneSetup
    {
        public override void Run()
        {
            foreach (Editor.BufferStop bufferStop in Object.FindObjectsOfType<Editor.BufferStop>())
            {
                foreach (VanillaObject vanillaObject in bufferStop.GetComponentsInChildren<VanillaObject>())
                    if (vanillaObject.asset == VanillaAsset.BufferStopModel)
                        vanillaObject.Replace();

                Transform detectionPoint = bufferStop.compressionPoint;
                BufferStopController controller = detectionPoint.gameObject.AddComponent<BufferStopController>();
                controller.bufferCompressionRange = bufferStop.compressionRange;

                GameObject go = bufferStop.gameObject;
                go.SetActive(false);
                Layer.Train_Big_Collider.ApplyRecursive(go);
                Layer.Default.Apply(bufferStop.playerCollider);
                BufferStop dvBufferStop = go.AddComponent<BufferStop>();
                dvBufferStop.triggerCollider = bufferStop.GetComponent<BoxCollider>();
                dvBufferStop.spawnOverlapCollider = bufferStop.playerCollider;
                go.SetActive(true);
            }
        }
    }
}
