using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Collections.Concurrent;

public enum DrawMode
{
    NosieMap,
    ColorMap,
    Mesh
}


public class MapGenerator : MonoBehaviour    
{
    public DrawMode drawMode;
    public MakeNoiseValue.NormalizeMode normalizeMode; 
    public const int mapChunkSize = 121;
    [Range(0,6)]
    public int levelOfDetail;


    public int seed;
    public float scale;
    public int octave;
    public float lacunarity;
    public float persistance;
    public float heightMult;
    public AnimationCurve heightCurve;
    public Vector2 offset;
    public bool autoUpdate;
    public TerrainType[] regions;
    public Renderer mesh;

    ConcurrentQueue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new ConcurrentQueue<MapThreadInfo<MapData>>();
    ConcurrentQueue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new ConcurrentQueue<MapThreadInfo<MeshData>>();

    void Awake()
    {
        Application.targetFrameRate = 120;
    }

    public void DrawMapEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);

        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NosieMap)
            mapDisplay.DrawNoiseMap(TextureGenerator.GenerateNoiseTexture(mapData.heightMap));
        else if (drawMode == DrawMode.ColorMap)
        {
            mapDisplay.DrawNoiseMap(TextureGenerator.GenerateColorTexture(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            mapDisplay.DrawMesh(MeshGenerator.GenerateTerrianMesh(mapData.heightMap, heightMult, heightCurve, levelOfDetail)
                , TextureGenerator.GenerateColorTexture(mapData.colorMap, mapChunkSize, mapChunkSize));
        }
    }


    MapData GenerateMapData(Vector2 offsetOfChunk)
    {
        float[,] noiseMap = MakeNoiseValue.CreateNoiseMap(mapChunkSize, mapChunkSize, seed, scale, octave, lacunarity, persistance, offset + offsetOfChunk, normalizeMode);
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

        for (int y = 0; y < mapChunkSize; y++)
        {
            for(int x = 0; x < mapChunkSize; x++)
            {
                for(int i = 0; i < regions.Length; i++)
                {
                    if(noiseMap[x,y] < regions[i].height)
                    {
                        colorMap[y * mapChunkSize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        return new MapData(noiseMap, colorMap);  
    }

    public void RequestMapData(Vector2 chunkOffset, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(chunkOffset);
        mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
    }

    public void MapDataRecieved(Vector2 chunkOffset, Action<MapData> callback)
    {
        Thread t1 = new Thread(() =>
        {
            RequestMapData(chunkOffset, callback);
        });
    }


    public void RequestMeshData(MapData mapData, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrianMesh(mapData.heightMap, heightMult, heightCurve, levelOfDetail);
        meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
    }

    public void MeshdataRecieved(MapData mapData, Action<MeshData> callback)
    {
        Thread t2 = new Thread(() =>
        {
            RequestMeshData(mapData, callback);
        });
    }

    void Update()
    {
        while(mapDataThreadInfoQueue.Count > 0)
        {
            MapThreadInfo<MapData> mapThreadInfo;
            mapDataThreadInfoQueue.TryDequeue(out mapThreadInfo);
            mapThreadInfo.mapCallback(mapThreadInfo.typeOfMap);
        }

        while (meshDataThreadInfoQueue.Count > 0)
        {
            MapThreadInfo<MeshData> meshThreadInfo;
            meshDataThreadInfoQueue.TryDequeue(out meshThreadInfo);
            meshThreadInfo.mapCallback(meshThreadInfo.typeOfMap);
        }
    }

    void OnValidate()
    {
        if (scale < 0.1f)
            scale = 0.1f;
    }

    
    struct MapThreadInfo<T>
    {
        public Action<T> mapCallback;
        public T typeOfMap;

        public MapThreadInfo(Action<T> mapCallback, T typeOfMap)
        {
            this.mapCallback = mapCallback;
            this.typeOfMap = typeOfMap;
        }
    }

}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap)
    {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}
