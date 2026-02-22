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

    [SerializeField] private GameInput gameInput;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float sprintSpeedCoefficient = 1.5f;

    public bool IsWalking { get; private set; }

    private void Update()
    {
        HandleMovement();
    }

    public void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();
        Vector3 moveDir = new Vector3(inputVector.x, 0, inputVector.y);

        float playerRadius = 0.5f;
        float playerHeight = 2f;

        float moveDist = speed * Time.deltaTime;

        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight,
            playerRadius, moveDir, moveDist);

        //IsWalking = moveDir != Vector3.zero;

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
            /*
             * isSprinting
             */

            transform.position += speed * moveDir * Time.deltaTime;
        }

        transform.forward = Vector3.Slerp(transform.forward, moveDir, rotationSpeed * Time.deltaTime);
    }
}
