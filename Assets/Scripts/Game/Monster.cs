using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utility;

public class Monster : UniqueInstance<Monster>
{
    [SerializeField]
    ItemIntake mouthIntake = null;

    [SerializeField]
    Timer hungerCapacity = null;

    public float Hunger => 1f - hungerCapacity.Progress;

    [SerializeField]
    Timer eatDelay = null;

    [SerializeField]
    Timer eatCooldown = null;

    [SerializeField]
    float heavyBreathingForce = 1f;

    [SerializeField]
    float minHunger = 0f;

    [SerializeField]
    bool heavyBreathing = false;

    Animator anim = null;

    RigidbodyZone heavyBreathingRange = null;

    bool dead = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        heavyBreathingRange = GetComponentInChildren<RigidbodyZone>();

        hungerCapacity.Start();
    }

    void Update()
    {
        if (dead) return;

        // Food

        if (hungerCapacity.Remaining > minHunger && hungerCapacity.Timeout())
        {
            Die();
        }

        eatCooldown.Timeout();

        UI.Instance.monsterHunger.SetProgress(Hunger);
        UI.Instance.monsterEatCD.SetProgress(eatCooldown.Cooldown);

        if (!eatCooldown.IsStarted && mouthIntake.inventory.ItemCount > 0)
        {
            if (!eatDelay.IsStarted) eatDelay.Start();
            else if (eatDelay.Timeout())
            {
                try
                {
                    KeyValuePair<string, uint> foodItem;

                    float missingFood = hungerCapacity.Elapsed;

                    foodItem = mouthIntake.inventory.Items.First(i => ((ItemManager.Instance.GetData(i.Key) as FoodData)?.hunger ?? float.PositiveInfinity) < missingFood);

                    mouthIntake.inventory.Remove(foodItem.Key, ConsumeFood(foodItem.Key, foodItem.Value));
                }
                catch (System.InvalidOperationException)
                {
                }
            }
        }

        anim.SetBool("Eating", eatCooldown.IsStarted);
    }

    void FixedUpdate()
    {
        if (heavyBreathing)
        {
            foreach (var rb in heavyBreathingRange.rbs)
            {
                rb.AddForce(transform.up * heavyBreathingForce, ForceMode.Acceleration);
            }
        }
    }

    uint ConsumeFood(string name, uint count)
    {
        float missingFood = hungerCapacity.Elapsed;

        FoodData data = ItemManager.Instance.GetData(name) as FoodData;

        count = (uint)Mathf.Min(missingFood / data.hunger, count);

        if (!hungerCapacity.IsStarted)
        {
            float capacity = hungerCapacity.Duration;
            hungerCapacity.Duration = 0f;
            hungerCapacity.Start();
            hungerCapacity.Duration = capacity;
        }

        hungerCapacity.Timeout(-count * data.hunger);

        eatCooldown.Start();

        return count;
    }

    public void StartHeavyBreath()
    {
        heavyBreathing = true;
    }

    public void EndHeavyBreath()
    {
        heavyBreathing = false;
    }

    public void Die()
    {
        if (dead) return;

        dead = true;
        anim.SetTrigger("Die");

        Game.Instance.Lose();
    }
}
