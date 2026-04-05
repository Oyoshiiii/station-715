using System;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private Weapon weapon;
    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteractAction;
    public event EventHandler OnInventoryOpenCloseAction;

    public event EventHandler OnSprintActionStarted;
    public event EventHandler OnSprintActionCanceled;

    public event EventHandler OnShootAction;
    public event EventHandler<bool> OnAimAction;
    public event EventHandler OnReloadAction;

    public event EventHandler<Vector2> OnInventoryMoveItems;

    private PlayerInputActions inputActions;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("More than 1 GameInput");
        }

        inputActions = new PlayerInputActions();

        inputActions.Player.Enable();

        inputActions.Player.Interact.performed += Interact_performed;
        inputActions.Player.Inventory.performed += Inventory_performed;
        inputActions.Player.Sprint.started += Sprint_started;
        inputActions.Player.Sprint.canceled += Sprint_canceled;

        inputActions.Player.Shoot.performed += Shoot_performed;
        inputActions.Player.Aim.performed += Aim_performed;
        inputActions.Player.Aim.canceled += Aim_performed;
        inputActions.Player.Reload.performed += Reload_performed;

        inputActions.Inventory.Inventory.performed += Inventory_performed;
        inputActions.Inventory.MoveDown.performed += MoveDown_performed;
        inputActions.Inventory.MoveUp.performed += MoveUp_performed;

        inventoryUI.OnInventoryOpened += Inventory_OnInventoryOpened;
        inventoryUI.OnInventoryClosed += Inventory_OnInventoryClosed;
    }

    private void MoveUp_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
    }

    private void MoveDown_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
    }

    private void Update()
    {
        if (weapon != null)
        {
            bool isAiming = inputActions.Player.Aim.IsPressed();
            weapon.StartAiming(isAiming);

            if (Player.Instance != null)
            {
                Player.Instance.SetAiming(isAiming);
            }
        }
    }

    private void Sprint_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnSprintActionCanceled?.Invoke(this, EventArgs.Empty);
    }

    private void Sprint_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnSprintActionStarted?.Invoke(this, EventArgs.Empty);
    }

    private void Shoot_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (obj.performed && weapon != null)
        {
            weapon.Shoot();
            OnShootAction?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Aim_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        bool isAiming = obj.performed;
        OnAimAction?.Invoke(this, isAiming);
    }

    private void Reload_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (obj.performed && weapon != null)
        {
            weapon.StartReload();
            OnReloadAction?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Inventory_OnInventoryClosed(object sender, EventArgs e)
    {
        inputActions.Player.Enable();
        inputActions.Inventory.Disable();
    }

    private void Inventory_OnInventoryOpened(object sender, EventArgs e)
    {
        inputActions.Player.Disable();
        inputActions.Inventory.Enable();
    }

    private void Inventory_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnInventoryOpenCloseAction?.Invoke(this, EventArgs.Empty);
    }

    private void Interact_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if(Player.Instance != null)
        {
            OnInteractAction?.Invoke(this, EventArgs.Empty);
        }
    }

    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = inputActions.Player.Move.ReadValue<Vector2>();
        inputVector = inputVector.normalized;

        return inputVector;
    }

    public void PlayerDead()
    {
        inputActions.Player.Disable();
    }


    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Player.Interact.performed -= Interact_performed;
            
            inputActions.Player.Shoot.performed -= Shoot_performed;
            inputActions.Player.Aim.performed -= Aim_performed;
            inputActions.Player.Aim.canceled -= Aim_performed;
            inputActions.Player.Reload.performed -= Reload_performed;
            inputActions.Dispose();
        }
    }
}
