using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUGraph : MonoBehaviour
{
    const int maxResolution = 1000;

    [SerializeField, Range(10, maxResolution)]
    int resolution = 100;

    [SerializeField, Range(0.1f, 10f)]
    float speed = 2f;

    [SerializeField]
    FunctionLibrary3D.FunctionName function;

    public bool enableTransition;


    [SerializeField, Range(0f, 10f)]
    float functionDuration = 2f, transitionDuration = 2f;

    float duration;
    bool transitioning;
    FunctionLibrary3D.FunctionName transitioningFunction;
    public enum TransitionMode { Cycle, Random }
    [SerializeField]
    TransitionMode transitionMode;

    [SerializeField]
    ComputeShader computeShader;

    ComputeBuffer positionsBuffer; // ComputeBuffers allocate space on the GPU

    [SerializeField]
    Mesh mesh;

    [SerializeField]
    Material material;

    private SettingsController settings;

    static readonly int 
        positionsId = Shader.PropertyToID("_Positions"),
        resolutionId = Shader.PropertyToID("_Resolution"),
        stepId = Shader.PropertyToID("_Step"),
        timeId = Shader.PropertyToID("_Time"),
        speedId = Shader.PropertyToID("_Speed"),
        transitionProgressId = Shader.PropertyToID("_TransitionProgress");

    private void OnEnable() //OnEnable is invoked every time the component is enabled, unlike awake() or start()
    {
        //construct a new ComputeBuffer, setting the amount of elements in the buffer (resolution * resolution)
        //  However, compute buffers hold abitrary untyped data, so the second argument (the stride) specifies
        //  the exact size of each element in bytes. Each position needs 3 float numbers, one float is 4 bytes so 
        //  we need 3 * 4 bytes as our stride.
        positionsBuffer = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
        settings = FindObjectOfType<SettingsController>();
    }

    private void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null; //opens it up to garbage collection
    }

    void UpdateFunctionOnGPU()
    {
        float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);
        computeShader.SetFloat(speedId, speed);
        if (transitioning)
        {
            computeShader.SetFloat(transitionProgressId, Mathf.SmoothStep(0f, 1f, duration / transitionDuration));
        }

        var kernelIndex = (int)function + (int)(transitioning ? transitioningFunction : function) * FunctionLibrary3D.FunctionCount;
        computeShader.SetBuffer(kernelIndex, positionsId, positionsBuffer);


        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(kernelIndex, groups, groups, 1);

        material.SetBuffer(positionsId, positionsBuffer);
        material.SetFloat(stepId, step);

        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * resolution);
    }

    private void Update()
    {
        duration += Time.unscaledDeltaTime;

        //if transitioning, check progress and if done disable
        if (transitioning)
        {
            if (duration >= transitionDuration)
            {
                duration -= transitionDuration;
                transitioning = false;
            }
        }
        else if (duration >= functionDuration && enableTransition)
        {
            //starting a transition
            duration -= functionDuration;
            transitioning = true;
            transitioningFunction = function;
            GetNextFunction();
        }

        UpdateFunctionOnGPU();
    }

    private void GetNextFunction()
    {
        //if transition mode is cycle, function will equal GetNextFunctionName(function), otherwise GetRandom...(function)
        function = transitionMode == TransitionMode.Cycle ?
        FunctionLibrary3D.GetNextFunctionName(function) :
        FunctionLibrary3D.GetRandomFunctionNameOtherThan(function);

        settings.UpdateFunctionDisplay((int)function); //update the dropdown on settings panel
    }

    public void SetResolution(int res)
    {
        resolution = Mathf.Clamp(res, 0, maxResolution);
    }

    public void SetSpeed(float spd)
    {
        speed = spd;
    }

    public void SetDuration(float dur)
    {
        functionDuration = dur;
    }

    public void SetTransitionDuration(float dur)
    {
        transitionDuration = dur;
    }

    public void SetFunction(FunctionLibrary3D.FunctionName funcName)
    {
        //if (enableTransition)
        //{
        //    Debug.Log("Transition!");
        //    duration -= functionDuration;
        //    transitioning = true;
        //    transitioningFunction = function;
        //}
        function = funcName;
    }
}
