using UnityEngine;

public class DoorSelectedVisual : MonoBehaviour
{
    [SerializeField] private Door door;

    [SerializeField] private GameObject interactBtnUI;
    [SerializeField] private GameObject openDoorBtnUI;

    private void Start()
    {
        Player.Instance.OnSelectedDoorChanged += Player_OnSelectedDoorChanged;
        door.OnOpenDoorBtnShowCompleted += Door_OnOpenDoorBtnShowCompleted;

        if (interactBtnUI != null)
            Hide(interactBtnUI);

        if (openDoorBtnUI != null)
            Hide(openDoorBtnUI);
    }

    private void Door_OnOpenDoorBtnShowCompleted(object sender, System.EventArgs e)
    {
        Hide(openDoorBtnUI);

        if(door.State == Door.DoorState.Unlocked)
        {
            Show(interactBtnUI);
        }
    }

    private void Player_OnSelectedDoorChanged(object sender, Player.OnSelectedDoorChangedEventArgs e)
    {
        if (door == null) return;

        if (door == e.selectedDoor && !door.IsOpened)
        {
            if (door.State == Door.DoorState.NeedKeyCard)
            {
                Hide(interactBtnUI);
                Show(openDoorBtnUI);
            }

            if (door.State == Door.DoorState.Unlocked)
            {
                Hide(openDoorBtnUI);
                Show(interactBtnUI);
            }
        }
        else if (door == e.selectedDoor && door.IsOpened)
        {
            Hide(interactBtnUI);
            Hide(openDoorBtnUI);
        }
        else
        {
            Hide(interactBtnUI);
            Hide(openDoorBtnUI);
        }
    }

    private void Show(GameObject gameObject)
    {
        gameObject.SetActive(true);
    }

    private void Hide(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if(Player.Instance != null)
        {
            Player.Instance.OnSelectedDoorChanged -= Player_OnSelectedDoorChanged;
        }
    }
}