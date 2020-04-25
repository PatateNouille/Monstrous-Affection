using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildZone : MonoBehaviour
{
    [SerializeField]
    MeshRenderer rend = null;

    [SerializeField]
    new Transform collider = null;

    [SerializeField]
    Color colCanBuild = Color.green;

    [SerializeField]
    Color colCantBuild = Color.red;

    int overlapping = 0;

    public bool Free => overlapping == 0;

    public Vector3 Center => collider.position;

    public void SetSize(Vector3 size)
    {
        collider.localScale = size;
        collider.localPosition = new Vector3(0f, size.y, size.z) * .5f;
    }

    private void Awake()
    {
        rend.material.color = colCanBuild;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (overlapping == 0)
        {
            rend.material.color = colCantBuild;
        }

        overlapping++;
    }

    private void OnTriggerExit(Collider other)
    {
        overlapping--;

        if (overlapping == 0)
        {
            rend.material.color = colCanBuild;
        }
    }
}
