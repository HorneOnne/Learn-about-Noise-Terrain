using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MakeNoiseValue
{
    public enum NormalizeMode
    {
        Local,
        Global
    }

    public static float[,] CreateNoiseMap(int width, int height, int seed, float scale, int octave, float lacunarity, float persistance, Vector2 offset, NormalizeMode normalizeMode)
    {
        System.Random rand = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octave];
        float frequency = 1;
        float amplitude = 1;
        float maxPossibleHeight = 0;

        for (int i = 0; i < octave; i++)
        {
            float offsetX = rand.Next(-100000, 100000) + offset.x;
            float offsetY = rand.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }
        float[,] noiseMap = new float[width, height];
        Noise nosie = new Noise(seed);

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

       
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                float heightNoise = 0;
                frequency = 1;
                amplitude = 1;
                for (int i = 0; i < octave; ++i)
                {
                    float sampleX = (x - (width / 2) + octaveOffsets[i].x) / scale * frequency ;
                    float sampleY = (y - (height / 2) - octaveOffsets[i].y) / scale * frequency ;

                    //float perlinValue = nosie.Eval(new Vec2f(sampleX, sampleY));
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    heightNoise += perlinValue * amplitude;

                    frequency *= lacunarity;
                    amplitude *= persistance;
                }

                if (heightNoise > maxHeight)
                    maxHeight = heightNoise;
                else if (heightNoise < minHeight)
                    minHeight = heightNoise;

                noiseMap[x, y] = heightNoise;
            }
        }

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                if(normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minHeight, maxHeight, noiseMap[x, y]);
                }
                else
                {
                    float normalizeHeight = (noiseMap[x, y] + 1) / (2 * maxPossibleHeight / 1.75f);
                        
                    noiseMap[x, y] = normalizeHeight;
                }
          
            }
            
        }

        return noiseMap;
    }
}
