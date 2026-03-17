using System;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public InventoryUI Instance { get; private set; }

    [SerializeField] private GameInput gameInput;
    [SerializeField] private Fader fader;

    [SerializeField] GameObject inventoryUIVisual;

    public event EventHandler OnInventoryOpened;
    public event EventHandler OnInventoryClosed;

    private bool isOpen;
    private bool isAnimating;

    private void Start()
    {
        if(Instance == null)
            Instance = this;

        Hide();
        isOpen = false;
        isAnimating = false;
    }

    private void Awake()
    {
        gameInput.OnInventoryOpenCloseAction += GameInput_OnInventoryOpenCloseAction;
    }

    private void GameInput_OnInventoryOpenCloseAction(object sender, System.EventArgs e)
    {
        if (isAnimating || fader == null) return;

        isOpen = !isOpen;
        if (isOpen)
        {
            Debug.Log("Inventory opened");

            OnInventoryOpened?.Invoke(this, EventArgs.Empty);
            StartCoroutine(ShowInventory());
        }
        else
        {
            Debug.Log("Inventory closed");

            OnInventoryClosed?.Invoke(this, EventArgs.Empty);
            StartCoroutine(HideInventory());
        }
    }

    private System.Collections.IEnumerator ShowInventory()
    {
        isAnimating = true;
        if (fader != null)
        {
            yield return StartCoroutine(fader.FadeIn());
        }

        Show();

        if (fader != null)
        {
            yield return StartCoroutine(fader.FadeOut());
        }
        isAnimating = false;
    }

    private System.Collections.IEnumerator HideInventory()
    {
        isAnimating = true;
        if (fader != null)
        {
            yield return StartCoroutine(fader.FadeIn());
        }

        Hide();

        if (fader != null)
        {
            yield return StartCoroutine(fader.FadeOut());
        }
        isAnimating = false;
    }

    private void Show()
    {
        if (inventoryUIVisual != null)
        {
            inventoryUIVisual.SetActive(true);
        }
    }

    private void Hide()
    {
        if (inventoryUIVisual != null)
        {
            inventoryUIVisual.SetActive(false);
        }
    }
}
