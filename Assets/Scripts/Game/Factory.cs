using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utility;

public class Factory : Interactable, IPowered
{
    [System.Serializable]
    public class ItemInfo
    {
        public string name = "";
        public uint count = 1;

        public ItemInfo (string _name, uint _count = 0)
        {
            name = _name;
            count = _count;
        }

        public static List<ItemInfo> ListFromNames(params string[] names)
        {
            return names.Select(n => new ItemInfo(n)).ToList();
        }
    }

    [System.Serializable]
    public class Recipe
    {
        public string name = "";
        public float duration = 1f;

        public List<ItemInfo> input = new List<ItemInfo>();
        public List<ItemInfo> output = new List<ItemInfo>();
    }

    [SerializeField]
    ItemIntake craftIntake = null;

    [SerializeField]
    Transform craftInfoOffset = null;

    [SerializeField]
    ItemIntake fuelIntake = null;

    [SerializeField]
    Transform fuelInfoOffset = null;

    [SerializeField]
    Transform outputBegin = null;

    [SerializeField]
    Transform outputEnd = null;

    [SerializeField]
    Timer outputDelay = null;

    [SerializeField]
    Timer outputCooldown = null;

    [SerializeField]
    Timer powerCapacity = null;

    [SerializeField]
    public int recipeSelected = 0;

    [SerializeField]
    public List<Recipe> recipes = null;

    public float Power => uraniumPowered ? 1f : powerCapacity.Remaining / powerCapacity.Duration;
    public bool IsOutOfPower => Mathf.Approximately(Power, 0f);

    [SerializeField, Locked]
    bool uraniumPowered = false;

    [SerializeField, Locked]
    bool crafting = false;
    Timer craftDelay = new Timer();

    [SerializeField, Locked]
    bool dropping = false;
    Queue<string> dropQueue = new Queue<string>();
    Rigidbody currentDrop = null;

    InfoBubble craftBubble = null;
    SimpleInfoBubble fuelBubble = null;

    Animator anim = null;

    public delegate void OnCraftingRecipe(Recipe recipe, int recipeIndex);

    public OnCraftingRecipe onRecipeCrafted = null;

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = Color.cyan;

        if (outputBegin != null)
            DrawCross(outputBegin.position, .5f, outputBegin.rotation, true);

