using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField]
    Transform pointPrefab;

    [SerializeField, Range(10, 100)]
    int resolution = 10;

    Transform[] points;

    //static
    //private void Awake()
    //{
    //    float step = resolution / 2;
    //    Vector3 scale = Vector3.one / step;
    //    Vector3 position = Vector3.zero;
    //    for (int i = 0; i < resolution; i++)
    //    {
    //        Transform point = Instantiate(pointPrefab);
    //        //We want to limit the range to be from -1 through 1, rather than 0-9.
    //        //first they need to be scaled down (10 1unit cubes will not fit into a 2 unit (-1 to 1) space without overlapping
    //        //point.localScale = Vector3.one / 5;
    //        point.localScale = scale;

    //        //to bring the cubes back together, we need to divide the position by 5 too. 
    //        // to go from a 0-2 range to -1-1, we need to subtract 1 
    //        //as the position is centred on a cube though, and the cubes are 0.2 wide, the left side is at -1.1 and the right side is at 0.9
    //        //to fix we add 0.5
    //        //point.localPosition = Vector3.right * ((i + 0.5f) / 5 - 1);
    //        position.x = (i + 0.5f) / step - 1;
    //        position.y = position.x * position.x * position.x;
    //        point.localPosition = position;
    //        point.SetParent(transform, false);
    //    }
    //}

    //animated
    private void Awake()
    {
        float step = resolution / 2;
        Vector3 scale = Vector3.one / step;
        Vector3 position = Vector3.zero;

        points = new Transform[resolution];
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i] = Instantiate(pointPrefab);
            
            point.localScale = scale;
            
            position.x = (i + 0.5f) / step - 1;     
            point.localPosition = position;
            
            point.SetParent(transform, false);
        }
    }

    private void Update()
    {
        float time = Time.time;
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i];
            Vector3 position = point.localPosition;
            position.y = Mathf.Tan(Mathf.PI * (position.x - time)) * 0.1f;

            point.localPosition = position;
        }
    }
}
