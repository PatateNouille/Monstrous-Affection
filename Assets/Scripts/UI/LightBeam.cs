using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utility;

public class LightBeam : MonoBehaviour
{
    [SerializeField]
    Transform visualBeam = null;

    [SerializeField]
    Timer lifetime = null;

    [SerializeField]
    AnimationCurve scaleOverLifetime = null;

    [SerializeField]
    AnimationCurve opacityOverLifetime = null;

    [ColorUsage(true, true)]
    public Color beamColor = Color.white;

    Vector3 baseScale = Vector3.one;

    Renderer[] rends = null;

    private void Awake()
    {
        baseScale = visualBeam.localScale;

        rends = GetComponentsInChildren<Renderer>();
    }

    void Start()
    {
        Ping();
    }

    void Update()
    {
        if (lifetime.IsStarted)
        {
            lifetime.Timeout();

            float progress = lifetime.ProgressRaw;

            visualBeam.localScale = baseScale * scaleOverLifetime.Evaluate(progress);

            float opacity = opacityOverLifetime.Evaluate(progress);
            foreach (var rend in rends)
            {
                Color col = beamColor;
                rend.material.SetColor("_EmissionColor", col);
                col.a = opacity;
                rend.material.color = col;
            }
        }
    }

    public void Ping()
    {
        lifetime.Start();
    }
}
