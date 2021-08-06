using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Graph : MonoBehaviour
{
    [SerializeField]
    private Transform pointPrefab;
    
    [SerializeField,Range(10,100)]
    private int resolution = 10;

    [SerializeField] 
    private FunctionLibrary.FunctionName function;
    
    public enum TransitionMode
    {
        Cycle,
        Random
    }

    [SerializeField] private TransitionMode transitionMode;
    [SerializeField, Min(0f)] 
    private float functionDuration = 1f,transitionDuration = 1f;
    
    private Transform[] points;
    
    private float duration;
    private bool transitioning;
    private FunctionLibrary.FunctionName transitionFunxtion;
    void Awake()
    {
        float step = 2f / resolution;
        var scale = Vector3.one * step;
        points = new Transform[resolution * resolution];
        for (int i = 0; i < points.Length; i++)
        {
            var point = points[i] = Instantiate(pointPrefab);
            point.localScale = scale;
            point.SetParent(transform,false);
        } 
    }

    private void Update()
    {
        duration += Time.deltaTime;
        if (transitioning)
        {
            if (duration >= transitionDuration)
            {
                duration -= functionDuration;
                transitioning = false;
            }    
        }
        else if (duration >= functionDuration)
        {
            duration -= functionDuration;
            transitioning = true;
            transitionFunxtion = function;
            PickNextFunction();
        }

        if (transitioning)
        {
            UpdateFunctionTransition();
        }
        else
        {
            UpdateFunction();
        }
    }

    void PickNextFunction()
    {
        function = transitionMode == TransitionMode.Cycle
            ? FunctionLibrary.GetNextFunctionName(function)
            : FunctionLibrary.GetRandomFunctionName(function);
    }
    void UpdateFunction()
    {
        var f = FunctionLibrary.GetFunction(function);
        float time = Time.time;
        float step = 2f / resolution;
        var scale = Vector3.one * step;
        float v = 0.5f * step - 1f;
        for (int i = 0,x = 0, z = 0; i < points.Length; i++,x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            var u = (x + 0.5f) * step - 1f;
            points[i].localPosition = f(u,v,time);
            //points[i].localScale = scale;
        }
    }
    void UpdateFunctionTransition()
    {
        var from = FunctionLibrary.GetFunction(transitionFunxtion);
        var to = FunctionLibrary.GetFunction(function);
        float progress = duration / transitionDuration;
        float time = Time.time;
        float step = 2f / resolution;
        var scale = Vector3.one * step;
        float v = 0.5f * step - 1f;
        for (int i = 0,x = 0, z = 0; i < points.Length; i++,x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            var u = (x + 0.5f) * step - 1f;
            points[i].localPosition = FunctionLibrary.Morph(u, v, time, from, to, progress);
            //points[i].localScale = scale;
        }
    }
}
