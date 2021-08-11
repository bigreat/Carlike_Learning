﻿using System;
using UnityEngine;
using static UnityEngine.Mathf;
using Random = UnityEngine.Random;

public static class FunctionLibrary
{
    public delegate Vector3 Function(float u, float v, float t);
    public enum FunctionName
    {
        Wave,
        MultiWave,
        Ripple,
        Sphere,
        Torus,
    }

    public static int FunctionCount => Enum.GetValues(typeof(FunctionName)).Length;

    private static Function[] functions = { Wave, MultiWave, Ripple,Sphere,Torus};

    public static FunctionName GetNextFunctionName(FunctionName name)
    {
        return (int)name < FunctionCount - 1 ? name + 1 : 0;
    }

    public static FunctionName GetRandomFunctionName(FunctionName name)
    {
        var res = (FunctionName)Random.Range(1, FunctionCount - 1);
        return res == name ? 0 : res;
    }

    public static Vector3 Morph(float u, float v, float t, Function form, Function to, float progress)
    {
        return Vector3.LerpUnclamped(form(u, v, t), to(u, v, t), SmoothStep(0f, 1f, progress));
    }
    public static Function GetFunction(FunctionName name) => functions[(int)name];
    
    public static Vector3 Wave(float u, float v,float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + v + t));;
        p.z = v;
        return p;
    }
    
    public static Vector3 MultiWave(float u, float v,float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + 0.5f * t));
        p.y += Sin(2f * PI * (v + t)) * 0.5f;
        p.y += Sin(PI * (u + v + 0.25f * t));
        p.y *= (1f / 2.5f);
        p.z = v;
        return p;
    }

    public static Vector3 Ripple(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        var d = Sqrt(u * u + v * v);
        p.y = Sin( PI * (4f * d - t));
        p.y /= (1 + 10f * d);
        p.z = v;
        return p;
    }

    public static Vector3 Sphere(float u, float v, float t)
    {
        Vector3 p;
        float r = 0.9f + 0.1f * Sin(PI * (6 * u + 4 * v + t));
        float s = r * Cos(0.5f * PI * v);
        p.x = s * Sin(PI * u);
        p.y = r * Sin(0.5f * PI * v);;
        p.z = s * Cos(PI * u);
        return p;
    }

    public static Vector3 Torus(float u, float v, float t)
    {
        Vector3 p;
        // float r = 1;
        float r1 = 0.7f + 0.1f * Sin(6 * u + 0.5f * t);
        float r2 = 0.15f -.05f * Sin(PI *(8 * u + 4f * v + 2f * t));
        float s =r1 + r2 * Cos(PI * v);
        p.x = s * Sin(PI * u);
        p.y = r2 * Sin(PI * v);
        p.z = s * Cos(PI * u);
        return p;
    }
}
