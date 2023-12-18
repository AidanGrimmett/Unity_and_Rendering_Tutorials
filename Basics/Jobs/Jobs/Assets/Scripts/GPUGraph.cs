using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUGraph : MonoBehaviour
{
    [SerializeField]
    ComputeShader computeShader;

    [SerializeField]
    Material material;

    [SerializeField]
    Mesh mesh;

    [SerializeField, Range(10, 1000)]
    int resolution = 10;

    [SerializeField]
    FunctionLibrary.FunctionName function;

    public enum TransitionMode { Cycle, Random }

    [SerializeField]
    TransitionMode transitionMode;

    [SerializeField, Min(0f)]
    float functionDuration = 1f, transitionDuration = 1f;

    private float duration;
    bool transitioning;
    FunctionLibrary.FunctionName transitionFunction;

    //GPU stuff
    ComputeBuffer positionsBuffer;

    static readonly int 
        positionsId = Shader.PropertyToID("_Positions"),
        resolutionId = Shader.PropertyToID("_Resolution"),
        stepId = Shader.PropertyToID("_Step"),
        timeId = Shader.PropertyToID("_Time");


    private void OnEnable()
    {
        positionsBuffer = new ComputeBuffer(resolution * resolution, 3 * 4);
    }

    private void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null;
    }

    private void Update()
    {
        //if (functionDuration > 0)
        //{
        //    duration += Time.deltaTime;
        //    if (transitioning)
        //    {
        //        if (duration >= transitionDuration)
        //        {
        //            duration -= transitionDuration;
        //            transitioning = false;
        //        }
        //    }
        //    else if (duration >= functionDuration)
        //    {
        //        PickFunction();
        //    }
        //}
        ////getting the correct delegate from the delegate array (uses an enum rather than index) 
        //FunctionLibrary.Function f = FunctionLibrary.GetFunction(function);
        UpdateFunctionOnGPU();
    }

    void UpdateFunctionOnGPU()
    {
        float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);

        computeShader.SetBuffer(0, positionsId, positionsBuffer);
        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(0, groups, groups, 1);

        material.SetBuffer(positionsId, positionsBuffer);
        material.SetFloat(stepId, step);
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, positionsBuffer.count);
    }

    void PickFunction()
    {
        duration -= functionDuration;
        transitioning = true;
        transitionFunction = function;
        //function to perform equals (bool check of current transitionmode) next in cycle or random
        function = transitionMode == TransitionMode.Cycle ? FunctionLibrary.GetNextFunctionName(function) : FunctionLibrary.GetRandomFunctionName(function);
    }
}
