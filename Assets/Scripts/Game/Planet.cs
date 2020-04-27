using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utility;

public class Planet : UniqueInstance<Planet>
{
    [SerializeField]
    [OnChangedCallback("SetSize")]
    public float radius = 1f;

    GameObject sphere = null;

    int resourceCount = 0;
    int resourceTotal = 0;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void SetSize()
    {
        if (sphere == null)
        {
            sphere = gameObject.transform.Find("Sphere").gameObject;
        }

        sphere.transform.localScale = Vector3.one * radius * 2f;
    }

    List<Vector3> GetPoints(float dist)
    {
        float totalLength = Mathf.PI * radius;

        float stepLength = Mathf.Sqrt(3f) * .5f * dist;

        int stepCount = (int)(totalLength / stepLength);

        float stepAngle = 180f / stepCount;

        float curAngle = 0f;

        List<Vector3> pts = new List<Vector3>();

        Vector3 up = Vector3.up * radius;

        while (curAngle < 180f)
        {
            Quaternion curRot = Quaternion.AngleAxis(curAngle, Vector3.right);

            float verticalTotalLength = Mathf.Sin(curAngle * Mathf.Deg2Rad) * totalLength;

            int verticalStepCount = (int)(verticalTotalLength / stepLength);

            float verticalStepAngle = 180f / verticalStepCount;

            float curVerticalAngle = 0f;

            while (curVerticalAngle < 360f)
            {
                Quaternion curVerticalRot = Quaternion.AngleAxis(curVerticalAngle, Vector3.up);

                pts.Add(curVerticalRot * curRot * up);

                curVerticalAngle += verticalStepAngle;
            }

            curAngle += stepAngle;
        }

        return pts;
    }

    Vector3 PopRandom(List<Vector3> pts)
    {
        int ptIndex = Random.Range(0, pts.Count);

        Vector3 pos = pts[ptIndex];
        pts.RemoveAt(ptIndex);

        return pos;
    }

    public void Populate(PlanetGenData data)
    {
        resourceTotal = 0;

        List<Vector3> pts = GetPoints(data.propDist);

        foreach (var deposit in data.deposits)
        {
            int depositLeft = data.perDepositRange.GetRandom();

            resourceTotal += depositLeft;

            while (depositLeft-- > 0)
            {
                Deposit d = Instantiate(deposit);
                d.transform.position = PopRandom(pts);

                d.onDie += RemoveResource;
            }
        }

        resourceCount = resourceTotal;

        Monster m = Instantiate(Game.Instance.monster);
        m.transform.position = PopRandom(pts);
    }

    void RemoveResource()
    {
        resourceCount--;

        UI.Instance.planetResources.SetProgress((float)resourceCount / resourceTotal);
    }
}
