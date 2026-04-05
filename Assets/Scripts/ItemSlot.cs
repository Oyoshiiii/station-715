using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemDescription;

    private Color color;

    public Item Item { get; private set; }

    private void Start()
    {
        if(iconImage != null) { color = iconImage.color; }
    }
    
    public void SetItem(Item newItem)
    {
        Item = newItem;
        if (newItem != null && iconImage != null)
        {
            iconImage.sprite = newItem.ItemSO.sprite;
            itemName.text = newItem.Name;
            itemDescription.text = newItem.Description;

            iconImage.enabled = true;
            color.a = 255;
        }
        else
        {
            itemName.text = "";
            itemDescription.text = "";
            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
            color.a = 0;
        }
    }
}
