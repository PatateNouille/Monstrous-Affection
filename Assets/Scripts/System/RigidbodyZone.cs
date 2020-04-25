using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyZone : MonoBehaviour
{
    [SerializeField]
    public List<Rigidbody> rbs = new List<Rigidbody>();

    private void Update()
    {
        for (int i = 0; i < rbs.Count; i++)
        {
            if (rbs[i] != null) continue;

            rbs.RemoveAt(i--);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;

        if (rb == null || rbs.Contains(rb)) return;

        rbs.Add(rb);
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;

        if (rb == null || !rbs.Contains(rb)) return;

        rbs.Remove(rb);
    }
}
