using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using static Utility;

public class UI : UniqueInstance<UI>
{
    #region Variables

    [Header("Links")]
    [SerializeField]
    Canvas scaledCanvas = null;

    [SerializeField]
    Canvas fixedCanvas = null;

    [Header("Prefabs")]
    public UIItemInfo itemInfo = null;
    public InfoBubble infoBubble = null;
    public SimpleInfoBubble simpleInfoBubble = null;
    public BuildZone buildZone = null;
    public LightBeam lightBeam = null;
    [SerializeField] GameObject highlight = null;
    [SerializeField] PingToken pingToken = null;

    [Header("UI elements")]
    public Bar monsterHunger = null;
    public Bar monsterEatCD = null;
    public Bar rocketProgress = null;
    public Bar rocketFuel = null;
    public Bar jetpackCharge = null;
    public Bar planetResources = null;

    [Header("Parameters")]
    public float pingDefaultSpeed = 1f;
    [SerializeField] float pingTokenDuration = 1f;

    [HideInInspector]
    public List<Interactable> interactables = new List<Interactable>();

    [HideInInspector]
    public List<Pingable> pingables = new List<Pingable>();

    List<GameObject> highlights = new List<GameObject>();

    List<(GameObject target, PingToken go, Stopwatch timer)> pingTokens = new List<(GameObject target, PingToken go, Stopwatch timer)>();

    Camera cam = null;

    #endregion



    protected override void Awake()
    {
        base.Awake();

        cam = MainCamera.Instance.camera;

        StartCoroutine(LateAwake());
    }

    IEnumerator LateAwake()
    {
        yield return new WaitForEndOfFrame();

        monsterHunger.SetProgress(1f);
        monsterEatCD.SetProgress(0f);
        rocketProgress.SetProgress(0f);
        rocketFuel.SetProgress(0f);
        jetpackCharge.SetProgress(1f);
        planetResources.SetProgress(1f);
    }

    private void LateUpdate()
    {
        if (cam == null) cam = MainCamera.Instance.camera;

        UpdateHighlights();

        UpdatePingTokens();
    }

    void UpdateHighlights()
    {
        int highlightCount = highlights.Count;
        int interactableCount = interactables.Count;

        for (; highlightCount < interactableCount; highlightCount++)
        {
            GameObject go = Instantiate(highlight);

            go.transform.SetParent(fixedCanvas.transform, false);

            highlights.Add(go);
        }
        for (; highlightCount > interactableCount; highlightCount--)
        {
            GameObject go = highlights[0];

            Destroy(go);

            highlights.RemoveAt(0);
        }

        for (int i = 0; i < interactableCount; i++)
        {
            var curInter = interactables[i];

            Rect rect = GetScreenRectFromCube(cam, curInter.transform.TransformPoint(curInter.highlighOffset), curInter.highlightSize, curInter.transform.rotation);

            RectTransform t = highlights[i].GetComponent<RectTransform>();

            t.sizeDelta = rect.size;
            t.position = rect.center;
        }
    }

    void UpdatePingTokens()
    {
        for (int i = pingTokens.Count - 1; i >= 0; i--)
        {
            var token = pingTokens[i];

            if (token.target == null
             || token.timer.Elapsed.Seconds >= pingTokenDuration)
            {
                Destroy(token.go.gameObject);
                pingTokens.RemoveAt(i);
                continue;
            }

            token.go.transform.position = cam.WorldToScreenPoint(token.target.transform.position);
        }
    }

    public static void MakeItemInfos(GameObject go, List<Factory.ItemInfo> infos)
    {
        UI.MakeItemInfos(go, infos?.Select(info => (info.name, info.count, (uint?)null)).ToList());
    }

    public static void MakeItemInfos(GameObject go, List<(string name, uint desired, uint? current)> infos)
    {
        int infoCount = infos?.Count ?? 0;

        for (int i = go.transform.childCount; i < infoCount; i++)
        {
            UIItemInfo uiItem = Instantiate(UI.Instance.itemInfo);
            uiItem.transform.SetParent(go.transform, false);
        }
        for (int i = go.transform.childCount; i > infoCount; i--)
        {
            Destroy(go.transform.GetChild(i - 1).gameObject);
        }

        for (int i = 0; i < infoCount; i++)
        {
            UIItemInfo uiItem = go.transform.GetChild(i).GetComponent<UIItemInfo>();

            uiItem.SetIconFromData(ItemManager.Instance.GetData(infos[i].name));
            uiItem.SetCount(infos[i].desired, infos[i].current);
        }
    }

    public void PingAll(Vector3 from, float? speed = null)
    {
        StartCoroutine(PingAllRoutine(from, speed.GetValueOrDefault(pingDefaultSpeed)));
    }

    IEnumerator PingAllRoutine(Vector3 from, float speed)
    {
        List<(Pingable go, float dist)> pingQueue = pingables.Select(b => (go: b, dist: Vector3.Distance(from, b.transform.position))).OrderBy(pair => pair.dist).ToList();

        float lastDist = 0f;

        while (pingQueue.Count > 0)
        {
            var cur = pingQueue[0];
            pingQueue.RemoveAt(0);

            yield return new WaitForSeconds((cur.dist - lastDist) / speed);

            lastDist = cur.dist;
            cur.go.Ping();

            int idx = pingTokens.FindIndex(token => token.target == cur.go.gameObject);

            if (idx == -1)
            {
                PingToken tokenGO = Instantiate(pingToken, fixedCanvas.transform);
                tokenGO.SetIcon(cur.go.tokenIcon);

                Stopwatch timer = new Stopwatch();
                timer.Start();

                pingTokens.Add((target: cur.go.gameObject, go: tokenGO, timer: timer));
            }
            else
            {
                pingTokens[idx].timer.Restart();
            }
        }
    }
}
