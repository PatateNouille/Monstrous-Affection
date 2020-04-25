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

    void RenderIcon(string itemName)
    {
        SpawnItem(itemName);

        Bounds bounds = curItem.Interactable.gameObject.GetBounds();

        MainCamera.Instance.transform.position = bounds.center + Vector3.back * 10f;
        MainCamera.Instance.camera.orthographicSize = Mathf.Max(bounds.extents.y, bounds.extents.x);

        MainCamera.Instance.camera.Render();

        Sprite sprite = SaveIcon(itemName);

        AssignIcon(itemName, sprite);
    }

    void SpawnItem(string itemName)
    {
        if (curItem != null)
        {
            DestroyImmediate(curItem.Interactable.gameObject);
        }

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

        AssetDatabase.SaveAssets();
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(IconRenderer))]
public class IconEditor : Editor
{
    IconRenderer iconRenderer = null;

    string itemName = "";

    private void OnEnable()
    {
        iconRenderer = target as IconRenderer;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Init"))
        {
            ItemManager.Instance.Load();
            MainCamera.Instance.Load();
        }

        if (GUILayout.Button("Generate All Icons"))
        {
            iconRenderer.RenderAll();
        }

        itemName = EditorGUILayout.TextField("Item Name", itemName);

        if (GUILayout.Button("Generate This Item"))
        {
            iconRenderer.RenderOne(itemName);
        }
    }
}
#endif