using System;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    [SerializeField] private int nextSceneIndex;

    //[SerializeField] private GameObject leftDoor;
    //[SerializeField] private GameObject rightDoor;

    //private Collider leftDoorCollider;
    //private Collider rightDoorCollider;

    private Collider doorCollider;

    public bool IsOpened {  get; private set; }

    public event EventHandler OnOpen;
    //public event EventHandler OnClosed;

    private void Awake()
    {
        if(this != null)
            doorCollider = this.GetComponent<Collider>();
    }

    public void Interact(Player player)
    {
        if (IsOpened)
        {
            Debug.Log("Door Opened");
            return;
        }

        Debug.Log("Interact Door");

        if(doorCollider != null)
            doorCollider.enabled = false;

        IsOpened = true;
        OnOpen?.Invoke(this, EventArgs.Empty);
    }

}
