using System.Collections.Generic;
using UnityEngine;

public class ItemDB
{
    static Dictionary<string, ItemBase> itmes;
    
    public static void Init()
    {
        itmes = new Dictionary<string, ItemBase>();

        var ItemList = Resources.LoadAll<ItemBase>("");
        foreach (var itme in ItemList)
        {
            if (itmes.ContainsKey(itme.Name))
            {
                Debug.LogError($"{itme.Name}이라는 이름의 아이템이 2개 있습니다.");
                continue;
            }

            itmes[itme.Name] = itme;
        }
    }

    public static ItemBase GetItemByName(string name)
    {
        if (!itmes.ContainsKey(name))
        {
            Debug.LogError($"데이터베이스에 {name}이라는 이름의 아이템이 존재하지 않습니다.");
            return null;
        }

        return itmes[name];
    }
}
