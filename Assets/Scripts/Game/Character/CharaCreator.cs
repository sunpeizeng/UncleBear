using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UncleBear
{
    public enum CharaEnum
    {
        UncleBear = 0,
        Andy,
        Giraffe,
        Elephant,
        Rabbit,
        Max,
    }

    public static class CharaCreator
    {
        public static Dictionary<int, CharaBase> DictCharacters = new Dictionary<int, CharaBase>();
        public static List<CharaCustomer> Customers = new List<CharaCustomer>();
        public static CharaBase Chef;
        public static CharaBase Waiter;
        private static List<CharaDataItem> _dataItems;

        public static void CreateCharacter(CharaEnum type)
        {
            if (_dataItems == null)
                _dataItems = SerializationManager.LoadFromCSV<CharaDataItem>("Data/Characters/CharacterDatas");

            if (DictCharacters.ContainsKey((int)type))
                return;
            var item = _dataItems.Find(p => p.strName == type.ToString());
            if (item == null)
                return;

            var objChara = GameObject.Instantiate(ItemManager.Instance.GetItem(item.strID).Prefab, Vector3.one * 500, Quaternion.identity);
            objChara.name = item.strName;
            objChara.transform.localScale = Vector3.one * item.fScale;
            switch (objChara.GetComponent<CharaData>().eType)
            {
                case CharaData.CharaClassType.Chef:
                    Chef = objChara.AddMissingComponent<CharaChef>();
                    DictCharacters.Add((int)type, Chef);
                    Chef.Initialize(type, item.strAnimID);
                    break;
                case CharaData.CharaClassType.Waiter:
                    Waiter = objChara.AddMissingComponent<CharaWaiter>();
                    DictCharacters.Add((int)type, Waiter);
                    Waiter.Initialize(type, item.strAnimID);
                    break;
                case CharaData.CharaClassType.Customer:
                    var customer = objChara.AddMissingComponent<CharaCustomer>();
                    Customers.Add(customer);
                    DictCharacters.Add((int)type, customer);
                    customer.Initialize(type, item.strAnimID);
                    break;
            }
        }
    }

    public class CharaDataItem : ICSVDeserializable
    {
        public string strID;
        public string strName;
        public string strAnimID;
        public float fScale;

        public virtual void CSVDeserialize(Dictionary<string, string[]> data, int index)
        {
            strID = data["ID"][index];
            strName = data["Name"][index];
            strAnimID = data["AnimID"][index];
            fScale = float.Parse(data["Scale"][index]);
        }
    }
}
