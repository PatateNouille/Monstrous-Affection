using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField]
    public bool inverted = false;

    [SerializeField]
    public bool keepVertical = false;

    PlanetSurfaceAligner aligner = null;
    Camera cam = null;

    void Start()
    {
        aligner = GetComponent<PlanetSurfaceAligner>();
    }

    void LateUpdate()
    {
        if (cam == null) cam = MainCamera.Instance.camera;

        Vector3 dir = cam.transform.position - transform.position;

        if (inverted) dir = -dir;

        Vector3 up;

        if (aligner != null)
        {
            up = aligner.groundUp;
        }
        else
        {
            up = cam.transform.up;
        }

        if (keepVertical)
        {
            dir = Vector3.ProjectOnPlane(dir, up);
        }

        transform.rotation = Quaternion.LookRotation(dir, up);
    }
}
