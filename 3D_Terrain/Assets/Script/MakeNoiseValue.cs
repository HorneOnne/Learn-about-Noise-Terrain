using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MakeNoiseValue
{
    public static float[,] CreateNoiseMap(int width, int height, int seed, float scale, int octave, float lacunarity, float persistance)
    {
        float[,] noiseMap = new float[width, height];
        Noise nosie = new Noise(seed);

        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;

       
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                float heightNoise = 0;
                float frequency = 1;
                float amplitude = 1;
                for (int i = 0; i < octave; ++i)
                {
                    float sampleX = (x - width / 2) / scale * frequency;
                    float sampleY = (y - height / 2) / scale * frequency;

                    float perlinValue = nosie.Eval(new Vec2f(sampleX, sampleY));
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
                noiseMap[x, y] = Mathf.InverseLerp(minHeight, maxHeight, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }
}
