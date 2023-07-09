#if UNITY_EDITOR
using UnityEditor;

namespace Mapify.Editor
{
    [CustomEditor(typeof(VanillaLocomotiveSpawner))]
    public class LocomotiveSpawnerEditor : UnityEditor.Editor
    {
        private LocomotiveSpawner spawner;
        private SerializedProperty loadingTrackStationId;
        private SerializedProperty loadingTrackYardId;
        private SerializedProperty loadingTrackId;

        private void OnEnable()
        {
            spawner = (LocomotiveSpawner)target;
            loadingTrackStationId = serializedObject.FindProperty(nameof(LocomotiveSpawner.loadingTrackStationId));
            loadingTrackYardId = serializedObject.FindProperty(nameof(LocomotiveSpawner.loadingTrackYardId));
            loadingTrackId = serializedObject.FindProperty(nameof(LocomotiveSpawner.loadingTrackId));
        }

        public override void OnInspectorGUI()
        {
            Station parentStation = spawner.GetComponentInParent<Station>();
            if (parentStation)
            {
                EditorGUILayout.PropertyField(loadingTrackStationId);
                EditorGUILayout.PropertyField(loadingTrackYardId);
                EditorGUILayout.PropertyField(loadingTrackId);
            }

            base.OnInspectorGUI();
        }
    }
}
#endif
