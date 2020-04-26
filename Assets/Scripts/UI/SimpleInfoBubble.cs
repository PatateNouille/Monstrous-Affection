using System.Linq;
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

    public void SetTitle(string _title)
    {
        title.text = _title;
    }

    public void SetContent(string _title, List<Factory.ItemInfo> infos)
    {
        SetContent(_title, infos.Select(info => (info.name, info.count, (uint?)null)).ToList());
    }

    public void SetContent(string _title, List<(string name, uint desired, uint? current)> infos)
    {
        SetTitle(_title);

        UI.MakeItemInfos(items, infos);
    }

    public void ClearContent()
    {
        UI.MakeItemInfos(items, (List<Factory.ItemInfo>)null);
    }
}
