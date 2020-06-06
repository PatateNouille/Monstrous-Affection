using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Rocket : Interactable, IPowered
{
    [SerializeField]
    public uint fuelCapacity = 1;

    [SerializeField]
    ParticleSystem thrustParticles = null;

    [SerializeField]
    float thrustSpeed = 1f;

    [SerializeField]
    Transform playerSeat = null;

    [SerializeField]
    CinemachineVirtualCamera flyingVcam = null;

    [SerializeField]
    float flyingCamOffset = 1f;

    [SerializeField]
    [Tooltip("Multplier of the offset when assigning it to the VCAM transposer Y offset\nIn function of the distance to the planet")]
    AnimationCurve flyingCamFactor = null;

    [SerializeField]
    float landDist = 1f;

    [SerializeField]
    float minimumFlyDist = 1f;

    [SerializeField]
    float lootMaxDist = 1f;

    public uint fuelStored = 0;

    public float Power => (float)fuelStored / fuelCapacity;

    public bool HighEnough => transform.position.magnitude >= minimumFlyDist;

    public EventPowerChanged OnPowerChanged { get; set; } = null;

    bool flying = false;
    bool landing = false;

    public override bool CanBeInteractedWith()
    {
        return base.CanBeInteractedWith() && fuelStored == fuelCapacity && Player.Instance.GrabbedType == null && !flying;
    }

    public override bool Interact()
    {
        if (!CanBeInteractedWith()) return false;

        TakeOff();

        Player.Instance.Sit(playerSeat);
        Player.Instance.SetSeatLocked(true);

        Game.Instance.Launch();

        return true;
    }

    public bool AddFuel(uint fuelCount)
    {
        fuelStored += fuelCount;

        bool full = fuelStored >= fuelCapacity;

        if (full)
        {
            fuelStored = fuelCapacity;
        }

        OnPowerChanged?.Invoke();

        UI.Instance.rocketFuel.SetProgress(Power);

        return full;
    }

    void TakeOff()
    {
        MainCamera.Instance.SetWorldUp(transform);
        flyingVcam.gameObject.SetActive(true);

        thrustParticles.Play(true);

        flying = true;
    }

    public void Land()
    {
        if (landing) return;

        landing = true;

        Vector3 dir = Game.Instance.CurPlanet.transform.position - transform.position;

        Vector3 localCamPos = transform.InverseTransformPoint(MainCamera.Instance.transform.position);
        flyingVcam.enabled = false;

        transform.position = dir.normalized * landDist;

        flyingVcam.enabled = true;
        MainCamera.Instance.transform.position = transform.TransformPoint(localCamPos);
    }

    void Crash()
    {
        if (!flying || !landing) return;

        flying = false;
        landing = false;

        MainCamera.Instance.SetWorldUp(Player.Instance.transform);
        flyingVcam.gameObject.SetActive(false);

        Player.Instance.Sit(null);

        foreach (var itemName in Game.Instance.startingStuff)
        {
            Vector3 offset = Random.insideUnitSphere * lootMaxDist;
            offset.y = Mathf.Abs(offset.y);

            IItem item = ItemManager.Instance.SpawnItem(itemName);
            item.transform.position = transform.TransformPoint(offset);
        }

        Destroy();
    }

    private void FixedUpdate()
    {
        if (flying)
        {
            transform.position += transform.up * thrustSpeed * Time.deltaTime;

            CinemachineTransposer transposer = flyingVcam.GetCinemachineComponent<CinemachineTransposer>();

            Vector3 toPlanet = Game.Instance.CurPlanet.transform.position - transform.position;

            float distFactor = flyingCamFactor.Evaluate(toPlanet.magnitude) * flyingCamOffset * (landing ? -1 : 1);

            transposer.m_FollowOffset.y = distFactor;
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
