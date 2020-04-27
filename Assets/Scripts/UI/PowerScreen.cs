using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using static Utility;

public delegate void EventPowerChanged();

public interface IPowered
{
    float Power { get; }

    EventPowerChanged OnPowerChanged { get; set; }
}

public class PowerScreen : MonoBehaviour
{
    [SerializeField]
    [OnChangedCallback("GetTargetFromObject")]
    GameObject targetObject = null;

    public IPowered target = null;

    [SerializeField]
    Image fillImage = null;

    [SerializeField]
    float filledWidth = 1f;

    [SerializeField]
    Timer blinkDelay = null;

    RectTransform t = null;
    float height = 0f;

    bool energyShortage = false;
    bool energyFull = false;

    void GetTargetFromObject()
    {
        if (targetObject == null) return;

        target = targetObject.GetComponent<IPowered>();

        if (target == null)
        {
            Debug.LogError($"There are no IPowered derived script on '{targetObject.name}' !");
            targetObject = null;
        }
    }

    private void Start()
    {
        t = fillImage.GetComponent<RectTransform>();
        height = t.sizeDelta.y;

        if (target == null) GetTargetFromObject();
    }

    void Update()
    {
        float progress = target.Power;

        energyShortage = progress <= 0.1f;
        energyFull = progress >= 0.9f;

        if (energyShortage)
        {
            if (!blinkDelay.IsStarted)
            {
                blinkDelay.Start();
                fillImage.color = Color.red;
            }
            else blinkDelay.Timeout(true);

            progress = Mathf.Round(blinkDelay.Progress);
        }
        else if (blinkDelay.IsStarted)
        {
            blinkDelay.Stop();
            fillImage.color = Color.white;
        }
        else if (energyFull)
        {
            fillImage.color = Color.green;
        }
        else
        {
            fillImage.color = Color.white;
        }

        t.sizeDelta = new Vector2(filledWidth * progress, height);
    }
}
