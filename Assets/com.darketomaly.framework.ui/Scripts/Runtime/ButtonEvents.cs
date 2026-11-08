using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonEvents : MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler, 
    IPointerDownHandler, IPointerUpHandler, 
    ISelectHandler, IDeselectHandler
{
    private Button targetButton;
    private bool wasInteractable;

    [Header("Debug")]
    [Tooltip("Check this box to print state changes to the Console.")]
    public bool showDebugLogs = true;

    // C# Actions for code subscription
    public event Action OnNormal;
    public event Action OnHighlighted;
    public event Action OnPressed;
    public event Action OnSelected;
    public event Action OnDisabled;
    public event Action OnEnabled;

    private void Awake()
    {
        targetButton = GetComponent<Button>();
        wasInteractable = targetButton.interactable;
    }

    private void Update()
    {
        // Check if interactable state changed (Disabled / Enabled)
        if (targetButton.interactable != wasInteractable)
        {
            wasInteractable = targetButton.interactable;
            if (wasInteractable)
            {
                if (showDebugLogs) Debug.Log($"[{gameObject.name}] Button Enabled", gameObject);
                OnEnabled?.Invoke();
                TriggerNormal();
            }
            else
            {
                if (showDebugLogs) Debug.Log($"[{gameObject.name}] Button Disabled", gameObject);
                OnDisabled?.Invoke();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!targetButton.interactable) return;
        
        if (showDebugLogs) Debug.Log($"[{gameObject.name}] Highlighted (Pointer Enter)", gameObject);
        OnHighlighted?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!targetButton.interactable) return;

        if (EventSystem.current.currentSelectedGameObject != gameObject)
        {
            TriggerNormal();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!targetButton.interactable) return;
        
        if (showDebugLogs) Debug.Log($"[{gameObject.name}] Pressed (Pointer Down)", gameObject);
        OnPressed?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!targetButton.interactable) return;

        if (RectTransformUtility.RectangleContainsScreenPoint(
            targetButton.transform as RectTransform, eventData.position, eventData.pressEventCamera))
        {
            if (showDebugLogs) Debug.Log($"[{gameObject.name}] Highlighted (Pointer Up inside)", gameObject);
            OnHighlighted?.Invoke();
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!targetButton.interactable) return;
        
        if (showDebugLogs) Debug.Log($"[{gameObject.name}] Selected", gameObject);
        OnSelected?.Invoke();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (!targetButton.interactable) return;
        
        if (showDebugLogs) Debug.Log($"[{gameObject.name}] Deselected", gameObject);
        TriggerNormal();
    }

    private void TriggerNormal()
    {
        if (showDebugLogs) Debug.Log($"[{gameObject.name}] Normal State", gameObject);
        OnNormal?.Invoke();
    }
}