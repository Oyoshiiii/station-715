using UnityEngine;

public class DoorAnimation : MonoBehaviour
{
    [SerializeField] private Door door;

    private const string Open = "Open";
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        door.OnOpen += Door_OnOpen;
    }

    private void Door_OnOpen(object sender, System.EventArgs e)
    {
        animator.SetTrigger(Open);
    }
}
