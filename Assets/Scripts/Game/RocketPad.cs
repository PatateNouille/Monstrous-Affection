using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketPad : Interactable
{
    [SerializeField]
    ItemIntake partIntake = null;

    [SerializeField]
    Transform partBubbleOffset = null;

    [SerializeField]
    ItemIntake fuelIntake = null;

    [SerializeField]
    Transform fuelBubbleOffset = null;

    [SerializeField]
    Transform partsOffset = null;

    [SerializeField]
    Transform rocketOffset = null;

    [SerializeField]
    PowerScreen rocketPowerScreen = null;

    [SerializeField]
    UnityEngine.Material matScreenOn = null;

    [SerializeField]
    float rocketPartHeight = 2f;

    [SerializeField]
    List<RocketPart> parts = null;

    RocketPartData nextPart = null;

    SimpleInfoBubble partBubble = null;
    SimpleInfoBubble fuelBubble = null;

    Rocket rocket = null;

    public float Progress => (float)parts.Count / ItemManager.Instance.RocketPartCount;

    public override bool CanBeInteractedWith()
    {
        return base.CanBeInteractedWith() && parts.Count == ItemManager.Instance.RocketPartCount;
    }

    public override bool Interact()
    {
        if (!CanBeInteractedWith()) return false;

        CraftRocket();

        return true;
    }

    private void Start()
    {
        partIntake.inventory.maxItemCount = 1;
        partIntake.filter.allowedByDefault = false;

        partBubble = Instantiate(UI.Instance.simpleInfoBubble, partBubbleOffset.position, Quaternion.identity);
        fuelBubble = Instantiate(UI.Instance.simpleInfoBubble, fuelBubbleOffset.position, Quaternion.identity);

        fuelBubble.SetContent("Fuel Intake", new List<Factory.ItemInfo>() { new Factory.ItemInfo("Gaz", 5) });

        SetNextPart(0);
    }

    private void Update()
    {
        if (nextPart != null && partIntake.inventory.Count(nextPart.itemName) > 0)
        {
            partIntake.inventory.Remove(nextPart.itemName, 1);

            parts.Add(SpawnPart(nextPart));

            UI.Instance.rocketProgress.SetProgress(Progress);

            SetNextPart(parts.Count);
        }

        if (rocket != null && rocket.Power < 1f && fuelIntake.inventory.ItemCount > 0)
        {
            Dictionary<string, uint> itemsConsumed = new Dictionary<string, uint>();

            foreach (var item in fuelIntake.inventory.Items)
            {
                ItemData data = ItemManager.Instance.GetData(item.Key);

                for (int i = 0; i < item.Value; i++)
                {
                    FuelData fuel = data as FuelData;
                    if (fuel != null)
                    {
                        uint count;
                        itemsConsumed.TryGetValue(item.Key, out count);

                        itemsConsumed[item.Key] = count + 1;

                        if (rocket.AddFuel(fuel.power))
                        {
                            break;
                        }
                    }

                }
            }

            foreach (var item in itemsConsumed)
            {
                fuelIntake.inventory.Remove(item.Key, item.Value);
            }
        }
    }

    void SetNextPart(int index)
    {
        if (index >= 0 && index < ItemManager.Instance.RocketPartCount)
        {
            nextPart = ItemManager.Instance.GetRocketPartData(index);

            partIntake.filter.infos.Clear();
            partIntake.filter.infos.Add(new ItemFilter.Info(nextPart.itemName, true));

            partBubble.SetContent(nextPart.itemName, new List<Factory.ItemInfo>() { new Factory.ItemInfo(nextPart.itemName, 1) });
        }
        else
        {
            nextPart = null;

            partBubble.SetContent("Completed !", null);
        }
    }

    RocketPart SpawnPart(RocketPartData data)
    {
        RocketPart part = ItemManager.Instance.SpawnItem(data.itemName) as RocketPart;

        part.SetInteractable(false);
        part.SetPlanetInfluence(false);

        part.GetComponent<Rigidbody>().isKinematic = true;

        part.transform.parent = partsOffset;
        part.transform.localPosition = new Vector3(0f, rocketPartHeight * parts.Count, 0f);
        part.transform.localRotation = Quaternion.identity;

        return part;
    }

    public Rocket CraftRocket()
    {
        if (Game.Instance.isMenu)
        {
            PlayerPrefs.SetInt("Built Rocket", 1);
            PlayerPrefs.Save();
        }

        parts.ForEach(p => p.Destroy());
        parts.Clear();

        rocket = Instantiate(Game.Instance.rocket);

        SceneSwitcher.Instance.SetRocket(rocket);

        rocket.transform.position = rocketOffset.position;
        rocket.transform.rotation = rocketOffset.rotation;

        rocketPowerScreen.gameObject.SetActive(true);
        rocketPowerScreen.target = rocket;

        fuelIntake.filter.infos.Clear();
        fuelIntake.filter.allowedByDefault = false;

        partIntake.filter.infos.Clear();
        partIntake.filter.allowedByDefault = false;

        MeshRenderer renderer = rocketPowerScreen.transform.parent.GetComponent<MeshRenderer>();

        UnityEngine.Material[] mats = renderer.materials;

        renderer.materials = mats.Select(m => m.name.Contains("Screen") ? matScreenOn : m).ToArray();

        return rocket;
    }
}
