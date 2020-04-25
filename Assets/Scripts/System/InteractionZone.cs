using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utility;

public class InteractionZone : UniqueInstance<InteractionZone>
{
    [SerializeField]
    public List<Interactable> targets = new List<Interactable>();

    private void OnTriggerEnter(Collider other)
    {
        Interactable target = other.GetComponentInParent<Interactable>();

        if (target == null || targets.Contains(target)) return;

        targets.Add(target);
    }

    private void OnTriggerExit(Collider other)
    {
        Interactable target = other.GetComponentInParent<Interactable>();

        if (target == null) return;

        int index = targets.FindIndex(item => item == target);

        if (index == -1) return;

        target.SetHighlighted(false);
        targets.RemoveAt(index);
    }

    public Interactable GetClosest()
    {
        if (targets.Count == 0) return null;

        Vector3 pos = transform.position;

        targets.Sort((x,y) => (int)Mathf.Sign(Vector3.Distance(pos, x.transform.position) - Vector3.Distance(pos, y.transform.position)));

        return targets.FirstOrDefault(t => t.CanBeInteractedWith());
    }
}
