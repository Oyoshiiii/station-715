using UnityEngine;

public class DoorAnimation : MonoBehaviour
{
    [SerializeField] private Door door;
    [SerializeField] private Animator animator;

    private const string Open = "Open";

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Start()
    {
        if (door != null)
        {
            door.OnOpen += Door_OnOpen;
        }
    }

    private void Door_OnOpen(object sender, System.EventArgs e)
    {
        animator.SetTrigger(Open);
    }

    private void OnDestroy()
    {
        if (door != null)
        {
            door.OnOpen -= Door_OnOpen;
        }
    }
}