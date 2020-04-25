using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemIntake : MonoBehaviour
{
    [SerializeField]
    public Inventory inventory = new Inventory();

    [SerializeField]
    public ItemFilter filter = new ItemFilter();

    private void OnTriggerStay(Collider other)
    {
        IItem item = other.GetComponentInParent<IItem>();

        if (item == null || !item.Interactable.CanBeInteractedWith() || !filter.IsItemAllowed(item) || inventory.IsFull) return;

        item.Interactable.Destroy();

        inventory.Add(item.Data.itemName, 1);
    }
}
