using System.Collections;
using System.Collections.Generic;

public class ItemManager : DoozyUI.Singleton<ItemManager>
{
    private Dictionary<string, Item> _mItemDic = new Dictionary<string, Item>();

	void Awake()
    {
        List<Item> items = SerializationManager.LoadFromCSV<Item>("Data/Items");
        for (int i = 0; i < items.Count; ++i)
        {
            _mItemDic.Add(items[i].ID, items[i]);
        }
    }

    public Item GetItem(string itemId)
    {
        Item item;
        if (_mItemDic.TryGetValue(itemId, out item))
            return item;

        return null;
    }
}
