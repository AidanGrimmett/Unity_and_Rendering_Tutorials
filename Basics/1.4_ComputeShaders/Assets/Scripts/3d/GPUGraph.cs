using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUGraph : MonoBehaviour
{
    [SerializeField, Range(10, 200)]
    int resolution = 10;

    [SerializeField, Range(0.1f, 10f)]
    float speed = 2f;

    [SerializeField]
    FunctionLibrary3D.FunctionName function;


    [SerializeField, Range(0f, 10f)]
    float functionDuration = 1, transitionDuration = 1;
    float duration;
    bool transitioning;
    FunctionLibrary3D.FunctionName transitioningFunction;
    public enum TransitionMode { Cycle, Random }
    [SerializeField]
    TransitionMode transitionMode;

    ComputeBuffer positionsBuffer; // ComputeBuffers allocate space on the GPU

    private void OnEnable() //OnEnable is invoked every time the component is enabled, unlike awake() or start()
    {
        //construct a new ComputeBuffer, setting the amount of elements in the buffer (resolution * resolution)
        //  However, compute buffers hold abitrary untyped data, so the second argument (the stride) specifies
        //  the exact size of each element in bytes. Each position needs 3 float numbers, one float is 4 bytes so 
        //  we need 3 * 4 bytes as our stride.
        positionsBuffer = new ComputeBuffer(resolution * resolution, 3 * 4);
    }

    private void OnDisable()
    {
        positionsBuffer.Release();
        positionsBuffer = null; //opens it up to garbage collection
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
        else if (duration >= functionDuration && functionDuration > 0)
        {
            //starting a transition
            duration -= functionDuration;
            transitioning = true;
            transitioningFunction = function;
            GetNextFunction();
        }
    }

    private void GetNextFunction()
    {
        //if transition mode is cycle, function will equal GetNextFunctionName(function), otherwise GetRandom...(function)
        function = transitionMode == TransitionMode.Cycle ?
        FunctionLibrary3D.GetNextFunctionName(function) :
        FunctionLibrary3D.GetRandomFunctionNameOtherThan(function);
    }
}
