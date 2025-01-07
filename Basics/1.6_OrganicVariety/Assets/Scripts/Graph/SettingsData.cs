using UnityEngine;
public class SettingsData : MonoBehaviour
{
    public static SettingsData Instance { get; private set; }

    public SliderData resolutionData = new SliderData(10, 1000, 100);
    public SliderData speedData = new SliderData(0.1f, 10f, 2f);
    public SliderData funcDurationData = new SliderData(0f, 10f, 2f);
    public SliderData transitionDurationData = new SliderData(0f, 10f, 3f);

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
}
