using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

public static class FunctionLibrary3D
{
    public delegate Vector3 Function(float u, float v, float t, float speed);

    public enum FunctionName { Wave, MultiWave, Ripple, Sphere, Torus, SpiralThing }

    static Function[] functions = { Wave, MultiWave, Ripple, Sphere, Torus, SpiralThing};

    public static int GetFunctionCount()
    {
        return functions.Length;
    }

    public static int FunctionCount => functions.Length; 

    //get function by name
    public static Function GetFunction(FunctionName name) => functions[(int)name];

    //get next function in the sequence
    public static FunctionName GetNextFunctionName(FunctionName name) => (int)name < functions.Length - 1 ? name + 1 : 0;

    public static FunctionName GetRandomFunctionNameOtherThan(FunctionName disallowedName)
    {
        //This is cool, the choice is a random selection between 1 and the function length (so 0 is never chosen)
        FunctionName choice = (FunctionName)Random.Range(1, functions.Length);
        //then here, we check if the choice is the disallowed function name (current function), and if it is we substitute the selection to 0
        return choice == disallowedName ? 0 : choice;
        //this introduces no selection bias and is very tasty
    }

    public static Vector3 Morph(float u, float v, float t, float speed, Function from, Function To, float progress)
    {
        //LerpUnclamped (because the smoothstep is already clamped, no need for extra clamping)
        //between two functions to interpolate the position of each point between the from func and the two func based on the progress of the transition
        //calculations are being done for BOTH functions, PLUS a little interpolation overhead during transitions, making them AT LEAST twice as expensive.
        return Vector3.LerpUnclamped(from(u, v, t, speed), To(u, v, t, speed), Mathf.SmoothStep(0, 1, progress));
    }

    public static Vector3 Wave(float u, float v, float t, float speed)
    {
        Vector3 p;
        p.x = u;
        //set y position, scaling pos.x by PI will show the full function, and scaling time by PI will make it repeat every 2 seconds
        p.y = Sin(PI * (u - v + t * (2 / speed))); //t * (2 / speed) will make it take 'speed' seconds to repeat the function
        p.z = v;

        return p; 
    }

    public static Vector3 MultiWave(float u, float v, float t, float speed)
    {
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (u + t * (2 / speed))); 
        p.y += 0.5f * Sin(2f * PI * (v + t * 2 * (2 / speed))); //add second function at double speed but half size
        p.y += Sin(PI * (u - v + 0.25f * t * (2 / speed)));
        p.y *= (1f / 2.5f); //prefer multiplication over division for non-constants. (2f / 3f) will be reduced to a single number by the compiler, but it doesn't know what y is and is more happy doing multiplication on something new than division :)
        p.z = v;
        return p;
    }

    public static Vector3 Ripple(float u, float v, float t, float speed)
    {
        float d = Sqrt(u * u + v * v);
        Vector3 p;
        p.x = u;
        p.y = Sin(PI * (4f * d - t * (2 / speed)));
        p.y /= (1f + 10f * d);
        p.z = v;
        return p;
    }

    public static Vector3 Sphere(float u, float v, float t, float speed)
    {
        Vector3 p;
        float r = 0.9f + 0.1f * Sin(PI * (6f * u + 4f * v + t * (2f / speed)));
        float s = r * Cos(0.5f * PI * v);
        p.x = s *  Sin(PI * u);
        p.y = r * Sin(PI * 0.5f * v);
        p.z = s * Cos(PI * u);
        return p;
    }

    public static Vector3 Torus(float u, float v, float t, float speed)
    {
        Vector3 p;
        float r1 = (7f + Sin(PI * (6f * u + (t / 2 / speed)))) / 10f;
        float r2 = (3 + Sin(PI * (8f * u + 4f * v + 2f * t / speed))) / 20;
        float s = r1 + r2 * Cos(PI * v);
        p.x = s * Sin(PI * u);
        p.y = r2 * Sin(PI * v);
        p.z = s * Cos(PI * u);
        return p * 1.5f;
    }

    public static Vector3 SpiralThing(float u, float v, float t, float speed)
    {
        Vector3 p;
        p.x = ((4 + Sin(2f * PI * v) * Sin(2f * PI * u)) * Sin(3f * PI * v * (t / 2 * (1f / speed)))) * 1f / 4f;
        p.y = (Sin(2f * PI * v * t) * Cos(2f * PI * u) + 8f * v - 4f) * 1f / 12f + 0.5f;
        p.z = ((4f + Sin(2f * PI * u) * Sin(2 * PI * u)) * Cos(3f * PI * v * (t / 2 * (1f / speed)))) * 1f / 4f;
        return p;
    }
}

