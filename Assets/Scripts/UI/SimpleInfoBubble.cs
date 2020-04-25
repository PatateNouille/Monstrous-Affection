using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SimpleInfoBubble : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI title = null;

    [SerializeField]
    GameObject items = null;

    public void SetContent(string _title, List<Factory.ItemInfo> infos)
    {
        title.text = _title;

        UI.MakeItemInfos(items, infos);
    }
}
