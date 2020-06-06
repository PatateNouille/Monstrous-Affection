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

    [SerializeField]
    AnimationCurve intensityOverDistance = null;

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

            float camDist = Vector3.Distance(transform.position, MainCamera.Instance.transform.position);

            float h, s, v;
            Color.RGBToHSV(beamColor, out h, out s, out v);
            v = Mathf.Pow(2f, intensityOverDistance.Evaluate(camDist));

            Color emission = Color.HSVToRGB(h, s, v, true);
            Color transparency = Color.white * opacityOverLifetime.Evaluate(progress);

            foreach (var rend in rends)
            {
                rend.material.SetColor("_EmissionColor", emission);
                rend.material.color = transparency;
            }
        }
    }

    public void Ping()
    {
        lifetime.Start();
    }
}
