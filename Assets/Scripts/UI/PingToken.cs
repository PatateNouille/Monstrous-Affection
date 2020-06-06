using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using static Utility;

public class PingToken : MonoBehaviour
{
    [SerializeField]
    RawImage icon = null;

    public void SetIcon(Texture tex)
    {
        icon.texture = tex;
    }
}
