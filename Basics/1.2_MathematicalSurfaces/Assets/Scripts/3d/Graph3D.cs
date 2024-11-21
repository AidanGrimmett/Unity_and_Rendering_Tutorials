using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph3D : MonoBehaviour
{
    [SerializeField]
    Transform pointPrefab;

    [SerializeField, Range(10, 100)]
    int resolution = 10;

    [SerializeField, Range(0.1f, 10f)]
    float speed = 2f;

    [SerializeField]
    FunctionLibrary3D.FunctionName function;

    Transform[] points;


    private void Awake()
    {
        //size of each step from one iteration to the next. this changes the x position of each point as well as the scale.
        float step = 2f / resolution; //(this is the same as doing (resolution / 2), then dividing by step instead of multiplying by step when we apply it)

        //scale is never changed, so can be set once outside the loop to avoid extra calculations.
        Vector3 scale = Vector3.one * step;

        points = new Transform[resolution * resolution];
        for (int i = 0; i < points.Length; i++)
        {
            //instantiate prefab
            Transform point = points[i] = Instantiate(pointPrefab, transform);

            //set scale
            point.localScale = scale;
        }
    }

    private void Update()
    {
        //invoke Time.time once outside of the loop ( ~EFFICIENCY~ )
        float t = Time.time;

        //get desired function method from FunctionLibrary
        FunctionLibrary3D.Function f = FunctionLibrary3D.GetFunction(function);

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

            Transform point = points[i];

            float u = (x + 0.5f) * step - 1f;

            //set position 
            point.localPosition = f(u, v, t, speed);
        }
    }
}
