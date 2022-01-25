using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
#if UNITY_EDITOR
    public override void OnInspectorGUI()
    {
        MapGenerator mapGenerator = (MapGenerator)target;

        DrawDefaultInspector();

        if (mapGenerator.autoUpdate)
            mapGenerator.DrawMapEditor();

        if (GUILayout.Button("Generator"))
        {
            mapGenerator.DrawMapEditor();
        }
    }

#endif
}
