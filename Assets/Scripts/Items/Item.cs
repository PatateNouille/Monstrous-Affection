using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Inventory
{
    [SerializeField]
    public uint maxItemCount = 9999;

    public Dictionary<string, uint> Items { get; } = new Dictionary<string, uint>();

    public uint ItemCount { get; private set; } = 0;

    public bool IsFull => ItemCount >= maxItemCount;

    public delegate void OnItemChanged(string name, uint before, uint now);
    public OnItemChanged onItemChanged = null;

    public delegate void OnInventoryChanged();
    public OnInventoryChanged onInventoryChanged = null;

    public uint Count(string name)
    {
        uint val;

        return Items.TryGetValue(name, out val) ? val : 0;
    }

    public uint Add(string name, uint count)
    {
        uint had = Count(name);

        if (IsFull) return had;

        count = (uint)Mathf.Min((int)(maxItemCount - ItemCount), (int)count);

        uint have = had + count;

        ItemCount += count;
        Items[name] = have;

        onItemChanged?.Invoke(name, had, have);
        onInventoryChanged?.Invoke();

        return have;
    }

    public uint Remove(string name, uint count)
    {
        uint had;

        if (!Items.TryGetValue(name, out had))
        {
            return 0;
        }
        else if (had <= count)
        {
            ItemCount -= had;
            Items.Remove(name);

            onItemChanged?.Invoke(name, had, 0);
            onInventoryChanged?.Invoke();

            return 0;
        }
        else
        {
            uint have = had - count;
            ItemCount -= count;
            Items[name] = have;

            onItemChanged?.Invoke(name, had, have);
            onInventoryChanged?.Invoke();

            return have;
        }
    }

    public override string ToString()
    {
        StringBuilder builder = new StringBuilder();

        foreach (var item in Items)
        {
            builder.AppendLine(item.Key + " : " + item.Value);
        }

        return builder.ToString();
    }
}

[System.Serializable]
public class ItemFilter
{
    [System.Serializable]
    public class Info
    {
        public string name = "";
        public bool allowed = true;
        public bool isType = false;

        public Info(string _name, bool _allowed, bool _type = false)
        {
            name = _name;
            allowed = _allowed;
            isType = _type;
        }
    }

    public bool allowedByDefault = true;

    public List<Info> infos = new List<Info>();

    public bool IsItemAllowed(IItem item)
    {
        if (infos.Count == 0) return allowedByDefault;

        foreach (var info in infos)
        {
            if (info.isType)
            {
                if (info.name == item.Data.GetType().Name) return info.allowed;
            }
            else if (info.name == item.Data.itemName) return info.allowed;
        }

        return allowedByDefault;
    }

    public bool Add(string name, bool allowed, bool type = false)
    {
        Info info = infos.FirstOrDefault(i => i.name == name);

        if (info != null)
        {
            info.allowed = allowed;
            info.isType = type;

            return false;
        }

        infos.Add(new Info(name, allowed, type));

        return true;
    }
}

public interface IItem
{
    GameObject gameObject { get; }

    Transform transform { get; }

    Interactable Interactable { get; }

    ItemData Data { get; set; }

    Collider Collider { get; }

    bool IsGrabbed { get; }

    float ItemHeight { get; }

    void SetPlanetInfluence(bool enabled);

    void Init();
}

public abstract class Item<T> : Interactable, IItem where T : ItemData
{
    [SerializeField]
    public T data = null;

    public Collider Collider { get; private set; } = null;

    protected PlanetSurfaceAligner aligner = null;
    protected PlanetAttracter attracter = null;

    public Interactable Interactable => this;

    public ItemData Data { get => data; set => data = value as T; }

    public bool IsGrabbed { get; private set; } = false;

    public float ItemHeight { get; private set; } = 0f;

    protected virtual void Awake()
    {
        aligner = GetComponent<PlanetSurfaceAligner>();
        attracter = GetComponent<PlanetAttracter>();
    }

    protected virtual void Start()
    {
        Init();
    }

    public void Init()
    {
        highlighOffset = data.highlightOffset;
        highlightSize = data.highlightSize;

        GameObject custom = Instantiate(data.customData);

        custom.transform.SetParent(transform, false);
        custom.name = "Custom Data - " + custom.name;

        try
        {
            Collider = gameObject.GetComponentsInChildren<Collider>().First(c => c.CompareTag("ItemCollider"));
        }
        catch (System.InvalidOperationException)
        {
            throw new System.Exception("No ItemCollider found for item '" + data.itemName + "'");
        }

        Vector3 ptUp = Collider.ClosestPoint(transform.TransformPoint(Vector3.up * 1000f));
        Vector3 ptDown = Collider.ClosestPoint(transform.TransformPoint(Vector3.down * 1000f));

        ItemHeight = Vector3.Distance(ptUp, ptDown);
    }

    public void SetPlanetInfluence(bool enabled)
    {
        aligner.enabled = enabled;
        attracter.enabled = enabled;
    }

    public override bool CanBeInteractedWith()
    {
        ItemData gt = Player.Instance.GrabbedType;

        return base.CanBeInteractedWith() && (gt == null || gt == data);
    }

    public override bool Interact()
    {
        if (!CanBeInteractedWith()) return false;

        SetPlanetInfluence(false);
        Collider.enabled = false;

        IsGrabbed = true;

        SetInteractable(false);

        return true;
    }

    public override void EndInteraction()
    {
        SetPlanetInfluence(true);
        Collider.enabled = true;

        IsGrabbed = false;

        SetInteractable(true);
    }
}
