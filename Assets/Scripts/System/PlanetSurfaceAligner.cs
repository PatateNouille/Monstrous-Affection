using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Experimental.SceneManagement;
#endif

using static Utility;

[ExecuteInEditMode]
public class PlanetSurfaceAligner : MonoBehaviour
{
    [SerializeField]
    bool alignOnStart = true;

    [SerializeField]
    bool alignOnUpdate = true;

    [SerializeField]
    bool snapOnStart = true;

    [SerializeField]
    bool snapOnUpdate = false;

    [HideInInspector]
    public Vector3 groundUp = Vector3.up;

    [SerializeField, Locked]
    public float groundDist = 0f;

    void Start()
    {
        GetGroundInfo();

        if (alignOnStart)
        {
            Align();
        }

        if (snapOnStart)
        {
            Snap();
        }
    }

    void Update()
    {
        GetGroundInfo();

        bool inPrefab = false;
#if UNITY_EDITOR
        inPrefab = PrefabStageUtility.GetCurrentPrefabStage() != null;
#endif

        if (alignOnUpdate || (!Application.isPlaying && !inPrefab && (alignOnStart || alignOnUpdate)))
        {
            Align();
        }
        
        if (snapOnUpdate || (!Application.isPlaying && !inPrefab && (snapOnStart || snapOnUpdate)))
        {
            Snap();
        }
    }

    public void GetGroundInfo()
    {
        if (Game.Instance == null) return;

        float len = transform.position.magnitude;

        groundDist = len - Game.Instance.CurPlanet.radius;
        groundUp = len != 0f ? transform.position / len : Vector3.up;
    }

    public void Align()
    {
        Vector3 projectedForward = Vector3.ProjectOnPlane(transform.forward, groundUp);

        transform.rotation = Quaternion.LookRotation(projectedForward.sqrMagnitude < .005f ? Vector3.forward : projectedForward, groundUp);
    }

    public void Snap()
    {
        if (Game.Instance == null) return;

        transform.position = groundUp * Game.Instance.CurPlanet.radius;
    }
}
