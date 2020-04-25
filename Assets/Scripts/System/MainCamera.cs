using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utility;

public class MainCamera : UniqueInstance<MainCamera>
{
    [SerializeField]
    [OnChangedCallback("UpdateSettings")]
    bool fog = true;

    [SerializeField]
    [OnChangedCallback("UpdateSettings")]
    Color fogColor = Color.gray;

    [SerializeField]
    [OnChangedCallback("UpdateSettings")]
    float fogDensity = 1f;

    [SerializeField]
    [OnChangedCallback("UpdateSettings")]
    float fogStart = 1f;

    [SerializeField]
    [OnChangedCallback("UpdateSettings")]
    float fogEnd = 2f;

    [SerializeField]
    [OnChangedCallback("UpdateSettings")]
    FogMode fogMode = FogMode.Exponential;

    [HideInInspector]
    public new Camera camera = null;

    void UpdateSettings()
    {
        RenderSettings.fog = fog;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogStartDistance = fogStart;
        RenderSettings.fogEndDistance = fogEnd;
        RenderSettings.fogMode = fogMode;
    }

    public void Load()
    {
        camera = GetComponent<Camera>();

        UpdateSettings();
    }

    private void Awake()
    {
        Load();
    }
}
