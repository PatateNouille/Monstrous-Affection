using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utility;

public class Deposit : Interactable
{
    [System.Serializable]
    class LootInfo
    {
        public string name = "";
        public float weight = 1f;
    }

    [SerializeField]
    uint hitPoints = 1;

    [SerializeField]
    Utility.RangeInt lootCountRange = null;

    [SerializeField]
    float lootMaxDist = 1f;

    [SerializeField]
    List<LootInfo> loots = null;

    bool dead = false;

    public delegate void OnHit();
    public OnHit onHit = null;

    public delegate void OnDie();
    public OnDie onDie = null;

    public override bool CanBeInteractedWith()
    {
        return base.CanBeInteractedWith();
    }

    public override bool Interact()
    {
        if (!CanBeInteractedWith()) return false;

        Hit();

        return true;
    }

    public bool Hit()
    {
        hitPoints--;

        dead = hitPoints == 0;

        onHit?.Invoke();

        if (dead) Die();

        return dead;
    }

    public void Die()
    {
        int lootCount = lootCountRange.GetRandom();

        while (lootCount-- > 0)
        {
            Loot();
        }

        onDie?.Invoke();

        Destroy();
    }

    public void Loot()
    {
        float totalWeight = loots.Sum(item => item.weight);

        float desiredWeight = totalWeight * Random.value;

        float currentWeight = 0f;

        foreach (var item in loots)
        {
            currentWeight += item.weight;

            if (desiredWeight > currentWeight) continue;

            IItem iitem = ItemManager.Instance.SpawnItem(item.name);

            Vector3 offset = Random.insideUnitSphere * lootMaxDist;

            iitem.Interactable.transform.position = transform.position + offset;

            break;
        }
    }
}
