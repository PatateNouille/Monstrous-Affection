using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utility;

public class Player : UniqueInstance<Player>
{
    [SerializeField]
    float moveSpeed = 1f;

    [SerializeField]
    float dragGround = 1f;

    [SerializeField]
    float dragAir = 1f;

    [SerializeField]
    LayerMask groundLayer = new LayerMask();

    [SerializeField]
    float rotationSpeed = 1f;

    [SerializeField]
    float rotationDropFactor = 1f;

    [SerializeField]
    float rotationDropMaxSpeed = 1f;

    [SerializeField]
    Vector3 movementDropFactor = Vector3.one;

    [SerializeField]
    float movementDropMaxBackward = 1f;

    [SerializeField]
    Timer interactCooldown = null;

    [SerializeField]
    [Tooltip("Power in function of the distance to the planet ground")]
    AnimationCurve jetpackForce = null;

    [SerializeField]
    float jetpackCapacity = 1f;

    [SerializeField]
    Timer jetpackCD = null;

    [SerializeField]
    Timer hitCooldown = null;

    [SerializeField]
    Transform grabOffset = null;

    // Inputs
    Vector3 inputMove = new Vector3();
    float inputRotate = 0f;
    bool inputPressInteract = false;
    bool inputInteract = false;
    bool inputDeinteract = false;
    bool interactPrev = false;

    // Components
    PlanetAttracter attracter = null;
    PlanetSurfaceAligner aligner = null;
    Rigidbody rb = null;
    [HideInInspector]
    public InteractionZone interaction = null;

    // Player
    List<IItem> grabbed = new List<IItem>();
    public ItemData GrabbedType => grabbed.Count > 0 ? grabbed[0].Data : null;

    bool seating = false;
    bool seatLocked = false;

    float jetpackCharge = 0f;


    void Start()
    {
        attracter = GetComponent<PlanetAttracter>();
        aligner = GetComponent<PlanetSurfaceAligner>();
        rb = GetComponent<Rigidbody>();
        interaction = GetComponentInChildren<InteractionZone>();

        jetpackCharge = jetpackCapacity;
    }

    void Update()
    {
        GetInput();

        Actuate();

        HandleSeat();

        Interact();

        Grab();
    }

    void FixedUpdate()
    {
        FixedActuate();

        TestGround();
    }

    void GetInput()
    {
        inputMove.x = Input.GetAxisRaw("Horizontal");
        inputMove.z = Input.GetAxisRaw("Vertical");
        inputMove.y = Input.GetAxisRaw("Jump");

        if (inputMove.sqrMagnitude > 1f) inputMove.Normalize();

        inputRotate = Input.GetAxisRaw("Mouse X");

        float interactAxis = Input.GetAxisRaw("Interact");
        inputInteract = interactAxis > .5f && !interactCooldown.IsStarted;
        inputPressInteract = interactAxis > .5f && !interactPrev;
        interactPrev = interactAxis >.5f;
        inputDeinteract = interactAxis < -.5f && !interactCooldown.IsStarted;
    }

    void HandleSeat()
    {
        if (seating)
        {
            if (seatLocked) return;

            if (inputMove.sqrMagnitude > 0f) Seat(null);
        }
    }

    void Interact()
    {
        hitCooldown.Timeout();
        interactCooldown.Timeout();

        Interactable target = hitCooldown.IsStarted ? null : interaction.GetClosest();

        bool canInteract = false;

        if (target != null) canInteract = target.CanBeInteractedWith();

        for (int i = 0; i < interaction.targets.Count; i++)
        {
            Interactable t = interaction.targets[i];

            bool same = target == null ? false : t == target;

            t.SetHighlighted(same && canInteract);
        }

        IItem item = target as IItem;
        bool wasGrabbed = item?.IsGrabbed ?? false;

        Factory factory = target as Factory;

        bool wantInteract = inputInteract;
        if (wasGrabbed || factory != null) wantInteract = inputPressInteract;

        if (!wantInteract || !canInteract) return;

        if (target.Interact())
        {
            interactCooldown.Start();

            if (item != null && !wasGrabbed)
            {
                grabbed.Add(item);
                item.Interactable.onDestroy += OnGrabbedDestroyed;
            }

            Deposit deposit = target as Deposit;
            if (deposit != null)
            {
                hitCooldown.Start();
            }
        }
    }

    void Grab()
    {
        if (grabbed.Count > 0)
        {
            float itemHeight = grabbed[0].ItemHeight;

            for (int i = 0; i < grabbed.Count; i++)
            {
                grabbed[i].Interactable.transform.position = grabOffset.TransformPoint(new Vector3(0f, itemHeight * i, 0f));
                grabbed[i].Interactable.transform.rotation = grabOffset.rotation;
            }

            if (inputDeinteract)
            {
                interactCooldown.Start();

                float radialDropForce = GetStep(0f, inputRotate * rotationSpeed * rotationDropFactor, rotationDropMaxSpeed);

                Vector3 localMove = Vector3.Scale(transform.InverseTransformDirection(rb.velocity), movementDropFactor);

                localMove.z = Mathf.Max(localMove.z, -movementDropMaxBackward);

                grabbed[0].Interactable.EndInteraction();
                grabbed[0].Interactable.GetComponent<Rigidbody>().velocity =
                    transform.TransformDirection(localMove)
                    + radialDropForce * transform.right;
                grabbed[0].Interactable.onDestroy -= OnGrabbedDestroyed;
                grabbed.RemoveAt(0);
            }
        }
    }

    void OnGrabbedDestroyed(Interactable destroyed)
    {
        grabbed.Remove(destroyed as IItem);
    }

    void Actuate()
    {
        transform.rotation = Quaternion.AngleAxis(inputRotate * rotationSpeed, aligner.enabled ? aligner.groundUp : transform.up) * transform.rotation;
    }

    void FixedActuate()
    {
        if (seating) return;

        rb.AddForce(transform.TransformDirection(inputMove.Flat()) * moveSpeed, ForceMode.VelocityChange);

        if (inputMove.y > 0f)
        {
            if (jetpackCharge > 0f)
            {
                jetpackCharge += GetStep(jetpackCharge, 0f, Time.fixedDeltaTime);

                rb.AddForce(aligner.groundUp * jetpackForce.Evaluate(aligner.groundDist), ForceMode.Impulse);
            }
        }
        else
        {
            jetpackCharge += GetStep(jetpackCharge, jetpackCapacity, Time.fixedDeltaTime);
        }

        UI.Instance.jetpackCharge.SetProgress(jetpackCharge / jetpackCapacity);
    }

    void TestGround()
    {
        bool onGround = Physics.Raycast(transform.position + transform.up * .1f, -transform.up, .2f, groundLayer.value);

        rb.drag = onGround ? dragGround : dragAir;
    }

    public void Seat(Transform seat)
    {
        bool wantSeat = seat != null;

        if (seating == wantSeat) return;

        seating = wantSeat;

        if (wantSeat)
        {
            transform.parent = seat;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            attracter.enabled = false;
            aligner.enabled = false;

            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }
        else
        {
            transform.parent = null;

            attracter.enabled = true;
            aligner.enabled = true;

            rb.isKinematic = false;
        }
    }

    public void SetSeatLocked(bool locked)
    {
        if (!seating) return;

        seatLocked = locked;
    }
}
