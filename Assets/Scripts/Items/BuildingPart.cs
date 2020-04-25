using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPart : Item<BuildingPartData>
{
    BuildZone zone = null;

    protected virtual void Update()
    {
        if (zone != null)
        {
            zone.transform.position = Player.Instance.transform.TransformPoint(Vector3.forward * 2f);
            zone.transform.rotation = Player.Instance.transform.rotation;
        }
    }

    public override bool Interact()
    {
        if (!base.Interact()) return false;

        SetInteractable(true);

        if (zone == null)
        {
            zone = Instantiate(UI.Instance.buildZone);

            zone.SetSize(data.size);
        }
        else
        {
            if (zone.Free)
            {
                Destroy(zone.gameObject);
                Instantiate(data.building, zone.Center, Quaternion.LookRotation(Player.Instance.transform.position - zone.Center, aligner.groundUp));
                Destroy();
            }
        }

        return true;
    }

    public override void EndInteraction()
    {
        Destroy(zone.gameObject);

        base.EndInteraction();
    }
}
