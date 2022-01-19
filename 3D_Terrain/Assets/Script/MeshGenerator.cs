using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrianMesh(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        float pivotX = (width - 1) / - 2.0f;
        float pivotY = (height - 1) / 2.0f;
        MeshData meshdata= new MeshData(width, height);
        int vertIndex = 0;
        
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                meshdata.vertices[vertIndex] = new Vector3(pivotX + x, noiseMap[x, y], pivotY - y);
                meshdata.uvs[vertIndex] = new Vector2(x/ (float)width, y/(float)height);
                if(x < width -1 && y < height - 1)
                {
                    meshdata.AddTriangle(vertIndex, vertIndex + width + 1, vertIndex + width);
                    meshdata.AddTriangle(vertIndex, vertIndex + 1, vertIndex + width + 1);
                }
                vertIndex++;
            }
        }
        return meshdata;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public Vector2[] uvs;
    public int[] triangles;

    int triangleIndex = 0;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        triangles = new int[(meshWidth-1) * (meshHeight-1) * 6];
        uvs = new Vector2[meshWidth * meshWidth];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}
