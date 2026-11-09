using System;
using DG.Tweening;
using Framework.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    [System.Serializable]
    public class TargetGraphic
    {
        public Graphic Graphic;
        public UIType Type;

        [NonSerialized] 
        public Tween Tween;
    }
    
    [RequireComponent(typeof(ButtonEvents))]
    public class UIComponentButton : MonoBehaviour
    {
        public TargetGraphic[] m_TargetGraphics;

        private ButtonEvents m_buttonEvents;
        
        private void Awake()
        {
            m_buttonEvents = GetComponent<ButtonEvents>();
            GetComponent<Button>().targetGraphic = null;
        }

        private void OnEnable()
        {
            m_buttonEvents.OnNormal += RefreshAnimated;
            m_buttonEvents.OnHighlighted += RefreshAnimated;
            m_buttonEvents.OnPressed += RefreshAnimated;
            m_buttonEvents.OnSelected += RefreshAnimated;
            m_buttonEvents.OnDisabled += RefreshAnimated;
            
            FrameworkEventsFactory.Subscribe(UIEvents.ManagerInitialized, OnManagerInitialized);
            FrameworkEventsFactory.Subscribe(UIEvents.OnThemeChanged, OnThemeChanged);
        }

        private void OnDisable()
        {
            m_buttonEvents.OnNormal -= RefreshAnimated;
            m_buttonEvents.OnHighlighted -= RefreshAnimated;
            m_buttonEvents.OnPressed -= RefreshAnimated;
            m_buttonEvents.OnSelected -= RefreshAnimated;
            m_buttonEvents.OnDisabled -= RefreshAnimated;
            
            FrameworkEventsFactory.Unsubscribe(UIEvents.ManagerInitialized, OnManagerInitialized);
            FrameworkEventsFactory.Unsubscribe(UIEvents.OnThemeChanged, OnThemeChanged);
        }
        
        private void OnThemeChanged()
        {
            Refresh(false);
        }
        
        private void OnManagerInitialized()
        {
            Refresh(true);
        }

        private void Refresh(bool instant)
        {
            switch (m_buttonEvents.CurrentState)
            {
                case ButtonState.Normal:
                    ApplyStateColor(block => block.normalColor, instant);
                    break;
                case ButtonState.Highlighted:
                    ApplyStateColor(block => block.highlightedColor, instant);
                    break;
                case ButtonState.Pressed:
                    ApplyStateColor(block => block.pressedColor, instant);
                    break;
                case ButtonState.Selected:
                    ApplyStateColor(block => block.selectedColor, instant);
                    break;
                case ButtonState.Disabled:
                    ApplyStateColor(block => block.disabledColor, instant);
                    break;
            }
        }

        private void RefreshAnimated()
        {
            Refresh(false);
        }
        
        private void ApplyStateColor(System.Func<ColorBlock, Color> colorSelector, bool instant)
        {
            foreach (var targetGraphic in m_TargetGraphics)
            {
                var graphic = targetGraphic.Graphic;
                var type = targetGraphic.Type;
        
                if (type is 
                    UIType.ButtonBackground or 
                    UIType.ButtonBorder or 
                    UIType.ButtonText or 
                    UIType.ButtonIcon)
                {
                    var colorBlock = UIManager.Instance.GetColorBlock(type);
                    var color = colorSelector(colorBlock);
                    UIManager.UpdateGraphic(graphic, color, type, ref targetGraphic.Tween, instant);
                }
            }
        }
    }
}