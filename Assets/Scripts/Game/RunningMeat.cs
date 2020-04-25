using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utility;

public class RunningMeat : MonoBehaviour
{
    [SerializeField]
    float jumpForce = 1f;

    [SerializeField]
    Timer jumpCooldown = null;

    [SerializeField]
    LayerMask jumpableLayers = new LayerMask();

    [SerializeField]
    float hitKnockback = 1f;

    PlanetSurfaceAligner aligner = null;
    Deposit deposit = null;

    Rigidbody rb = null;

    void Start()
    {
        aligner = GetComponent<PlanetSurfaceAligner>();

        deposit = GetComponent<Deposit>();

        deposit.onHit += OnHit;
        deposit.onDie += OnDie;

        rb = GetComponent<Rigidbody>();

        jumpCooldown.Start();
    }

    void Update()
    {
        if (jumpCooldown.Timeout())
        {
            Jump();

            jumpCooldown.Start();
        }
    }

    void OnHit()
    {
        Vector3 hitDir = transform.position - Player.Instance.transform.position;
        hitDir = Vector3.ProjectOnPlane(hitDir, aligner.groundUp);
        hitDir.Normalize();

        rb.AddForce(hitDir * hitKnockback, ForceMode.Impulse);

        Jump();
    }

    void OnDie()
    {

    }

    void Jump()
    {
        if (!Physics.Raycast(transform.position + transform.up * .1f, -transform.up, .2f, jumpableLayers.value)) return;

        Vector3 jumpDir = Random.insideUnitSphere;

        jumpDir.y = Mathf.Abs(jumpDir.y);

        rb.AddForce(transform.TransformDirection(jumpDir) * jumpForce, ForceMode.Impulse);
    }
}
