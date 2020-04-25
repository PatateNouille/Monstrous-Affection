using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bar : MonoBehaviour
{
    [SerializeField]
    RectTransform fill = null;

    RectTransform rt = null;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public void SetProgress(float progress)
    {
        fill.sizeDelta = new Vector2(rt.sizeDelta.x * Mathf.Clamp01(progress), fill.sizeDelta.y);
    }
}
