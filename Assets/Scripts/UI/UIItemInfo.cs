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

    public void SetCount(uint _desired, uint? _current = null)
    {
        if (_desired == 0)
        {
            if (_current == null)
                count.text = "";
            else
                count.text = "x" + _current.Value;
        }
        else
        {
            if (_current == null)
                count.text = "x" + _desired;
            else
                count.text = $"{_current} / {_desired}";
        }
    }
}
