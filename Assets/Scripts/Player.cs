using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance {  get; private set; }

    private void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError("More than 1 player on the map!");
        }
        
        Instance = this;
    }

    public event EventHandler<OnSelectedDoorChangedEventArgs> OnSelectedDoorChanged;
    public class OnSelectedDoorChangedEventArgs : EventArgs
    {
        public Door selectedDoor;
    }

    [SerializeField] private GameInput gameInput;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationSpeed = 10f;
    //[SerializeField] private float sprintSpeedCoefficient = 1.5f;

    [SerializeField] private LayerMask doorMask;
    //[SerializeField] private LayerMask enemyMask;
    //[SerializeField] private LayerMask tableMask;

    private Vector3 lastInteractionDir;
    private Door selectedDoor;

    public bool IsWalking { get; private set; }

    private void Start()
    {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
    }

    private void Update()
    {
        HandleMovement();
        HandleInteractions();
    }

    public void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

        float playerRadius = 0.4f;
        float playerHeight = 1f;

        float moveDist = speed * Time.deltaTime;

        int obstacleMask = LayerMask.GetMask("Door") | LayerMask.GetMask("Default");

        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
            playerRadius, moveDir, moveDist, obstacleMask);

        //IsWalking = moveDir != Vector3.zero;

        if (!canMove)
        {
            //пробуем двинуться по X
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0);
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
                playerRadius, moveDirX, moveDist, obstacleMask);

            if (canMove)
            {
                moveDir = moveDirX;
            }
            else
            {
                //пробуем двинуться по Z
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z);
                canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
                    playerRadius, moveDirZ, moveDist, obstacleMask);

                if (canMove)
                {
                    moveDir = moveDirZ;
                }
            }
        }

        if (canMove)
        {
            /*
             * isSprinting
             */

            transform.position += speed * moveDir * Time.deltaTime;
        }

        transform.forward = Vector3.Slerp(transform.forward, moveDir, rotationSpeed * Time.deltaTime);

    }

    private void HandleInteractions()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

        if(moveDir != Vector3.zero)
        {
            lastInteractionDir = moveDir;
        }

        float interactionDist = 2f;

        int interactionMask = LayerMask.GetMask("Door") | LayerMask.GetMask("Enemy") |
            LayerMask.GetMask("Table") /*LayerMask.GetMask("Loot")*/ ;

        if(Physics.Raycast(transform.position, lastInteractionDir, out RaycastHit raycastHit,
            interactionDist, interactionMask))
        {
            Debug.Log(raycastHit.transform);
            if(raycastHit.transform.TryGetComponent(out Door door))
            {
                if(door != selectedDoor)
                {
                    SetSelectedDoor(door);
                }
            }
            else SetSelectedDoor(null);
        }
        else
        {
            SetSelectedDoor(null);
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if(selectedDoor != null)
        {
            selectedDoor.Interact(this);
        }
    }

    private void SetSelectedDoor(Door door)
    {
        this.selectedDoor = door;
        OnSelectedDoorChanged?.Invoke(this, new OnSelectedDoorChangedEventArgs
        {
            selectedDoor = this.selectedDoor
        });
    }
}
