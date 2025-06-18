using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrictableObjectDB<T> : MonoBehaviour where T : ScriptableObject
{
    static Dictionary<string, T> objects;

    public static void Init()
    {
        objects = new Dictionary<string, T>();

        var objectArray = Resources.LoadAll<T>("");
        foreach (var obj in objectArray)
        {
            if (objects.ContainsKey(obj.name))
            {
                Debug.LogError($"{obj.name}이라는 이름의 포켓몬이 두 마리 있습니다.");
                continue;
            }

            objects[obj.name] = obj;
        }
    }

    public static T GetObjectByName(string name)
    {
        if (!objects.ContainsKey(name))
        {
            Debug.LogError($"데이터베이스에 {name}이라는 이름의 포켓몬이 존재하지 않습니다.");
            return null;
        }

        return objects[name];
    }
}