        if (outputEnd != null)
        {
            if (outputBegin != null)
                Gizmos.DrawLine(outputBegin.position, outputEnd.position);

            DrawCross(outputEnd.position, .5f, outputEnd.rotation, true);
        }
    }

    private void Start()
    {
        anim = GetComponent<Animator>();

        powerCapacity.Start();

        craftBubble = Instantiate(UI.Instance.infoBubble, craftInfoOffset.position, Quaternion.identity);
        fuelBubble = Instantiate(UI.Instance.simpleInfoBubble, fuelInfoOffset.position, Quaternion.identity);

        SetSelectedRecipe(0);

        SetBubbles(false);
    }

    void Update()
    {
        // Craft

        if (!IsOutOfPower)
        {
            if (crafting)
            {
                if (!uraniumPowered) powerCapacity.Timeout();

                if (craftDelay.Timeout())
                {
                    crafting = false;

                    onRecipeCrafted?.Invoke(recipes[recipeSelected], recipeSelected);

                    outputDelay.Start();
                }
            }
            else if (outputDelay.IsStarted)
            {
                if (outputDelay.Timeout())
                {
                    dropping = true;
                }
            }
            else if (dropping)
            {
                if (!outputCooldown.IsStarted)
                {
                    string name = dropQueue.Dequeue();

                    IItem item = ItemManager.Instance.SpawnItem(name);
                    item.SetPlanetInfluence(false);
                    item.Interactable.SetInteractable(false);

                    currentDrop = item.Interactable.GetComponent<Rigidbody>();
                    currentDrop.isKinematic = true;

                    outputCooldown.Start();

                    anim.SetTrigger("Drop");
                }
            }
            else
            {
                if (CheckRecipeInput())
                {
                    craftDelay.Duration = recipes[recipeSelected].duration;
                    craftDelay.Start();
                    crafting = true;

                    foreach (var item in recipes[recipeSelected].input)
                    {
                        craftIntake.inventory.Remove(item.name, item.count);
                    }

                    foreach (var item in recipes[recipeSelected].output)
                    {
                        for (int i = 0; i < item.count; i++)
                        {
                            dropQueue.Enqueue(item.name);
                        }
                    }
                }
            }
        }

        // Fuel

        if (fuelIntake.inventory.ItemCount > 0)
        {
            uint uraniumCount = fuelIntake.inventory.Count("Uranium");
            if (uraniumCount > 0)
            {
                uraniumPowered = true;

                fuelIntake.inventory.Remove("Uranium", uraniumCount);

                fuelIntake.filter.infos.Remove(new ItemFilter.Info("Uranium", true));

                SetBubbles(false);
            }

            try
            {
                KeyValuePair<string, uint> fuelItem;

                float missingPower = powerCapacity.Elapsed;

                fuelItem = fuelIntake.inventory.Items.First(i => ((ItemManager.Instance.GetData(i.Key) as FuelData)?.power ?? float.PositiveInfinity) < missingPower);

                fuelIntake.inventory.Remove(fuelItem.Key, ConsumeFuel(fuelItem.Key, fuelItem.Value));
            }
            catch (System.InvalidOperationException)
            {
            }
        }

        anim.SetBool("Crafting", crafting);
        anim.SetBool("Dropping", dropping);
        anim.SetFloat("Power", Power);
    }

    private void FixedUpdate()
    {
        if (dropping && outputCooldown.IsStarted)
        {
            bool ended = outputCooldown.Timeout(Time.fixedDeltaTime);
            float progress = outputCooldown.Progress;

            currentDrop.MovePosition(Vector3.Lerp(outputBegin.position, outputEnd.position, progress));
            currentDrop.MoveRotation(Quaternion.Slerp(outputBegin.rotation, outputEnd.rotation, progress));

            if (ended)
            {
                currentDrop.isKinematic = false;
                IItem item = currentDrop.GetComponent<IItem>();
                item.SetPlanetInfluence(true);
                item.Interactable.SetInteractable(true);

                if (dropQueue.Count == 0)
                {
                    currentDrop = null;
                    dropping = false;
                }
            }
        }
    }

    public override bool CanBeInteractedWith()
    {
        return base.CanBeInteractedWith() && !crafting && !dropping && !outputDelay.IsStarted;
    }

    public override bool Interact()
    {
        if (!CanBeInteractedWith()) return false;

        SetSelectedRecipe(recipeSelected + 1);

        return true;
    }

    public int SetSelectedRecipe(int selected)
    {
        if (recipes.Count == 0)
            recipeSelected = -1;
        else
            recipeSelected = selected % recipes.Count;

        SetBubbles(true);

        SetCraftIntakeFilter();

        return recipeSelected;
    }

    void SetCraftIntakeFilter()
    {
        ItemFilter filter = craftIntake.filter;

        filter.allowedByDefault = false;

        filter.infos.Clear();

        if (recipeSelected != -1) foreach (var item in recipes[recipeSelected].input)
        {
            filter.Add(item.name, true);
        }
    }

    void SetBubbles(bool craftOnly)
    {
        craftBubble.SetFromRecipe(recipes.Count > 0 && recipeSelected != -1 ? recipes[recipeSelected] : null);

        if (!craftOnly)
        {
            if (!uraniumPowered)
                fuelBubble.SetContent("Fuel Input", ItemInfo.ListFromNames("Wood", "Gaz", "Uranium"));
            else
                fuelBubble.SetContent("Fuel Input", ItemInfo.ListFromNames("Wood", "Gaz"));
        }
    }

    bool CheckRecipeInput()
    {
        if (recipes.Count == 0) return false;

        Recipe recipe = recipes[recipeSelected];

        bool canCraft = true;

        foreach (var item in recipe.input)
        {
            if (craftIntake.inventory.Count(item.name) >= item.count) continue;

            canCraft = false;
            break;
        }

        return canCraft;
    }

    uint ConsumeFuel(string name, uint count)
    {
        float missingPower = powerCapacity.Elapsed;

        FuelData data = ItemManager.Instance.GetData(name) as FuelData;

        count = (uint)Mathf.Min(missingPower / data.power, count);

        if (!powerCapacity.IsStarted)
        {
            float capacity = powerCapacity.Duration;
            powerCapacity.Duration = 0f;
            powerCapacity.Start();
            powerCapacity.Duration = capacity;
        }

        powerCapacity.Timeout(-count * data.power);

        return count;
    }
}
