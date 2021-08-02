using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractal : MonoBehaviour
{
    [SerializeField, Range(1, 8)] 
    private int depth = 4;

    #region Old Fractal

    // private void Start()
    // {
    //     name = "Fractal" + depth;
    //     if (depth <= 1) return;
    //     var childA = CreateChild(Vector3.up,Quaternion.identity);
    //     var childB = CreateChild(Vector3.right,Quaternion.Euler(0,0,-90f));
    //     var childC = CreateChild(Vector3.left,Quaternion.Euler(0,0,90f));
    //     var childD = CreateChild(Vector3.forward, Quaternion.Euler(90f, 0f, 0f));
    //     var childE = CreateChild(Vector3.back, Quaternion.Euler(-90f, 0f, 0f));
    //     // var childF = CreateChild(Vector3.down, Quaternion.Euler(0f, 90f, 0f));
    //     childA.transform.SetParent(transform, false);
    //     childB.transform.SetParent(transform,false);
    //     childC.transform.SetParent(transform,false);
    //     childD.transform.SetParent(transform,false);
    //     childE.transform.SetParent(transform,false);
    //     // childF.transform.SetParent(transform,false);
    // }
    //
    // Fractal CreateChild(Vector3 dir,Quaternion rotation)
    // {
    //     var child = Instantiate(this);
    //     child.depth -= 1;
    //     child.transform.localPosition = 0.75f * dir;
    //     child.transform.localRotation = rotation;
    //     child.transform.localScale = 0.5f * Vector3.one;
    //     return child;
    // }
    //
    // private void Update()
    // {
    //     transform.Rotate(0,22.5f * Time.deltaTime,0);
    // }
    #endregion
    [SerializeField]
    private Mesh m_Mesh;
    [SerializeField]
    private Material m_Material;

    private static Vector3[] directinos =
    {
        Vector3.up, Vector3.right, Vector3.left, Vector3.forward, Vector3.back
    };

    private static Quaternion[] rotations =
    {
        Quaternion.identity,
        Quaternion.Euler(0, 0, -90),
        Quaternion.Euler(0, 0, 90),
        Quaternion.Euler(90, 0, 0),
        Quaternion.Euler(-90, 0, 0),
    };

    struct FractalPart
    {
        public Vector3 dir,worldPosition;
        public Quaternion rot,worldRotation;
        public float spinAngle;
    }
    private FractalPart[][] Parts;
    private Matrix4x4[][] Matrices;
    private ComputeBuffer[] MatricesBuffers;
    private void OnEnable()
    {
        Parts = new FractalPart[depth][];
        Matrices = new Matrix4x4[depth][];
        MatricesBuffers = new ComputeBuffer[depth];
        int stride = 16 * 4;
        for (int i = 0,length = 1; i < depth; i++,length *= 5)
        {
            Parts[i] = new FractalPart[length];
            Matrices[i] = new Matrix4x4[length];
            MatricesBuffers[i] = new ComputeBuffer(length, stride);
        }
        Parts[0][0] = CreatePart(0);
        for (int i = 1; i < depth; i++)
        {
            var l = Parts[i];
            for (int j = 0; j < l.Length; j += 5)
            {
                for (int k = 0; k < 5; k++)
                {
                   l[j + k]  = CreatePart(k);
                }
            }
        }
    }

    private void Update()
    {
        float spinAngleDelta = 22.5f * Time.deltaTime;
        FractalPart root = Parts[0][0];
        root.spinAngle += spinAngleDelta;
        root.worldRotation = root.rot * Quaternion.Euler(0,root.spinAngle,0);
        Parts[0][0] = root;
        Matrices[0][0] = Matrix4x4.TRS(root.worldPosition,root.worldRotation,Vector3.one);
        float scale = 1;
        for (int i = 1; i < Parts.Length; i++)
        {
            scale *= 0.5f;
            FractalPart[] levelParts = Parts[i];
            FractalPart[] parentParts = Parts[i - 1];
            Matrix4x4[] levelMatrices = Matrices[i];
            for (int j = 0; j < levelParts.Length; j++)
            {
                var parent = parentParts[j / 5];
                var part = levelParts[j];
                part.spinAngle += spinAngleDelta;
                part.worldRotation = parent.worldRotation * (part.rot * Quaternion.Euler(0,part.spinAngle,0));
                part.worldPosition = parent.worldPosition + parent.worldRotation * (1.5f * scale * part.dir);
                levelParts[j] = part;
                levelMatrices[j] = Matrix4x4.TRS(part.worldPosition,part.worldRotation,scale * Vector3.one);
            }
        }

        for (int i = 0; i < MatricesBuffers.Length; i++)
        {
            MatricesBuffers[i].SetData(Matrices[i]);
        }
    }

    FractalPart CreatePart(int levelIndex,int childIndex,float scale)
    {
        var go = new GameObject("Fractal Part: L" + levelIndex + " C" + childIndex);
        go.transform.localScale = scale * Vector3.one;
        go.transform.SetParent(transform,false);
        go.AddComponent<MeshFilter>().mesh = m_Mesh;
        go.AddComponent<MeshRenderer>().material = m_Material;
        return new FractalPart
        {
            dir =  directinos[childIndex],
            rot = rotations[childIndex],
        };
    }
    FractalPart CreatePart(int childIndex)
    {
        return new FractalPart
        {
            dir =  directinos[childIndex],
            rot = rotations[childIndex],
        };
    }

    private void OnDisable()
    {
        for (int i = 0; i < MatricesBuffers.Length; i++)
        {
            MatricesBuffers[i].Release();
        }
        Parts = null;
        Matrices = null;
        MatricesBuffers = null;
    }

    private void OnValidate()
    {
        if (Parts != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }
}
