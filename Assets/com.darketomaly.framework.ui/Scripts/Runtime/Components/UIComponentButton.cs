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
    }
    
    [RequireComponent(typeof(SelectableEvents))]
    public class UIComponentButton : MonoBehaviour
    {
        public UISemantic m_Semantic;
        
        public TargetGraphic[] m_TargetGraphics;

        private SelectableEvents m_SelectableEvents;
        private Button m_button;
        
        private void Awake()
        {
            m_SelectableEvents = GetComponent<SelectableEvents>();
            
            m_button = GetComponent<Button>();
            m_button.targetGraphic = null;
        }

        private void OnEnable()
        {
            m_SelectableEvents.OnNormal += RefreshAnimated;
            m_SelectableEvents.OnHighlighted += RefreshAnimated;
            m_SelectableEvents.OnPressed += RefreshAnimated;
            m_SelectableEvents.OnSelected += RefreshAnimated;
            m_SelectableEvents.OnDisabled += RefreshAnimated;
            
            FrameworkEventsFactory.Subscribe(UIEvents.ManagerInitialized, OnManagerInitialized);
            FrameworkEventsFactory.Subscribe(UIEvents.OnThemeChanged, OnThemeChanged);
        }

        private void OnDisable()
        {
            m_SelectableEvents.OnNormal -= RefreshAnimated;
            m_SelectableEvents.OnHighlighted -= RefreshAnimated;
            m_SelectableEvents.OnPressed -= RefreshAnimated;
            m_SelectableEvents.OnSelected -= RefreshAnimated;
            m_SelectableEvents.OnDisabled -= RefreshAnimated;
            
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
            switch (m_SelectableEvents.CurrentState)
            {
                case SelectableState.Normal:
                    ApplyStateColor(block => block.normalColor, instant);
                    break;
                case SelectableState.Highlighted:
                    ApplyStateColor(block => block.highlightedColor, instant);
                    break;
                case SelectableState.Pressed:
                    ApplyStateColor(block => block.pressedColor, instant);
                    break;
                case SelectableState.Selected:
                    ApplyStateColor(block => block.selectedColor, instant);
                    break;
                case SelectableState.Disabled:
                    ApplyStateColor(block => block.disabledColor, instant);
                    break;
            }
        }

        private void ApplyScaleAnimation()
        {
            var state = m_SelectableEvents.CurrentState;
            var rt = m_button.transform as RectTransform;
                    
            UIManager.UpdateScale(rt, state, m_Semantic);
        }
        
        private void ApplyStateColor(System.Func<ColorBlock, Color> colorSelector, bool instant)
        {
            foreach (var targetGraphic in m_TargetGraphics)
            {
                var graphic = targetGraphic.Graphic;
                var type = targetGraphic.Type;
        
                if (type.IsColorBlock())
                {
                    var colorBlock = UIManager.GetColorBlock(type, m_Semantic);
                    var color = colorSelector(colorBlock);
                    UIManager.UpdateGraphicColor(graphic, color, instant, m_Semantic);
                }
            }
        }
        
        #if UNITY_EDITOR

        private void OnValidate()
        {
            if (!Application.isPlaying && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                ApplyStateColor(block => block.normalColor, true);
            }
        }
        
        #endif
    }
}