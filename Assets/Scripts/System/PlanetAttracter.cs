using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlanetSurfaceAligner))]
public class PlanetAttracter : MonoBehaviour
{
    [SerializeField]
    float gravityForce = 1f;

    PlanetSurfaceAligner aligner = null;
    Rigidbody rb = null;

    void Start()
    {
        aligner = GetComponent<PlanetSurfaceAligner>();
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rb.AddForce(-aligner.groundUp * gravityForce, ForceMode.Force);
    }
}
