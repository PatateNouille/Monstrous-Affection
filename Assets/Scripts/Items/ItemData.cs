using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData : ScriptableObject
{
    public string itemName = "Unknown Item";
    public GameObject customData = null;
    public Vector3 highlightOffset = Vector3.zero;
    public Vector3 highlightSize = Vector3.one;
    public Sprite icon = null;
}
