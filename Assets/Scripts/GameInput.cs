using System;
using UnityEngine;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteractAction;

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

    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.Player.Interact.performed -= Interact_performed;
            inputActions.Dispose();
        }
    }
}
