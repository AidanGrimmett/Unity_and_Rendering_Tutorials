using UnityEngine;
public struct SliderData
{
    public float MinValue; //slider minimum value
    public float MaxValue; //slider maximum value
    public float CurrentValue; // current slider value

    public SliderData(float minVal, float maxVal, float currentVal)
    {
        MinValue = minVal;
        MaxValue = maxVal;
        CurrentValue = Mathf.Clamp(currentVal, minVal, maxVal);
    }

    public void SetValue(float value)
    {
        CurrentValue = Mathf.Clamp(value, MinValue, MaxValue);
    }
}

