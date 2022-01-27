using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class EndlessTerrain : MonoBehaviour
{
    public LODInfo[] detailLevels;

    public static float maxViewDist;

    public Transform viewer;
    public Material mapMaterial;
    public Material chunkMaterial;

    public static Vector2 viewerPos;
    private Vector2 previousPos;

    static MapGenerator mapGenerator;
    int chunkSize;
    int chunkVisibleInViewDist;
    public Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunkVisibleLastUpdate = new List<TerrainChunk>();


    private void Start()
    {
        maxViewDist = detailLevels[detailLevels.Length - 1].threadholds;
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunkVisibleInViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);
        previousPos = viewerPos;
        UpdateVibibleChunk();
    }

    private void Update()
    {
        viewerPos = new Vector2(viewer.position.x, viewer.position.z);

        if((previousPos -  viewerPos).SqrMagnitude() > 200)
        {
            previousPos = viewerPos;
            UpdateVibibleChunk();
        }
        

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
                }
                else
                {
                    terrainChunkDictionary.Add(terrainChunkKey, new TerrainChunk(terrainChunkKey, chunkSize, detailLevels, transform, chunkMaterial));
                }
            }
        }

    }
    public class TerrainChunk
    {
        public LODInfo[] levelOfDetails;
        public LODMesh[] lodMeshes;

        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MapData mapData;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] levelOfDetails, Transform parent, Material material)
        {
            this.levelOfDetails = levelOfDetails;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);


            meshObject = new GameObject("Chunk");
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;

            lodMeshes = new LODMesh[levelOfDetails.Length];

            for (int i = 0; i < levelOfDetails.Length; i++)
            {
                lodMeshes[i] = new LODMesh(levelOfDetails[i].lod, UpdateTerrainChunk);
            }

            mapGenerator.RequestMapData(position, MapDataRecieved);
       
            SetVisible(false);

        }


        public void MapDataRecieved(MapData mapData)
        {
            this.mapData = mapData;
            Texture2D texture = TextureGenerator.GenerateColorTexture(this.mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }



        public void UpdateTerrainChunk()
        {
            float viewDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPos));
            // float viewDstFromNearestEdge = (viewerPos - this.position).magnitude;
            bool visible = viewDstFromNearestEdge <= maxViewDist;

            if (visible)
            {
                int index = 0;

                for (int i = 0; i < levelOfDetails.Length - 1; i++)
                {
                    if (levelOfDetails[i].threadholds < viewDstFromNearestEdge)
                    {
                        index = i + 1;
                    }
                    else
                        break;
                }

                LODMesh newLODMesh = lodMeshes[index];
                if (newLODMesh.hasMesh)
                {
                    meshFilter.mesh = newLODMesh.mesh;
                }
                else if (!newLODMesh.hasRequestMesh)
                {
                    newLODMesh.RequestMesh(mapData);
                }
                terrainChunkVisibleLastUpdate.Add(this);
            }

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

    public class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestMesh;
        public bool hasMesh;
        public int lod;
        System.Action updateTerrainChunkCallback;

        public LODMesh(int lod, System.Action updateTerrainChunkCallback)
        {
            this.lod = lod;
            this.updateTerrainChunkCallback = updateTerrainChunkCallback;
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataRecieved);

        }

        public void OnMeshDataRecieved(MeshData meshData)
        {
            hasMesh = true;
            mesh = meshData.GenerateMesh();

            updateTerrainChunkCallback();
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public float threadholds;
        public int lod;
    }

}


