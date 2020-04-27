using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using static Utility;

public class IconRenderer : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    Vector2Int iconSize = new Vector2Int(256, 256);

    [SerializeField]
    float camDistance = 10f;

    [SerializeField]
    bool selectOnRender = false;

    [SerializeField]
    Transform offset = null;

    RenderTexture rt = null;

    IItem curItem = null;

    void InitRT()
    {
        if (rt != null && (rt.width != iconSize.x || rt.height != iconSize.y))
        {
            ReleaseRT();
        }

        if (rt == null)
        {
            rt = RenderTexture.GetTemporary(iconSize.x, iconSize.y);

            MainCamera.Instance.camera.targetTexture = rt;
        }
    }

    void ReleaseRT()
    {
        if (rt != null)
        {
            RenderTexture.ReleaseTemporary(rt);
            rt = null;

            MainCamera.Instance.camera.targetTexture = null;
        }
    }

    public void RenderAll()
    {
        InitRT();

        foreach (var item in ItemManager.Instance.Items)
        {
            RenderIcon(item.Key);
        }

        ReleaseRT();
    }

    public void RenderOne(string itemName)
    {
        InitRT();

        RenderIcon(itemName);

        ReleaseRT();
    }

    public void RenderObject(GameObject go, string iconName)
    {
        ClearPivot();

        GameObject inst = Instantiate(go);

        inst.transform.parent = offset.transform;
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localRotation = Quaternion.identity;
        inst.transform.localScale = Vector3.one;

        InitRT();

        RenderObjectIcon(inst, iconName);

        ReleaseRT();

    }

    void RenderIcon(string itemName)
    {
        ClearPivot();

        SpawnItem(itemName);

        Sprite sprite = RenderObjectIcon(curItem.Interactable.gameObject, itemName);

        AssignIcon(itemName, sprite);
    }

    Sprite RenderObjectIcon(GameObject go, string iconName)
    {
        Bounds bounds = go.GetBounds();

        MainCamera.Instance.transform.position = bounds.center + Vector3.back * camDistance;
        MainCamera.Instance.camera.orthographicSize = Mathf.Max(bounds.extents.y, bounds.extents.x);

        MainCamera.Instance.camera.Render();

        Sprite sprite = SaveIcon(iconName);

        if (selectOnRender) Selection.activeObject = sprite;

        return sprite;
    }

    void SpawnItem(string itemName)
    {
        curItem = ItemManager.Instance.SpawnItem(itemName);

        curItem.Interactable.transform.parent = offset.transform;
        curItem.Interactable.transform.localPosition = Vector3.zero;
        curItem.Interactable.transform.localRotation = Quaternion.identity;
        curItem.Interactable.transform.localScale = Vector3.one;

        curItem.Interactable.GetComponent<PlanetSurfaceAligner>().enabled = false;

        curItem.Init();
    }

    Sprite SaveIcon(string itemName)
    {
        Rect iconRect = new Rect(Vector2.zero, iconSize);

        Texture2D tex = new Texture2D(iconSize.x, iconSize.y);

        RenderTexture.active = rt;

        tex.ReadPixels(iconRect, 0, 0);

        Sprite sprite = Sprite.Create(tex, iconRect, ((Vector2)iconSize) * .5f);

        sprite.name = itemName;

        string path = $"Assets/Textures/Icons/{itemName}.png";

        return SaveSpriteToEditorPath(sprite, path);
    }

    void AssignIcon(string itemName, Sprite sprite)
    {
        ItemData data = ItemManager.Instance.GetData(itemName);

        data.icon = sprite;

        EditorUtility.SetDirty(data);
    }

    void ClearPivot()
    {
        for (int i = 0; i < offset.transform.childCount; i++)
        {
            DestroyImmediate(offset.transform.GetChild(0).gameObject);
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(IconRenderer))]
public class IconEditor : Editor
{
    IconRenderer iconRenderer = null;

    static string itemName = "";
    static string iconName = "";
    static GameObject prefab = null;

    private void OnEnable()
    {
        iconRenderer = target as IconRenderer;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();

        if (GUILayout.Button("Init"))
        {
            ItemManager.Instance.Load();
            MainCamera.Instance.Load();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate All Icons"))
        {
            iconRenderer.RenderAll();
        }

        EditorGUILayout.Space();

        itemName = EditorGUILayout.TextField("Item Name", itemName);

        if (GUILayout.Button("Generate This Item"))
        {
            iconRenderer.RenderOne(itemName);
        }

        EditorGUILayout.Space();

        iconName = EditorGUILayout.TextField("Icon Name", iconName);
        prefab = (GameObject)EditorGUILayout.ObjectField("Custom Object", prefab, typeof(GameObject), true);

        if (GUILayout.Button("Generate This Object"))
        {
            iconRenderer.RenderObject(prefab, iconName);
        }
    }
}
#endif