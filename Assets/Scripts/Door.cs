using System;
using UnityEngine;

public class Door : MonoBehaviour
{
    public enum DoorState
    {
        Unlocked,
        NeedKeyCard,
        Locked
    }

    [SerializeField] private Loader.Scene nextScene;
    [SerializeField] private DoorState state = DoorState.Unlocked;

    [SerializeField] private float openAnimationTime = 1f;

    [SerializeField] private KeyCardSO keyCardSO;

    [SerializeField] private GameObject interactBtnUIInteracted;

    [SerializeField] private Fader fader;

    private Collider doorCollider;

    public bool IsOpened { get; private set; }
    public bool IsAnimate { get; private set; } 

    public event EventHandler OnOpen;
    public event EventHandler OnOpenAnimationComplete; 

    private void Awake()
    {
        doorCollider = GetComponent<Collider>();

        if (interactBtnUIInteracted != null)
        {
            interactBtnUIInteracted.SetActive(false);
        }
    }

    public void Interact(Player player)
    {
        if(interactBtnUIInteracted != null)
        {
            interactBtnUIInteracted.SetActive(true);
        }

        if (IsOpened || IsAnimate)
        {
            return;
        }

        switch (state)
        {
            case DoorState.Unlocked:
                OpenDoor(player);
                break;

            case DoorState.NeedKeyCard:
                break;

            case DoorState.Locked:
                break;
        }
    }

    private void OpenDoor(Player player)
    {
        IsAnimate = true;

        if (doorCollider != null)
            doorCollider.enabled = false;

        IsOpened = true;
        OnOpen?.Invoke(this, EventArgs.Empty);

        StartCoroutine(OpenAnimationCoroutine());
    }

    private System.Collections.IEnumerator OpenAnimationCoroutine()
    {
        yield return new WaitForSeconds(openAnimationTime);
        OnOpenAnimationComplete?.Invoke(this, EventArgs.Empty);

        if(fader != null)
        {
            yield return StartCoroutine(fader.FadeIn());
        }

        Loader.Load(nextScene);
    }

    public Loader.Scene GetScene()
    {
        return nextScene;
    }

    public void SetDoorState(DoorState state)
    {
        this.state = state;
    }
}