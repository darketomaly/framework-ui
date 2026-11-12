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
        public Tween ColorTween;
    }
    
    [RequireComponent(typeof(ButtonEvents))]
    public class UIComponentButton : MonoBehaviour
    {
        public UISemantic m_Semantic;
        
        public TargetGraphic[] m_TargetGraphics;

        private ButtonEvents m_buttonEvents;
        private Button m_button;
        private Tween m_scaleTween;
        
        private void Awake()
        {
            m_buttonEvents = GetComponent<ButtonEvents>();
            
            m_button = GetComponent<Button>();
            m_button.targetGraphic = null;
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
            
            FrameworkEventsFactory.Unsubscribe(UIEvents.ManagerInitialized, OnThemeChanged);
            FrameworkEventsFactory.Unsubscribe(UIEvents.OnThemeChanged, OnThemeChanged);
        }
        
        private void OnThemeChanged()
        {
            RefreshColor(false);
        }
        
        private void OnManagerInitialized()
        {
            RefreshColor(true);
        }
        
        private void RefreshAnimated()
        {
            RefreshColor(false);
            ApplyScaleAnimation();
        }

        private void RefreshColor(bool instant)
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

        private void ApplyScaleAnimation()
        {
            var state = m_buttonEvents.CurrentState;
            var rt = m_button.transform as RectTransform;
                    
            UIManager.UpdateScale(rt, ref m_scaleTween, state, m_Semantic);
        }
        
        private void ApplyStateColor(System.Func<ColorBlock, Color> colorSelector, bool instant)
        {
            foreach (var targetGraphic in m_TargetGraphics)
            {
                var graphic = targetGraphic.Graphic;
                var type = targetGraphic.Type;
        
                if (type.IsColorBlock())
                {
                    var colorBlock = UIManager.Instance.GetColorBlock(type, m_Semantic);
                    var color = colorSelector(colorBlock);
                    UIManager.UpdateGraphicColor(graphic, color, ref targetGraphic.ColorTween, instant, m_Semantic);
                }
            }
        }
    }
}