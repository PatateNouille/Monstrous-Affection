using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utility;

[CreateAssetMenu(menuName = "Planet Generation Data")]
public class PlanetGenData : ScriptableObject
{
    public List<Deposit> deposits = null;
    public Utility.RangeInt perDepositRange = null;

    public float propDist = 1f;
}
