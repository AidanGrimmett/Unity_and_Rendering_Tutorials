using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class SettingsController : MonoBehaviour
{
    [SerializeField]
    GameObject SettingsPanel, FPSPanel;
    [SerializeField]
    GPUGraph gpuGraph;

    [SerializeField]
    Slider resolutionSlider, speedSlider, durationSlider, transitionDurationSlider;

    [SerializeField]
    TextMeshProUGUI resText, speedText, durationText, transitionDurationText;

    [SerializeField]
    Toggle transitionToggle, showFPSToggle;

    [SerializeField]
    TMP_Dropdown functionsDropdown;

    private bool showSettings;

    private void OnEnable()
    {
        List<string> dropOptions = new List<string>();
        foreach (var name in Enum.GetValues(typeof(FunctionLibrary3D.FunctionName)))
        {
            dropOptions.Add(name.ToString());
        }
        functionsDropdown.AddOptions(dropOptions);
        SetSettingsPanelVisibility(true);
    }

    private void SetSettingsPanelVisibility(bool active)
    {
        showSettings = active;
        SettingsPanel.SetActive(active);
        if (active)
        {
            UpdateAllData();
        }
    }

    public void UpdateAllData()
    {
        UpdateRes();
        UpdateSpeed();
        UpdateDuration();
        UpdateTransitionDuration();
        UpdateTransitionEnabled();
    }

    protected void UpdateRes()
    {
        resText.SetText("Resolution: {0:0}", resolutionSlider.value);
        gpuGraph.SetResolution((int)resolutionSlider.value);
    }

    public void UpdateSpeed()
    {
        speedText.SetText("Speed: {0:1}", speedSlider.value);
        gpuGraph.SetSpeed(speedSlider.value);
    }

    public void UpdateDuration()
    {
        durationText.SetText("Function Duration: {0:1}", durationSlider.value);
        gpuGraph.SetDuration(durationSlider.value);
    }

    public void UpdateTransitionDuration()
    {
        transitionDurationText.SetText("Transition Duration: {0:1}", transitionDurationSlider.value);
        gpuGraph.SetTransitionDuration(transitionDurationSlider.value);
    }

    public void UpdateTransitionEnabled()
    {
        gpuGraph.enableTransition = transitionToggle.isOn;
    }    

    public void UpdateShowFPS()
    {
        FPSPanel.SetActive(showFPSToggle.isOn);
    }

    public void UpdateFunctionDisplay(int index)
    {
        functionsDropdown.value = index;
    }

    public void UpdateFunctionSelection()
    {
        gpuGraph.SetFunction((FunctionLibrary3D.FunctionName)functionsDropdown.value);
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            if (!showSettings)
            {
                SetSettingsPanelVisibility(true);
            }
            else if (Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetMouseButtonDown(0) && 
                (Input.mousePosition.x > 260f || Screen.height - Input.mousePosition.y > 350f))
            {
                SetSettingsPanelVisibility(false);
            }
        }
    }
}
