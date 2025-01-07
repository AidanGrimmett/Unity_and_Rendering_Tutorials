using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField]
    Transform pointPrefab;

    [SerializeField, Range(10, 100)]
    int resolution = 10;

    [SerializeField, Range(0.1f, 10f)]
    float speed = 2f;

    [SerializeField]
    FunctionLibrary.FunctionName function;

    Transform[] points;


    private void Awake()
    {
        //we never change the Z position of the point, so we can initialise the position struct outside of the loop and only change the x and y components
        Vector3 position = Vector3.zero;

        //size of each step from one iteration to the next. this changes the x position of each point as well as the scale.
        float step = 2f / resolution; //(this is the same as doing (resolution / 2), then dividing by step instead of multiplying by step when we apply it)

        //scale is never changed, so can be set once outside the loop to avoid extra calculations.
        Vector3 scale = Vector3.one * step;

        points = new Transform[resolution];
        for (int i = 0; i < points.Length; i++)
        {
            //instantiate prefab
            Transform point = points[i] = Instantiate(pointPrefab, transform);
            //set x position using step
            position.x = (i + 0.5f) * step - 1f;
            //set y position (the function we are drawing)
            //position.y = Mathf.Pow(position.x, 3); (static function, moving to animating the function so don't need this anymore)

            //set position and scale
            point.localPosition = position;
            point.localScale = scale;
        }
    }

    private void Update()
    {
        //invoke Time.time once outside of the loop ( ~EFFICIENCY~ )
        float t = Time.time;

        //get desired function method from FunctionLibrary
        FunctionLibrary.Function f = FunctionLibrary.GetFunction(function);

        for (int i = 0; i < points.Length; i++)
        {
            //get reference to point, get position
            Transform point = points[i];

            //get local position variable
            Vector3 position = point.localPosition;
            //change y position
            position.y = f(position.x, t, speed);
            point.localPosition = position;
        }
    }
}
