using System;
using UnityEngine;

public class Clock : MonoBehaviour
{
    [SerializeField]
    Transform hoursHolder, minutesHolder, secondsHolder;

    const float hoursToDegrees = -30, minutesToDegrees = -6, secondsToDegrees = -6;

    private void Update()
    {
        TimeSpan currentTime = DateTime.Now.TimeOfDay;
        hoursHolder.localRotation = Quaternion.Euler(0, 0, hoursToDegrees * (float)currentTime.TotalHours);

        minutesHolder.localRotation = Quaternion.Euler(0, 0, minutesToDegrees * (float)currentTime.TotalMinutes);

        secondsHolder.localRotation = Quaternion.Euler(0, 0, secondsToDegrees * (float)currentTime.TotalSeconds);
    }
}