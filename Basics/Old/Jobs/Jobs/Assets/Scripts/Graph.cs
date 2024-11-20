using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField]
    Transform pointPrefab;

    [SerializeField, Range(10, 250)]
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

    Transform[] points;

    private void Awake()
    {
        points = new Transform[0];
    }

    private void Update()
    {
        if (functionDuration > 0)
        {
            duration += Time.deltaTime;
            if (transitioning)
            {
                if (duration >= transitionDuration)
                {
                    duration -= transitionDuration;
                    transitioning = false;
                }
            }
            else if (duration >= functionDuration)
            {
                PickFunction();
            }
        }
        //getting the correct delegate from the delegate array (uses an enum rather than index) 
        FunctionLibrary.Function f = FunctionLibrary.GetFunction(function);
        if (resolution * resolution != points.Length)
            RefreshResolution();

        if (transitioning)
        {
            UpdateTransition();
        }
        else
        {
            UpdateFunction(f);
        }
    }

    private void RefreshResolution()
    {
        DeletePoints();

        float step = 2f / resolution;
        Vector3 scale = Vector3.one * step;

        points = new Transform[resolution * resolution];
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i] = Instantiate(pointPrefab);

            point.localScale = scale;

            point.SetParent(transform, false);
        }
    }

    private void DeletePoints()
    {
        for (int i = 0; i < points.Length; i++)
        {
            Destroy(points[i].gameObject);
        }
    }

    void PickFunction()
    {
        duration -= functionDuration;
        transitioning = true;
        transitionFunction = function;
        //function to perform equals (bool check of current transitionmode) next in cycle or random
        function = transitionMode == TransitionMode.Cycle ? FunctionLibrary.GetNextFunctionName(function) : FunctionLibrary.GetRandomFunctionName(function);
    }

    void UpdateFunction(FunctionLibrary.Function f)
    {
        float time = Time.time;
        float step = 2f / resolution;
        float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            //invoking the method specified
            float u = (x + 0.5f) * step - 1f;
            points[i].localPosition = f(u, v, time);
        }
    }

    void UpdateTransition()
    {
        FunctionLibrary.Function from = FunctionLibrary.GetFunction(transitionFunction), to = FunctionLibrary.GetFunction(function);
        float progress = duration / transitionDuration;
        float time = Time.time;
        float step = 2f / resolution;
        float v = 0.5f * step - 1f;
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }
            //invoking the method specified
            float u = (x + 0.5f) * step - 1f;
            points[i].localPosition = FunctionLibrary.Morph(u, v, time, from, to, progress);
        }
    }
}
