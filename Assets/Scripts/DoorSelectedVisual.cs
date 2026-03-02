using UnityEngine;

public class DoorSelectedVisual : MonoBehaviour
{
    [SerializeField] private Door door;

    [SerializeField]
    private GameObject[] visualGameObjects;

    private void Start()
    {
        Player.Instance.OnSelectedDoorChanged += Player_OnSelectedDoorChanged;
    }

    private void Player_OnSelectedDoorChanged(object sender, Player.OnSelectedDoorChangedEventArgs e)
    {
        if (door == e.selectedDoor && !door.IsOpened)
        {
            Show();
        }
        else if (door == e.selectedDoor && door.IsOpened)
        {
            Hide();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        foreach (var visualGameObject in visualGameObjects)
        {
            visualGameObject.SetActive(true);
        }
    }

    private void Hide()
    {
        foreach (var visualGameObject in visualGameObjects)
        {
            visualGameObject.SetActive(false);
        }
    }
}