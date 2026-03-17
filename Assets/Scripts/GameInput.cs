using System;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    [SerializeField] private InventoryUI inventoryUI;
    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteractAction;
    public event EventHandler OnInventoryOpenCloseAction;

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

        inputActions.Inventory.Inventory.performed += Inventory_performed;
        inputActions.Inventory.MoveItems.performed += MoveItems_performed;

        inventoryUI.OnInventoryOpened += Inventory_OnInventoryOpened;
        inventoryUI.OnInventoryClosed += Inventory_OnInventoryClosed;
    }

    private void MoveItems_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        // Получаем вектор движения и вызываем событие
        Vector2 moveVector = obj.ReadValue<Vector2>();
        OnInventoryMoveItems?.Invoke(this, moveVector);
        Debug.Log($"Move items performed: {moveVector}"); // Для отладки
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

    public Vector2 GetInventoryMoveVector()
    {
        return inputActions.Inventory.MoveItems.ReadValue<Vector2>();
    }

    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Player.Interact.performed -= Interact_performed;
            inputActions.Inventory.MoveItems.performed -= MoveItems_performed; // Добавлено
            inputActions.Dispose();
        }
    }
}
