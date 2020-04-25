using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static Utility;

public class ItemManager : UniqueInstance<ItemManager>
{
    [SerializeField]
    GameObject itemTemplate = null;

    Dictionary<string, ItemData> items = new Dictionary<string, ItemData>();
    Dictionary<System.Type, System.Type> itemComponent = new Dictionary<System.Type, System.Type>();

    public Dictionary<string, ItemData> Items => items;

    List<RocketPartData> rocketParts = new List<RocketPartData>();

    public int RocketPartCount => rocketParts.Count;

    public RocketPartData GetRocketPartData(int i)
    {
        return rocketParts[i];
    }

    public void Load()
    {
        // Item Datas
        {
            ItemData[] datas = Resources.LoadAll<ItemData>("Items/");

            foreach (var data in datas)
            {
                if (items.ContainsKey(data.itemName)) Debug.LogError($"Item name '{data.itemName}' already exists !");

                items[data.itemName] = data;

                if (data.GetType() == typeof(RocketPartData))
                    rocketParts.Add(data as RocketPartData);
            }

            rocketParts.Sort((x, y) => x.partBuildIndex - y.partBuildIndex);
        }

        // Component Types
        {
            System.Type itemType = typeof(Item<>);

            Assembly assembly = Assembly.GetExecutingAssembly();
            System.Type[] types = assembly.GetTypes().Where(t => t.IsSubclassOfRawGeneric(itemType) && t != itemType).ToArray();

            foreach (var type in types)
            {
                System.Type dataType = type.BaseType.GenericTypeArguments[0];
                itemComponent[dataType] = type;
            }
        }
    }

    private void Start()
    {
        Load();
    }

    public ItemData GetData(string name)
    {
        ItemData data;

        return items.TryGetValue(name, out data) ? data : null;
    }

    public IItem SpawnItem(string name)
    {
        ItemData data = GetData(name);

        if (data == null) throw new System.Exception($"Item name '{name}' is invalid");

        IItem item = Instantiate(itemTemplate).AddComponent(itemComponent[data.GetType()]) as IItem;

        item.Interactable.name += $" ({name})";
        item.Data = data;

        return item;
    }
}
