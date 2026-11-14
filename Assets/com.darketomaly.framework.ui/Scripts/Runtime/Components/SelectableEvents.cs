using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Framework.UI
{
    public enum SelectableState
    {
        Normal,
        Highlighted,
        Pressed,
        Selected,
        Disabled
    }

    [RequireComponent(typeof(Selectable))]
    public class SelectableEvents : MonoBehaviour, 
        IPointerEnterHandler, IPointerExitHandler, 
        IPointerDownHandler, IPointerUpHandler, 
        ISelectHandler, IDeselectHandler
    {
        private Selectable targetSelectable;
        private bool wasInteractable;

        [Header("Debug")]
        [Tooltip("Check this box to print state changes to the Console.")]
        public bool showDebugLogs;

        public SelectableState CurrentState { get; private set; } = SelectableState.Normal;

        public event Action OnNormal;
        public event Action OnHighlighted;
        public event Action OnPressed;
        public event Action OnSelected;
        public event Action OnDisabled;
        public event Action OnEnabled;

        private void Awake()
        {
            targetSelectable = GetComponent<Selectable>();
            wasInteractable = targetSelectable.interactable;
            CurrentState = wasInteractable ? SelectableState.Normal : SelectableState.Disabled;
        }

        private void Update()
        {
            // Check if interactable state changed (Disabled / Enabled)
            if (targetSelectable.interactable != wasInteractable)
            {
                wasInteractable = targetSelectable.interactable;
                if (wasInteractable)
                {
                    if (showDebugLogs) this.Log($"[{name}] Selectable Enabled");
                    OnEnabled?.Invoke();
                    TriggerNormal();
                }
                else
                {
                    CurrentState = SelectableState.Disabled;
                    if (showDebugLogs) this.Log($"[{name}] Selectable Disabled");
                    OnDisabled?.Invoke();
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!targetSelectable.interactable) return;
            
            CurrentState = SelectableState.Highlighted;
            if (showDebugLogs) this.Log($"[{name}] Highlighted (Pointer Enter)");
            OnHighlighted?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!targetSelectable.interactable) return;

            if (EventSystem.current.currentSelectedGameObject != gameObject)
            {
                TriggerNormal();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!targetSelectable.interactable) return;
            
            CurrentState = SelectableState.Pressed;
            if (showDebugLogs) this.Log($"[{name}] Pressed (Pointer Down)");
            OnPressed?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!targetSelectable.interactable) return;

            if (RectTransformUtility.RectangleContainsScreenPoint(
                targetSelectable.transform as RectTransform, eventData.position, eventData.pressEventCamera))
            {
                CurrentState = SelectableState.Highlighted;
                if (showDebugLogs) this.Log($"[{name}] Highlighted (Pointer Up inside)");
                OnHighlighted?.Invoke();
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (!targetSelectable.interactable) return;
            
            CurrentState = SelectableState.Selected;
            if (showDebugLogs) this.Log($"[{name}] Selected");
            OnSelected?.Invoke();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (!targetSelectable.interactable) return;
            
            TriggerNormal();
        }

        private void TriggerNormal()
        {
            CurrentState = SelectableState.Normal;
            if (showDebugLogs) this.Log($"[{name}] Normal State");
            OnNormal?.Invoke();
        }
    }
}