using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

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
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;

        if (savedHealth >= 0f)
        {
            health = savedHealth;
        }
        else
        {
            health = healthMax; 
        }
    }

    public event EventHandler<OnSelectedDoorChangedEventArgs> OnSelectedDoorChanged;
    public class OnSelectedDoorChangedEventArgs : EventArgs
    {
        public Door selectedDoor;
    }

    public event EventHandler<OnSelectedTableChangedEventArgs> OnSelectedTableChanged;
    public class OnSelectedTableChangedEventArgs : EventArgs
    {
        public Table selectedTable;
    }

    public event EventHandler OnDoorOpened;

    public event EventHandler OnPlayerDead;

    [SerializeField] private GameInput gameInput;

    [SerializeField] private Inventory inventory;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float sprintSpeedCoefficient = 1.5f;
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private float pushBackForce = 7f;
    [SerializeField] private float pushBackDecayRate = 15f;

    [SerializeField] private Transform weaponPoint;
    [SerializeField] private GameObject weapon;

    [SerializeField] private float healthMax = 100f;

    private Vector3 pushBackVelocity;

    private float health;
    private static float savedHealth = -1f;

    private Vector3 lastInteractionDir;

    private Door selectedDoor;
    private Table selectedTable;

    private static Transform playerPointPosition;

    private Vector2 lastMousePosition;
    private bool mouseMoved;
    private float mouseMoveThreshold = 0.01f;

    public bool IsWalking { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsAiming { get; private set; } = false;

    [SerializeField] private float playerRadius = 0.4f;
    private float playerHeight = 1f;

    //private Item item;

    private void Start()
    {
        if (playerPointPosition != null)
        {
            transform.position = playerPointPosition.position;
            transform.eulerAngles = playerPointPosition.eulerAngles;
        }

        gameInput.OnInteractAction += GameInput_OnInteractAction;

        gameInput.OnSprintActionCanceled += GameInput_OnSprintActionCanceled;
        gameInput.OnSprintActionStarted += GameInput_OnSprintActionStarted;

        Door.OnPlayerPoint += Door_OnPlayerPoint;

        if (weapon != null && weaponPoint != null)
        {
            weapon.SetActive(true);

            weapon.transform.parent = weaponPoint;
            weapon.transform.localPosition = Vector3.zero;
        }

        lastMousePosition = Mouse.current.position.ReadValue();

        Debug.Log($"HP игрока: {health}");
    }

    private void Update()
    {
        Vector2 currentMousePosition = Mouse.current.position.ReadValue();
        mouseMoved = Vector2.Distance(currentMousePosition, lastMousePosition) > mouseMoveThreshold;
        lastMousePosition = currentMousePosition;

        switch (state)
        {
            case PlayerState.Normal:
                HandleMovement();
                if ((!IsWalking || !IsSprinting || IsAiming) && mouseMoved)
                    HandleRotate();
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
        if (pushBackVelocity.sqrMagnitude > 0.01f && !IsAiming)
        {
            Vector3 pushDir = new Vector3(pushBackVelocity.x, 0f, pushBackVelocity.z).normalized;
            float pushDist = pushBackVelocity.magnitude * Time.deltaTime;

            if (!Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
                                      playerRadius, pushDir, pushDist))
            {
                transform.position += pushDir * pushDist;
            }

            pushBackVelocity = Vector3.Lerp(pushBackVelocity, Vector3.zero, pushBackDecayRate * Time.deltaTime);
        }

        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

        IsWalking = moveDir != Vector3.zero;

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

        if(canMove)
        {
            float speedModifier = IsSprinting ? sprintSpeedCoefficient : 1;
            transform.position += speed * moveDir * Time.deltaTime * speedModifier;
        }
        else
        {
            HandleRotate();
        }

        if (moveDir != Vector3.zero)
        {
            if (!IsAiming)
            {
                transform.forward = Vector3.Slerp(transform.forward, moveDir, rotationSpeed * Time.deltaTime);
            }
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
                lookDir = targetDir.normalized;
        }
        else
        {
            Vector3 farPoint = ray.GetPoint(100f);
            Vector3 targetDir = farPoint - transform.position;
            targetDir.y = 0;
            if (targetDir != Vector3.zero)
                lookDir = targetDir.normalized;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lookDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void HandleInteractions()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

        if (moveDir != Vector3.zero)
        {
            lastInteractionDir = moveDir;
        }

        float interactionDist = 1.5f;

        if (Physics.Raycast(transform.position, lastInteractionDir, out RaycastHit raycastHit,
            interactionDist))
        {
            //Debug.Log(raycastHit.transform);
            if (raycastHit.transform.TryGetComponent(out Door door))
            {
                if (door != selectedDoor && !door.IsAnimate)
                {
                    SetSelectedDoor(door);
                }
            }
            else
            {
                if (raycastHit.transform.TryGetComponent(out Table table))
                {
                    if (table != selectedTable)
                    {
                        SetSelectedTable(table);
                    }
                }
                else
                {
                    SetSelectedTable(null);
                    SetSelectedDoor(null);
                }
            }
        }
        else
        {
            SetSelectedDoor(null);
            SetSelectedTable(null);
        }
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (selectedDoor != null && state == PlayerState.Normal)
        {
            if(selectedDoor.State == Door.DoorState.Unlocked)
            {
                state = PlayerState.OpenDoor;

                selectedDoor.OnOpenAnimationComplete += Door_OnOpenAnimationComplete;
                selectedDoor.Interact();
            }
            else if (selectedDoor.State == Door.DoorState.NeedKeyCard)
            {
                selectedDoor.Interact(inventory.HasRightKeyCard(selectedDoor));
            }
            else
            {
                selectedDoor.Interact();
            }
        }
        else if(selectedTable != null)
        {
            selectedTable.Interact(this);
        }
    }

    private void GameInput_OnSprintActionStarted(object sender, EventArgs e)
    {
        IsSprinting = true;
    }
    private void GameInput_OnSprintActionCanceled(object sender, EventArgs e)
    {
        IsSprinting = false;
    }

    private void Door_OnPlayerPoint(object sender, Door.OnPlayerPointEventArgs e)
    {
        playerPointPosition = e.playerPointTransform;
    }

    private void Door_OnOpenAnimationComplete(object sender, EventArgs e)
    {
        Door door = sender as Door;
        if (door != null)
        {
            door.OnOpenAnimationComplete -= Door_OnOpenAnimationComplete;

            SaveHealth();

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
    
    private void SetSelectedTable(Table table)
    {
        this.selectedTable = table;
        OnSelectedTableChanged?.Invoke(this, new OnSelectedTableChangedEventArgs
        {
            selectedTable = this.selectedTable
        });
    }

    public Table GetSelectedTable()
    {
        return selectedTable;
    }

    public void SetItem(Item item)
    {
        inventory.SetItem(item);
    }

    public float GetHealth()
    {
        return health;
    }

    public void SaveHealth()
    {
        savedHealth = health;
    }

    public void SetAiming(bool aiming)
    {
        IsAiming = aiming;
    }

    public void TakeDamageFromEnemy(float dmg, Vector3 pushBackDir)
    {
        health -= dmg;

        if (health <= 0)
        {
            health = 0f;
            OnPlayerDead?.Invoke(this, EventArgs.Empty);
            StartCoroutine(Die());
            return;
        }

        if (pushBackDir != Vector3.zero)
        {
            pushBackVelocity = pushBackDir.normalized * pushBackForce;
        }
    }

    private void OnDestroy()
    {
        if (selectedDoor != null)
        {
            selectedDoor.OnOpenAnimationComplete -= Door_OnOpenAnimationComplete;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public IEnumerator Die()
    {
        savedHealth = -1f;

        yield return new WaitForSeconds(1f);

        gameInput.PlayerDead();

        StopAllCoroutines();

        transform.position = new Vector3(0, 0, -100);
    }
}