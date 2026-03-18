using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public enum PlayerState
    {
        Normal,
        OpenDoor,
        WaitingForLoad
    }

    private PlayerState state = PlayerState.Normal;

    public static Player Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
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

    public event EventHandler OnDoorOpened;

    [SerializeField] private GameInput gameInput;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationSpeed = 10f;

    private Vector3 lastInteractionDir;
    private Door selectedDoor;

    public bool IsWalking { get; private set; }

    private void Start()
    {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
    }

    private void Update()
    {
        Debug.Log(Instance);
        switch (state)
        {
            case PlayerState.Normal:
                HandleMovement();
                //HandleRotate();
                HandleInteractions();
                break;

            case PlayerState.OpenDoor:
                break;

            case PlayerState.WaitingForLoad:
                break;
        }
    }

    private void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

        float playerRadius = 0.4f;
        float playerHeight = 1f;

        float moveDist = speed * Time.deltaTime;

        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
            playerRadius, moveDir, moveDist);

        if (!canMove)
        {
            //пробуем двинуться по X
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0);
            canMove = moveDir.x != 0 && !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
                playerRadius, moveDirX, moveDist);

            if (canMove)
            {
                moveDir = moveDirX;
            }
            else
            {
                //пробуем двинуться по Z
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z);
                canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
                    playerRadius, moveDirZ, moveDist);

                if (canMove)
                {
                    moveDir = moveDirZ;
                }
            }
        }

        if (canMove)
        {
            transform.position += speed * moveDir * Time.deltaTime;
        }

        if (moveDir != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, moveDir, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleRotate()
    {

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);
        Vector3 lookDir = transform.forward;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 targetDir = hit.point - transform.position;
            targetDir.y = 0;

            if (targetDir != Vector3.zero)
            {
                lookDir = targetDir.normalized;

                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void HandleInteractions()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

        if (moveDir != Vector3.zero)
        {
            lastInteractionDir = moveDir;
        }

        float interactionDist = 2f;

        if (Physics.Raycast(transform.position, lastInteractionDir, out RaycastHit raycastHit,
            interactionDist))
        {
            Debug.Log(raycastHit.transform);
            if (raycastHit.transform.TryGetComponent(out Door door))
            {
                if (door != selectedDoor && !door.IsAnimate)
                {
                    Debug.Log("SetSelected Door");
                    SetSelectedDoor(door);
                }
            }
            else
            {
                Debug.Log("No Door");
                SetSelectedDoor(null);
            }
        }
        else
        {
            Debug.Log("No Door");
            SetSelectedDoor(null);
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (selectedDoor != null && state == PlayerState.Normal)
        {
            state = PlayerState.OpenDoor;
            selectedDoor.OnOpenAnimationComplete += Door_OnOpenAnimationComplete;
            selectedDoor.Interact(this);
        }
    }

    private void Door_OnOpenAnimationComplete(object sender, EventArgs e)
    {
        Door door = sender as Door;
        if (door != null)
        {
            door.OnOpenAnimationComplete -= Door_OnOpenAnimationComplete;
            state = PlayerState.WaitingForLoad;
            OnDoorOpened?.Invoke(this, EventArgs.Empty);
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

    public Door GetSelectedDoor()
    {
        return selectedDoor;
    }

    private void OnDestroy()
    {
        if (selectedDoor != null)
        {
            selectedDoor.OnOpenAnimationComplete -= Door_OnOpenAnimationComplete;
        }
    }
}