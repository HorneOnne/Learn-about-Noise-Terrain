using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DrawMode
{
    NosieMap,
    ColorMap,
    Mesh
}

public class MapGenerator : MonoBehaviour    
{
    public DrawMode drawMode;
    public int width;
    public int height;
    public int seed;
    public float scale;
    public int octave;
    public float lacunarity;
    public float persistance;
    public bool autoUpdate;
    public TerrainType[] regions;
    public Renderer mesh;
    public void GenerateMap()
    {
        float[,] noiseMap = MakeNoiseValue.CreateNoiseMap(width, height, seed, scale, octave, lacunarity, persistance);
        Color[] colorMap = new Color[width * height];
        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
       
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                for(int i = 0; i < regions.Length; i++)
                {
                    if(noiseMap[x,y] < regions[i].height)
                    {
                        colorMap[y * width + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        if (drawMode == DrawMode.NosieMap)
            mapDisplay.DrawNoiseMap(TextureGenerator.GenerateNoiseTexture(noiseMap));
        else if (drawMode == DrawMode.ColorMap)
        {
            mapDisplay.DrawNoiseMap(TextureGenerator.GenerateColorTexture(colorMap, width, height));
        }
        else if(drawMode == DrawMode.Mesh)
        {
            mapDisplay.DrawMesh(MeshGenerator.GenerateTerrianMesh(noiseMap)
                , TextureGenerator.GenerateColorTexture(colorMap, width, height));
        }    
    }
    
    void OnValidate()
    {
        if(width <= 1)
            width = 1;
        if(height <= 1)
            height = 1;
        if (scale < 0.1f)
            scale = 0.1f;

    }

}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}
