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
    public class FrameworkButton : MonoBehaviour
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
            m_buttonEvents.OnNormal += OnNormal;
            m_buttonEvents.OnHighlighted += OnHighlighted;
            m_buttonEvents.OnDisabled += OnDisabled;
            
            FrameworkEventsFactory.Subscribe(UIEvents.ManagerInitialized, OnManagerInitialized);
            FrameworkEventsFactory.Subscribe(UIEvents.OnThemeChanged, OnThemeChanged);
        }

        private void OnDisable()
        {
            m_buttonEvents.OnNormal -= OnNormal;
            m_buttonEvents.OnHighlighted -= OnHighlighted;
            m_buttonEvents.OnDisabled -= OnDisabled;
            
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
            ApplyStateColor(block => block.normalColor, instant);
        }

        private void OnDisabled()
        {
            ApplyStateColor(block => block.disabledColor, false);
        }

        private void OnNormal()
        {
            ApplyStateColor(block => block.normalColor, false);
        }
        
        private void OnHighlighted()
        {
            ApplyStateColor(block => block.highlightedColor, false);
        }
        
        private void ApplyStateColor(System.Func<ColorBlock, Color> colorSelector, bool instant)
        {
            foreach (var targetGraphic in m_TargetGraphics)
            {
                var graphic = targetGraphic.Graphic;
                var type = targetGraphic.Type;
        
                if (type is UIType.ButtonBackground or UIType.ButtonBorder)
                {
                    var colorBlock = UIManager.Instance.GetColorBlock(type);
                    var color = colorSelector(colorBlock);
                    UIManager.UpdateGraphic(graphic, color, type, ref targetGraphic.Tween, instant);
                }
            }
        }
    }
}