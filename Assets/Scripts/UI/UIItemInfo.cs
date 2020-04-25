using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIItemInfo : MonoBehaviour
{
    [SerializeField]
    Image icon = null;

    [SerializeField]
    TextMeshProUGUI count = null;

    public void SetIconFromData(ItemData data)
    {
        icon.sprite = data.icon;
    }

    public void SetCount(uint _count)
    {
        if (_count == 0)
            count.text = "";
        else
            count.text = "x" + _count;
    }
}
