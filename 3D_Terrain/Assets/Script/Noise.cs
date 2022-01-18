using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vec2f
{
    public float x, y;
    public Vec2f()
    {
        this.x = 0.0f;
        this.y = 0.0f;
    }

    public Vec2f(float xx, float yy)
    {
        this.x = xx;
        this.y = yy;
    }

    public static Vec2f operator -(Vec2f v1, Vec2f v2)
        => new Vec2f(v1.x - v2.x, v1.y - v2.y);
    public static Vec2f operator *(Vec2f vec, float v)
        => new Vec2f(vec.x * v, vec.y * v);

    public override string ToString()
        => $"[{this.x}] - [{this.y}]";
}


public class Noise
{
    const int kMaxTableSize = 256;
    const int kMaxTalbeSizeMask = kMaxTableSize - 1;
    float[] r = new float[kMaxTableSize * kMaxTableSize];

    public Noise(int seed = 2021)
    {
        System.Random random = new System.Random(seed);

        for (int i = 0; i < kMaxTableSize; i++)
        {
            for (int j = 0; j < kMaxTableSize; j++)
            {
                r[i * kMaxTableSize + j] = (float)random.NextDouble();
            }
        }
    }


    private float SmoothValue(float v)
    {
        return v * v * (3 - 2 * v);
    }

    public float Eval(Vec2f vec)
    {
        int lastX = (int)Math.Floor(vec.x);
        int lastY = (int)Math.Floor(vec.y);

        float tx = vec.x - lastX;
        float ty = vec.y - lastY;

        int rx0 = lastX & kMaxTalbeSizeMask;
        int rx1 = (lastX + 1) & kMaxTalbeSizeMask;
        int ry0 = lastY & kMaxTalbeSizeMask;
        int ry1 = (lastY + 1) & kMaxTalbeSizeMask;

        float c00 = r[ry0 * kMaxTableSize + rx0];
        float c01 = r[ry0 * kMaxTableSize + rx1];
        float c10 = r[ry1 * kMaxTableSize + rx0];
        float c11 = r[ry1 * kMaxTableSize + rx1];



        float nx0 = Mathf.Lerp(c00, c01, SmoothValue(tx));
        float nx1 = Mathf.Lerp(c10, c11, SmoothValue(tx));



        return Mathf.Lerp(nx0, nx1, SmoothValue(ty));
    }  
}

