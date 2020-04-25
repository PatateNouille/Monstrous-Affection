using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : Interactable, IPowered
{
    [SerializeField]
    public float fuelCapacity = 1f;

    [SerializeField]
    ParticleSystem thrustParticles = null;

    [SerializeField]
    float thrustSpeed = 1f;

    [SerializeField]
    Transform playerSeat = null;

    [SerializeField]
    float landDist = 1f;

    [SerializeField]
    float minimumFlyDist = 1f;

    [SerializeField]
    float lootMaxDist = 1f;

    float fuelStored = 0f;

    public float Power => fuelStored / fuelCapacity;

    public bool HighEnough => transform.position.magnitude >= minimumFlyDist;

    bool flying = false;
    bool landing = false;

    public override bool CanBeInteractedWith()
    {
        return base.CanBeInteractedWith() && Mathf.Approximately(Power, 1f) && Player.Instance.GrabbedType == null;
    }

    public override bool Interact()
    {
        if (!CanBeInteractedWith()) return false;

        TakeOff();

        Player.Instance.Seat(playerSeat);
        Player.Instance.SetSeatLocked(true);

        Game.Instance.Launch();

        return true;
    }

    public bool AddFuel(float fuel)
    {
        fuelStored += fuel;

        bool full = fuelStored >= fuelCapacity;

        if (full)
        {
            fuelStored = fuelCapacity;
        }

        UI.Instance.rocketFuel.SetProgress(Power);

        return full;
    }

    void TakeOff()
    {
        thrustParticles.Play(true);

        flying = true;
    }

    public void Land()
    {
        if (landing) return;

        landing = true;

        Vector3 dir = Game.Instance.CurPlanet.transform.position - transform.position;

        transform.position = dir.normalized * landDist;
    }

    void Crash()
    {
        if (!flying || !landing) return;

        flying = false;
        landing = false;

        Player.Instance.Seat(null);

        foreach (var itemName in Game.Instance.startingStuff)
        {
            Vector3 offset = Random.insideUnitSphere * lootMaxDist;
            offset.y = Mathf.Abs(offset.y);

            IItem item = ItemManager.Instance.SpawnItem(itemName);
            item.Interactable.transform.position = transform.TransformPoint(offset);
        }

        Destroy();
    }

    private void Update()
    {
        if (flying)
        {
            transform.position += transform.up * thrustSpeed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (flying && landing)
        {
            Crash();
        }
    }
}
