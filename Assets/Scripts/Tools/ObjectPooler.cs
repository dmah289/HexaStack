using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObjectPooler
{
    public static Dictionary<string, Queue<Component>> poolDict = new Dictionary<string, Queue<Component>>();
    public static Dictionary<string, Component> poolLookUp = new Dictionary<string, Component>();

    public static void EnqueueObject<T> (string itemName, T item) where T : Component
    {
        if (!item.gameObject.activeSelf)
            return;

        item.transform.position = Vector3.zero;
        item.transform.rotation = Quaternion.Euler(0, 0, 0);
        item.gameObject.SetActive(false);
        poolDict[itemName].Enqueue(item);
    }

    public static T DequeueObject<T> (string itemName) where T : Component
    {
        if (poolDict[itemName].TryDequeue(out var item))
        {
            item.gameObject.SetActive(true);
            return (T)item;
        }
        T newInstance = Object.Instantiate((T)poolLookUp[itemName]);
        EnqueueObject(itemName, newInstance);
        return DequeueObject<T>(itemName);
    }

    public static void SetUpPool<T> (string itemName, int poolSize, T itemPrefab) where T : Component
    {
        poolLookUp.Add(itemName, itemPrefab);
        poolDict.Add(itemName, new Queue<Component>());
        for(int i = 0; i < poolSize; i++)
        {
            T instance = Object.Instantiate(itemPrefab);
            EnqueueObject(itemName, instance);
        }
    }
}
