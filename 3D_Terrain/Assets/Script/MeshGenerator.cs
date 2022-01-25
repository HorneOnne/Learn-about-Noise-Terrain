using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrianMesh(float[,] noiseMap, float heightMult, AnimationCurve heightCurve, int levelOfDetail)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        float pivotX = (width - 1) / - 2.0f;
        float pivotY = (height - 1) / 2.0f;

        int meshSimplificationIncrement = (levelOfDetail ==0)? 1: levelOfDetail * 2;
        int verticesPerLine = (width - 1) / meshSimplificationIncrement +1;

        MeshData meshdata= new MeshData(verticesPerLine, verticesPerLine);
        int vertIndex = 0;
        
        for (int y = 0; y < height; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x+=meshSimplificationIncrement)
            {
                meshdata.vertices[vertIndex] = new Vector3(pivotX + x, heightCurve.Evaluate(noiseMap[x,y]) * heightMult, pivotY - y);
                meshdata.uvs[vertIndex] = new Vector2(x/ (float)width, y/(float)height);
                if(x < width -1 && y < height - 1)
                {
                    meshdata.AddTriangle(vertIndex, vertIndex + verticesPerLine + 1, vertIndex + verticesPerLine);
                    meshdata.AddTriangle(vertIndex, vertIndex + 1, vertIndex + verticesPerLine + 1);
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
