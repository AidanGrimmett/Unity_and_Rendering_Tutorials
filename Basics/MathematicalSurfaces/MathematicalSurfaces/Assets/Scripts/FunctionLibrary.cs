using UnityEngine;
using static UnityEngine.Mathf;

public static class FunctionLibrary
{
    //Delegates are references to methods. 
    public delegate Vector3 Function(float u, float v, float t);
    //we can make an array of delegates, so that different methods can easily be invoked using the array (
    static Function[] functions = { Wave, MultiWave, Ripple, Sphere, FunkySphere};

    public enum FunctionName { Wave, MultiWave, Ripple, Sphere, FunkySphere }

    public static Function GetFunction(FunctionName name)
    {
        return functions[(int)name];
    }


    public static Vector3 Wave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + v + t));
        p.z = v;
        return p;
    }
    
    public static Vector3 MultiWave(float u, float v, float t)
    {
        Vector3 p;
        p.x = u;
        p.y =  Sin(PI * (u + 0.5f * t));
        p.y += 0.5f * Sin(2f * PI * (v + t));
        p.y += Sin(PI * (u + v + 0.25f *t));
        p.z = v;
        return p;
    }

    public static Vector3 Ripple(float u, float v, float t)
    {
        float d = Sqrt(u * u + v * v);
        Vector3 p;
        p.x = u;
        p.y = Sin(4 * (PI * d - t));
        p.y /= (1 + 10 * d);
        p.z = v;
        return p;
    }

    public static Vector3 Sphere(float u, float v, float t)
    {
        float r = 1f + Sin(PI * t);
        float s = r * Cos(PI * 0.5f * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin((PI * 0.5f) * v);
        p.z = s * Cos(PI * u);
        return p;
    }

    public static Vector3 FunkySphere(float u, float v, float t)
    {
        float r = 0.9f + 0.1f * Sin(PI * (4f * u + 8f * v + t));
        float s = r * Cos(PI * 0.5f * v);
        Vector3 p;
        p.x = s * Sin(PI * u);
        p.y = r * Sin((PI * 0.5f) * v);
        p.z = s * Cos(PI * u);
        return p;
    }
}
