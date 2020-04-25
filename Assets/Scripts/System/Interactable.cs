using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utility;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField]
    public Vector3 highlighOffset = Vector3.zero;
    [SerializeField]
    public Vector3 highlightSize = Vector3.one;

    [SerializeField]
    public bool interactable = true;

    public delegate void OnDestroy(Interactable destroyed);
    public OnDestroy onDestroy = null;

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        DrawWireCube(transform.TransformPoint(highlighOffset), highlightSize, transform.rotation);
    }



    public virtual void SetInteractable(bool _interactable)
    {
        interactable = _interactable;

        if (!interactable) SetHighlighted(false);
    }

    public virtual bool CanBeInteractedWith()
    {
        return interactable;
    }

    public abstract bool Interact();

    public virtual void EndInteraction() { }


    public virtual void Destroy()
    {
        InteractionZone.Instance.targets.Remove(this);

        SetHighlighted(false);

        Destroy(gameObject);

        onDestroy?.Invoke(this);
    }


    public virtual void SetHighlighted(bool highlighted)
    {
        if (!CanBeInteractedWith()) highlighted = false;

        if (UI.Instance.interactables.Contains(this) == highlighted) return;

        if (highlighted)
            UI.Instance.interactables.Add(this);
        else
            UI.Instance.interactables.Remove(this);
    }
}
