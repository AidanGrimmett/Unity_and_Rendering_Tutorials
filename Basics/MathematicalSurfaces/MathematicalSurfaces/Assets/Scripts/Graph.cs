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

    Transform[] points;

    private void Awake()
    {
        points = new Transform[0];
    }

    private void Update()
    {
        //getting the correct delegate from the delegate array (uses an enum rather than index) 
        FunctionLibrary.Function f = FunctionLibrary.GetFunction(function);
        if (resolution * resolution != points.Length)
            RefreshResolution();

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
}
