﻿using UnityEngine;
using static UnityEngine.Mathf;
public static class FunctionLibrary
{
    public delegate float Function(float x, float z, float t);
    public enum FunctionName
    {
        Wave,
        MultiWave,
        Ripple
    }

    private static Function[] functions = { Wave, MultiWave, Ripple};

    public static Function GetFunction(FunctionName name)
    {
        return functions[(int)name];
    }
    
    public static float Wave(float x, float z,float t)
    {
        return Sin(PI * (x + t));
    }
    
    public static float MultiWave(float x, float z,float t)
    {
        var y = Sin(PI * (x + 0.5f * t));
        y += Sin(2f * PI * (x + t)) * 0.5f;
        return y * (2f / 3f);
    }

    public static float Ripple(float x, float z, float t)
    {
        var d = Abs(x);
        var y = Sin( PI * (4f * d - t));
        return y / (1 + 10f *d);
    }
}