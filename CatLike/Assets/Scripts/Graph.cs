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
        Vector3 position = Vector3.zero;
        var scale = Vector3.one * step;
        points = new Transform[resolution * resolution];
        for (int i = 0,x = 0; i < points.Length; i++,x++)
        {
            var point = points[i] = Instantiate(pointPrefab);
            position.x = (i + 0.5f) * step - 1f;
            point.localPosition = position;
            point.localScale = scale;
            point.SetParent(transform,false);
        }
    }
    
    void Update()
    {
        var f = FunctionLibrary.GetFunction(function);
        float time = Time.time;
        for (int i = 0; i < points.Length; i++)
        {
            var point = points[i];
            var position = point.localPosition;
            position.y = f(position.x, position.z, time);
            point.localPosition = position;
        }
    }
}
