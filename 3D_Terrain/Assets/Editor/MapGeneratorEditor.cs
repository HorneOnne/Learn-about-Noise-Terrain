using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGenerator = (MapGenerator)target;

        DrawDefaultInspector();

        if(mapGenerator.autoUpdate)
            mapGenerator.GenerateMap();

        if (GUILayout.Button("Generator"))
        {
            mapGenerator.GenerateMap();
        }
    }
}
