using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

public static class FunctionLibrary
{
    public delegate float Function(float x, float t, float speed);

    public enum FunctionName { Wave, MultiWave, Ripple }

    static Function[] functions = { Wave, MultiWave, Ripple};

    public static Function GetFunction(FunctionName name)
    {
        return functions[(int)name];
    }

    public static float Wave(float x, float t, float speed)
    {
        //set y position, scaling pos.x by PI will show the full function, and scaling time by PI will make it repeat every 2 seconds
        return Sin(PI * (x + t * (2 / speed))); //t * (2 / speed) will make it take 'speed' seconds to repeat the function
    }

    public static float MultiWave(float x, float t, float speed)
    {
        float y = Sin(PI * (x + t * (2 / speed))); 
        y += 0.5f * Sin(2f * PI * (x + t * 2 * (2 / speed))); //add second function at double speed but half size
        return y * (2f / 3f); //prefer multiplication over division for non-constants. (2f / 3f) will be reduced to a single number by the compiler, but it doesn't know what y is and is more happy doing multiplication on something new than division :)
    }

    public static float Ripple(float x, float t, float speed)
    {
        float d = Abs(x);
        float y = Sin(PI * (4f * d - t * (2 / speed)));
        return y / (1f + 10f * d);
    }
}
