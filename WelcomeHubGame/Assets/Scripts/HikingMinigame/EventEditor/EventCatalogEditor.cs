using Event;
using UnityEditor;
using UnityEngine;

namespace HikingMinigame.EventEditor
{
    [CustomEditor(typeof(EventCatalog))]
    public class EventCatalogEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var catalog = (EventCatalog)target;

            if (GUILayout.Button("Auto-Populate From Project"))
            {
                var guids = AssetDatabase.FindAssets("t:EventDefinition");
                var found = new EventDefinition[guids.Length];

                for (var i = 0; i < guids.Length; i++)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    found[i] = AssetDatabase.LoadAssetAtPath<EventDefinition>(path);
                }

                catalog.Events = found;
                EditorUtility.SetDirty(catalog);
            }
        }
    }
}