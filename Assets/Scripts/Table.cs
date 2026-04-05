using System;
using UnityEngine;

public class Table : MonoBehaviour
{
    [SerializeField] private ItemSO itemSO;
    [SerializeField] private Transform ItemTopPoint;

    [SerializeField] private GameObject keyCardTakeUpUIMessage;
    [SerializeField] private float openKeyCardTakeUpUIMessageTime = 3f;

    private static bool WasTaken;

    private Item item;

    private void Awake()
    {
        if(!WasTaken && !HasItem())
        {
            item = Item.SpawnItem(itemSO, this);
        }

        if(keyCardTakeUpUIMessage != null)
        {
            keyCardTakeUpUIMessage.SetActive(false);
        }
    }

    public void Interact(Player player)
    {
        if (HasItem())
        {
            GetItem().SetItemToPlayer(player);
            WasTaken = true;

            ClearItem();

            StartCoroutine(ShowMessageCoroutine(keyCardTakeUpUIMessage));
        }
    }

    private System.Collections.IEnumerator ShowMessageCoroutine(GameObject messageObject)
    {
        if (messageObject == null) yield break;

        messageObject.SetActive(true);
        yield return new WaitForSeconds(openKeyCardTakeUpUIMessageTime);

        if (messageObject != null)
        {
            messageObject.SetActive(false);
        }
    }

    public Transform GetItemFollowTransform()
    {
        return ItemTopPoint;
    }

    public Item GetItem()
    {
        return item;
    }

    public void ClearItem()
    {
        item = null;
    }
    
    public bool HasItem()
    {
        return item != null;
    }

    public void SetItem(Item item)
    {
        this.item = item;
    }
}
