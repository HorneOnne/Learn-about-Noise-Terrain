using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25;


    public static float maxViewDist = 350;

    public Transform viewer;
    public Material mapMaterial;
    public Material chunkMaterial;

    public static Vector2 viewerPos;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunkVisibleInViewDist;
    public Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunkVisibleLastUpdate = new List<TerrainChunk>();


    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunkVisibleInViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);
    }

    private void Update()
    {
        viewerPos = new Vector2(viewer.position.x, viewer.position.z);

        UpdateVibibleChunk();

    }
    void UpdateVibibleChunk()
    {
        for (int i = 0; i < terrainChunkVisibleLastUpdate.Count; i++)
        {
            terrainChunkVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunkVisibleLastUpdate.Clear();
     

        int currentChunkCoordX = Mathf.RoundToInt(viewerPos.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPos.y / chunkSize);

        for (int yOffset = -chunkVisibleInViewDist; yOffset <= chunkVisibleInViewDist; yOffset++)
        {
            for (int xOffset = -chunkVisibleInViewDist; xOffset <= chunkVisibleInViewDist; xOffset++)
            {
                Vector2 terrainChunkKey = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(terrainChunkKey))
                {
                    terrainChunkDictionary[terrainChunkKey].UpdateTerrainChunk();
                    if (terrainChunkDictionary[terrainChunkKey].IsVisible())
                    {
                        terrainChunkVisibleLastUpdate.Add(terrainChunkDictionary[terrainChunkKey]);
                    }
                    
                }
                else
                {
                    terrainChunkDictionary.Add(terrainChunkKey, new TerrainChunk(terrainChunkKey, chunkSize, transform, chunkMaterial));
                }
            }
        }

    }
    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MapData mapData;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;


        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material)
        {

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

 
            meshObject = new GameObject("Chunk");
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3;

            meshObject.transform.parent = parent;
            mapGenerator.RequestMapData(position, MapDataRecieved);
            SetVisible(false);
           
        }

    
        public void MapDataRecieved(MapData mapData)
        {
            mapGenerator.RequestMeshData(mapData, MeshDataRecieved);
            Texture2D texuture = TextureGenerator.GenerateColorTexture(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texuture;
        }

        public void MeshDataRecieved(MeshData meshData)
        {
            meshFilter.mesh = meshData.GenerateMesh();
            
        }

        public void UpdateTerrainChunk()
        {
            float viewDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPos));
           // float viewDstFromNearestEdge = (viewerPos - this.position).magnitude;
            bool visible = viewDstFromNearestEdge <= maxViewDist;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
 
}


