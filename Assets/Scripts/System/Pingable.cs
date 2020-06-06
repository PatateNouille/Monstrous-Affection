using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pingable : MonoBehaviour
{
    [ColorUsage(true, true)]
    public Color beamColor = Color.white;

    public Texture tokenIcon = null;

    public float discoverDistance = 1f;

    public bool discovered { get; private set; } = false;

    public System.Action onPing = null;

    IEnumerator TryDiscover()
    {
        while (!discovered)
        {
            yield return null;

            if (Vector3.Distance(Player.Instance.transform.position, transform.position) <= discoverDistance)
            {
                Discover();
            }
        }
    }

    public bool Discover()
    {
        if (discovered) return false;

        discovered = true;

        OnDiscovered();

        return true;
    }

    protected virtual void OnDiscovered()
    {
        LightBeam beam = Instantiate(UI.Instance.lightBeam, transform.position, Quaternion.identity);
        beam.beamColor = beamColor;

        onPing += beam.Ping;

        UI.Instance.pingables.Add(this);
    }

    public void Ping()
    {
        onPing?.Invoke();
    }

    private void Start()
    {
        StartCoroutine(TryDiscover());
    }
}
