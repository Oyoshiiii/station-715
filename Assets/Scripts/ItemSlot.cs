using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    private Color color;

    public Item Item { get; private set; }

    private void Start()
    {
        color = iconImage.color;
    }
    
    public void SetItem(Item newItem)
    {
        Item = newItem;
        if (newItem != null)
        {
            iconImage.sprite = newItem.icon;
            iconImage.enabled = true;
            color.a = 255;
        }
        else
        {
            iconImage.enabled = false;
            color.a = 0;
        }
    }
}

// Пример класса предмета
[System.Serializable]
public class Item
{
    public string itemName;
    public string description;
    public Sprite icon;
}