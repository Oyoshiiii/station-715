using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField]
    private Player player;

    private const string ISWALKING = "IsWalking";

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        animator.SetBool(ISWALKING, false);
    }

    private void Update()
    {
        animator.SetBool(ISWALKING, player.IsWalking);
    }
}
