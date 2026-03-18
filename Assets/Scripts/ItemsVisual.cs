using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ItemsVisual : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform sampleListItemSlot;
    [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;

    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemDescription;

    [Header("Settings")]
    [SerializeField] private float scrollSpeed = 10f; //скорость прокрутки
    [SerializeField] private float snapThreshold = 20f; //порог для привязки

    private float itemHeight;
    private int totalItems;
    private int currentItemIndex = 0;
    private bool isSnapping = false;
    private float targetPosition;
    private List<RectTransform> itemSlots = new List<RectTransform>();

    private void Start()
    {
        // Получаем высоту одного элемента
        itemHeight = sampleListItemSlot.rect.height + verticalLayoutGroup.spacing;

        // Собираем все слоты
        foreach (RectTransform child in content)
        {
            itemSlots.Add(child);
        }

        totalItems = itemSlots.Count;

        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnInventoryMoveItems += HandleInventoryMove;
        }

        if (inventoryUI != null)
        {
            inventoryUI.OnInventoryOpened += OnInventoryOpened;
            inventoryUI.OnInventoryClosed += OnInventoryClosed;
        }
    }

    private void OnDestroy()
    {
        if (GameInput.Instance != null)
        {
            GameInput.Instance.OnInventoryMoveItems -= HandleInventoryMove;
        }

        if (inventoryUI != null)
        {
            inventoryUI.OnInventoryOpened -= OnInventoryOpened;
            inventoryUI.OnInventoryClosed -= OnInventoryClosed;
        }
    }

    private void HandleInventoryMove(object sender, Vector2 moveVector)
    {
        if (!enabled) return;

        Debug.Log($"HandleInventoryMove: {moveVector}");

        if (moveVector.y > 0) // W - движение вверх (к первому предмету)
        {
            MoveToPreviousItem();
        }
        else if (moveVector.y < 0) // S - движение вниз (к последнему предмету)
        {
            MoveToNextItem();
        }
    }

    private void Update()
    {
        if (isSnapping)
        {
            SmoothScrollToTarget();
        }
    }

    private void OnInventoryOpened(object sender, System.EventArgs e)
    {
        enabled = true;
    }

    private void OnInventoryClosed(object sender, System.EventArgs e)
    {
        enabled = false;
    }

    private void MoveToNextItem()
    {
        if (isSnapping) return;

        currentItemIndex = (currentItemIndex + 1) % totalItems;
        targetPosition = currentItemIndex * itemHeight;
        isSnapping = true;

        Debug.Log($"Moving to next item: {currentItemIndex}");
    }

    private void MoveToPreviousItem()
    {
        if (isSnapping) return;

        currentItemIndex--;
        if (currentItemIndex < 0)
        {
            currentItemIndex = totalItems - 1;
        }
        targetPosition = currentItemIndex * itemHeight;
        isSnapping = true;

        Debug.Log($"Moving to previous item: {currentItemIndex}");
    }

    private void SmoothScrollToTarget()
    {
        float currentY = -content.anchoredPosition.y;
        float newY = Mathf.Lerp(currentY, targetPosition, Time.deltaTime * scrollSpeed);

        content.anchoredPosition = new Vector2(
            content.anchoredPosition.x,
            -newY
        );

        if (Mathf.Abs(newY - targetPosition) < 0.1f)
        {
            content.anchoredPosition = new Vector2(
                content.anchoredPosition.x,
                -targetPosition
            );
            isSnapping = false;
            scrollRect.velocity = Vector2.zero;

            // Обновляем отображение информации о предмете
            UpdateItemInfo(currentItemIndex);
        }
    }

    private void UpdateItemInfo(int index)
    {
        ItemSlot slot = itemSlots[index].GetComponent<ItemSlot>();
        if (slot != null && slot.Item != null)
        {
            itemName.text = slot.Item.itemName;
            itemDescription.text = slot.Item.description;
        }
    }

    public void SetCurrentItem(int index)
    {
        if (index >= 0 && index < totalItems)
        {
            currentItemIndex = index;
            targetPosition = currentItemIndex * itemHeight;
            content.anchoredPosition = new Vector2(
                content.anchoredPosition.x,
                -targetPosition
            );
            UpdateItemInfo(currentItemIndex);
        }
    }
}