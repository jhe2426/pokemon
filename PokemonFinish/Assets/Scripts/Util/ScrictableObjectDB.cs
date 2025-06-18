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
                Debug.LogError($"{obj.name}�̶�� �̸��� ���ϸ��� �� ���� �ֽ��ϴ�.");
                continue;
            }

            objects[obj.name] = obj;
        }
    }

    public static T GetObjectByName(string name)
    {
        if (!objects.ContainsKey(name))
        {
            Debug.LogError($"�����ͺ��̽��� {name}�̶�� �̸��� ���ϸ��� �������� �ʽ��ϴ�.");
            return null;
        }

        return objects[name];
    }
}

