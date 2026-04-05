using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    static List<Item> items;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("more than 1 inventory");
            Destroy(Instance);
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;

        items = new List<Item>();
    }

    public void SetItem(Item item)
    {
        if(items.Count < 8) 
        {
            items.Add(item);
            Debug.Log("Добавлен " + item.Name);
        }
        else
        {
            Debug.Log("Недостаточно место в инвентаре");
        }
    }

    public void UseItem(Item item)
    {
        items.Remove(item);
        Debug.Log("Использован предмет " + item.Name);
    }

    public bool HasRightKeyCard(Door selectedDoor)
    {
        bool hasKeyCard = false;

        foreach (Item item in items)
        {
            if (item is KeyCard)
            {
                KeyCard keyCard = item as KeyCard;
                if (selectedDoor.NextDoorName == keyCard.OpenedDoorName)
                {
                    Debug.Log("Была использована " +  keyCard.Name);
                    hasKeyCard = true;
                    break;
                }
            }
        }

        return hasKeyCard;
    }

}
