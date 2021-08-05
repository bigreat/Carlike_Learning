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
    
    private Transform[] points;
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
    
    void Update()
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
}
