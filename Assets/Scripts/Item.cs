using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private ItemSO itemSO;
    public ItemSO ItemSO { get { return itemSO; } }

    private Table table;

    [SerializeField] private string name;
    public string Name {  get; private set; }

    [SerializeField] private string description;
    public string Description { get; private set; }


    private void Awake()
    {
        Description = description;
        Name = name;
    }
    public static Item SpawnItem(ItemSO itemSO, Table table)
    {
        Transform kitchenObjectTransform = Instantiate(itemSO.prefab);
        Item item = kitchenObjectTransform.GetComponent<Item>();
        item.SetItemParent(table);
        return item;
    }

    private void SetItemParent(Table table)
    {
        if (this.table != null)
        {
            this.table.ClearItem();
        }

        this.table = table;
        table.SetItem(this);

        transform.parent = table.GetItemFollowTransform();
        transform.localPosition = Vector3.zero;
    }

    public void SetItemToPlayer(Player player)
    {
        player.SetItem(this);
        DestroySelf();
    }

    public void DestroySelf()
    {
        table.ClearItem();
        Destroy(gameObject);
    }
}
