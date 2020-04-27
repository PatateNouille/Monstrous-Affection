using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using static Utility;

public class UI : UniqueInstance<UI>
{
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
    [SerializeReference]
    GameObject highlight = null;

    [Header("UI elements")]
    public Bar monsterHunger = null;
    public Bar monsterEatCD = null;
    public Bar rocketProgress = null;
    public Bar rocketFuel = null;
    public Bar jetpackCharge = null;
    public Bar planetResources = null;

    [HideInInspector]
    public List<Interactable> interactables = new List<Interactable>();

    List<GameObject> highlights = new List<GameObject>();

    Camera cam = null;

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
}
