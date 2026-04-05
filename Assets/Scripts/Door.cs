using System;
using UnityEngine;

public class Door : MonoBehaviour
{
    public enum DoorState
    {
        Unlocked,
        NeedKeyCard,
        Locked,
        StartState
    }

    public enum DoorName
    {
        Default,

        FromStartToCoridor,
        FromCoridorToStart,

        FromCoridorToPantry,
        FromPantryToCoridor
    }

    [SerializeField] private Loader.Scene nextScene;

    [SerializeField] private DoorState doorState;
    [SerializeField] private DoorName nextDoorName = DoorName.FromStartToCoridor;

    [SerializeField] private float openAnimationTime = 1f;
    [SerializeField] private float openDoorUIMessageTime = 3f;

    [SerializeField] private float showOpenDoorBtnTime = 0.5f;

    [SerializeField] private ItemSO keyCardSO;

    [SerializeField] private GameObject interactBtnUIInteracted;
    [SerializeField] private GameObject openDoorBtnUIInteracted;

    [SerializeField] private GameObject doorUIMessage;

    [SerializeField] private Fader fader;

    [SerializeField] private Transform playerPoint;

    private Collider doorCollider;

    public bool IsOpened { get; private set; }
    public bool IsAnimate { get; private set; } 

    public DoorName NextDoorName { get { return nextDoorName; } private set { nextDoorName = value; } }
    public DoorState State { get; private set; }
    private static DoorState state = DoorState.StartState;

    public event EventHandler OnOpen;
    public event EventHandler OnOpenAnimationComplete;

    public event EventHandler OnOpenDoorBtnShowCompleted;

    public static event EventHandler<OnPlayerPointEventArgs> OnPlayerPoint;
    public class OnPlayerPointEventArgs : EventArgs
    {
        public Transform playerPointTransform;
    }

    private void Awake()
    {
        doorCollider = GetComponent<Collider>();

        if(state == DoorState.StartState)
        {
            state = doorState;
            State = state;
        }
        else
        {
            State = state;
            doorState = state;
        }

        NextDoorName = nextDoorName;

        if (interactBtnUIInteracted != null)
        {
            interactBtnUIInteracted.SetActive(false);
        }
        if (openDoorBtnUIInteracted != null)
        {
            openDoorBtnUIInteracted.SetActive(false);
        }

        if (doorUIMessage != null)
        {
            doorUIMessage.SetActive(false);
        }
        else doorUIMessage = null;

        if (nextDoorName == Loader.GetPlayerPointDoor())
        {
            OnPlayerPoint?.Invoke(this, new OnPlayerPointEventArgs
            {
                playerPointTransform = playerPoint
            });
        }
    }

    public void Interact(bool hasKeyCard = false)
    {
        if (IsOpened || IsAnimate)
        {
            return;
        }

        switch (State)
        {
            case DoorState.Unlocked:
                if (interactBtnUIInteracted != null)
                {
                    interactBtnUIInteracted.SetActive(true);
                }

                OpenDoor();
                break;

            case DoorState.NeedKeyCard:
                if (interactBtnUIInteracted != null && hasKeyCard)
                {
                    openDoorBtnUIInteracted.SetActive(true);
                }

                TryUnlockDoor(hasKeyCard);
                break;

            case DoorState.Locked:
                break;
        }
    }

    private void OpenDoor()
    {
        IsAnimate = true;

        if (doorCollider != null)
            doorCollider.enabled = false;

        IsOpened = true;
        OnOpen?.Invoke(this, EventArgs.Empty);

        StartCoroutine(OpenAnimationCoroutine());
    }

    private void TryUnlockDoor(bool hasKeyCard)
    {
        if (hasKeyCard)
        {
            StartCoroutine(ShowObjectCoroutine(openDoorBtnUIInteracted, showOpenDoorBtnTime));
            SetDoorState(DoorState.Unlocked);
        }
        else
        {
            StartCoroutine(ShowObjectCoroutine(doorUIMessage, openDoorUIMessageTime));
        }
    }

    private System.Collections.IEnumerator ShowObjectCoroutine(GameObject messageObject, float time)
    {
        if (messageObject == null) yield break;

        messageObject.SetActive(true);
        yield return new WaitForSeconds(time);

        if (messageObject != null)
        {
            messageObject.SetActive(false);
            OnOpenDoorBtnShowCompleted?.Invoke(this, EventArgs.Empty);
        }
    }

    private System.Collections.IEnumerator OpenAnimationCoroutine()
    {
        yield return new WaitForSeconds(openAnimationTime);
        OnOpenAnimationComplete?.Invoke(this, EventArgs.Empty);

        if(fader != null)
        {
            yield return StartCoroutine(fader.FadeIn());
        }

        state = State;

        Loader.Load(nextScene);
    }

    public Loader.Scene GetScene()
    {
        return nextScene;
    }

    public void SetDoorState(DoorState newState)
    {
        state = newState;
        State = state;
        doorState = state;
    }
}