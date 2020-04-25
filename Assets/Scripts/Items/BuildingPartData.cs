using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item Data/Building Part")]
public class BuildingPartData : ItemData
{
    public GameObject building = null;
    public Vector3 size = Vector3.one;
}
