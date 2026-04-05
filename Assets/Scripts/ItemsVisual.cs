using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ItemsVisual : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewPortTransform;
    [SerializeField] private RectTransform content;

    [SerializeField] private VerticalLayoutGroup vertLayoutGroup;

    [SerializeField] private RectTransform[] slotItemsTransform;
    [SerializeField] private ItemSlot[] slotItems;

    private Vector2 oldVelocity;
    bool isUpdated;

    public static ItemsVisual Instance { get; private set; }

    private void Start()
    {
        isUpdated = false;
        oldVelocity = Vector2.zero;

        int itemsToAdd = Mathf.CeilToInt(viewPortTransform.rect.height / (slotItemsTransform[0].rect.height + vertLayoutGroup.spacing)) + 5;
    
        for(int i = 0; i < itemsToAdd; i++)
        {
            RectTransform rt = Instantiate(slotItemsTransform[i % slotItemsTransform.Length], content);
            rt.SetAsLastSibling();
        }

        for(int i = 0; i < itemsToAdd; i++)
        {
            int num = slotItemsTransform.Length - i - 1;

            while(num < 0)
            {
                num += slotItemsTransform.Length;
            }

            RectTransform rt = Instantiate(slotItemsTransform[num], content);
            rt.SetAsFirstSibling();
        }

        content.localPosition = new Vector3(content.localPosition.x, (slotItemsTransform[0].rect.height + vertLayoutGroup.spacing) * itemsToAdd,
            content.localPosition.z);
    }

    private void Update()
    {
        HandleItemsMove();
    }

    private void HandleItemsMove()
    {
        if (isUpdated)
        {
            isUpdated = false;
            scrollRect.velocity = oldVelocity;
        }

        if (content.localPosition.y > 0)
        {
            Canvas.ForceUpdateCanvases();
            oldVelocity = scrollRect.velocity;
            content.localPosition -= new Vector3(0, slotItemsTransform.Length * (slotItemsTransform[0].rect.height + vertLayoutGroup.spacing), 0);
            isUpdated = true;
        }

        if (content.localPosition.y < slotItemsTransform.Length * (slotItemsTransform[0].rect.height + vertLayoutGroup.spacing))
        {
            Canvas.ForceUpdateCanvases();
            oldVelocity = scrollRect.velocity;
            content.localPosition += new Vector3(0, slotItemsTransform.Length * (slotItemsTransform[0].rect.height + vertLayoutGroup.spacing), 0);
            isUpdated = true;
        }
    }
}