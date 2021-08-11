using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GPUGraph : MonoBehaviour
{
    private const int maxResolution = 1000;
    [SerializeField] private ComputeShader computeShader;
    [SerializeField] private Material material;
    [SerializeField] private Mesh mesh;
    [SerializeField,Range(10,maxResolution)]
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
    
    private float duration;
    private bool transitioning;
    private FunctionLibrary.FunctionName TransitionFunction;

    private ComputeBuffer postionBuffer;

    static readonly int
        positionsId = Shader.PropertyToID("_Positions"),
        resolutionId = Shader.PropertyToID("_Resolution"),
        stepId = Shader.PropertyToID("_Step"),
        timeId = Shader.PropertyToID("_Time"),
        transitionProgressId = Shader.PropertyToID("_TransitionProgress");

    void UpdateFunctionOnGPU()
    {
        float step = 2f / resolution;
        computeShader.SetInt(resolutionId,resolution);
        computeShader.SetFloat(stepId,step);
        computeShader.SetFloat(timeId,Time.time);
        if (transitioning)
        {
            computeShader.SetFloat(transitionProgressId, Mathf.SmoothStep(0f, 1f, duration / transitionDuration));
        }
        var kernelIndex = (int)function + (int)(transitioning ? TransitionFunction : function) * FunctionLibrary.FunctionCount;
        computeShader.SetBuffer(kernelIndex,positionsId,postionBuffer);
        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(kernelIndex, groups, groups, 1);
        material.SetBuffer(positionsId,postionBuffer);
        material.SetFloat(stepId, step);
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution)); 
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds,resolution * resolution);
    }
    private void OnEnable()
    {
        postionBuffer = new ComputeBuffer(maxResolution * maxResolution,3 * 4);
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
            TransitionFunction = function;
            PickNextFunction();
        }

        UpdateFunctionOnGPU();
    }

    void PickNextFunction()
    {
        function = transitionMode == TransitionMode.Cycle
            ? FunctionLibrary.GetNextFunctionName(function)
            : FunctionLibrary.GetRandomFunctionName(function);
    }

    private void OnDisable()
    {
        postionBuffer.Release();
        postionBuffer = null;
    }
}
