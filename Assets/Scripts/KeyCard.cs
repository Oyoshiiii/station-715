using UnityEngine;

public class KeyCard : Item
{
    [SerializeField] private Door.DoorName openedDoorName;

    public Door.DoorName OpenedDoorName { get; private set; }
    public static bool WasUsed {  get; private set; }

    private void Awake()
    {
        OpenedDoorName = openedDoorName;
    }
}
