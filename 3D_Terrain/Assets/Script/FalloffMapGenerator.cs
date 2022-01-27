using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffMapGenerator
{ 
    public static float[,] GenerateFalloffMap(int size, AnimationCurve animationCurve)
    {
        float[,] falloffMap = new float[size, size];

        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1;
                float y = j / (float)size * 2 - 1;

                float valueMap = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                falloffMap[i, j] = animationCurve.Evaluate(valueMap);
            }
        }

        return falloffMap;
    }
}
