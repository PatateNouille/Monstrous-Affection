using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

using static Utility;

public class MainCamera : UniqueInstance<MainCamera>
{
    [SerializeField]
    [OnChangedCallback("UpdateFogSettings")]
    bool fog = true;

    [SerializeField]
    [OnChangedCallback("UpdateFogSettings")]
    Color fogColor = Color.gray;

    [SerializeField]
    [OnChangedCallback("UpdateFogSettings")]
    float fogDensity = 1f;

    [SerializeField]
    [OnChangedCallback("UpdateFogSettings")]
    float fogStart = 1f;

    [SerializeField]
    [OnChangedCallback("UpdateFogSettings")]
    float fogEnd = 2f;

    [SerializeField]
    [OnChangedCallback("UpdateFogSettings")]
    FogMode fogMode = FogMode.Exponential;

    [HideInInspector]
    public new Camera camera = null;

    [HideInInspector]
    public CinemachineBrain brain = null;

    public void UpdateFogSettings()
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
        brain = GetComponent<CinemachineBrain>();

        UpdateFogSettings();
    }

    protected override void Awake()
    {
        base.Awake();

        Load();

        SetWorldUp(Player.Instance.transform);
    }

    public void Warp(Vector3 pos)
    {
        brain.enabled = false;

        transform.position = pos;

        brain.enabled = true;
    }

    public void SetWorldUp(Transform worldUpOverride)
    {
        brain.m_WorldUpOverride = worldUpOverride;
    }
}
