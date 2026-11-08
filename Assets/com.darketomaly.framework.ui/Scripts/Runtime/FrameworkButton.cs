using System;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    [System.Serializable]
    public struct TargetGraphic
    {
        public Graphic Graphic;
        public UIType Type;
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
        }

        private void OnDisable()
        {
            m_buttonEvents.OnNormal -= OnNormal;
            m_buttonEvents.OnHighlighted -= OnHighlighted;
        }
        
        private void OnNormal()
        {
            foreach (var targetGraphic in m_TargetGraphics)
            {
                var graphic = targetGraphic.Graphic;
                var type = targetGraphic.Type;
                
                if (type == UIType.ButtonBackground)
                {
                    var colorBlock = UIManager.Instance.GetColorBlock(type);
                    graphic.color = colorBlock.normalColor;
                }
            }
        }
        
        private void OnHighlighted()
        {
            foreach (var targetGraphic in m_TargetGraphics)
            {
                var graphic = targetGraphic.Graphic;
                var type = targetGraphic.Type;
                
                if (type == UIType.ButtonBackground)
                {
                    var colorBlock = UIManager.Instance.GetColorBlock(type);
                    graphic.color = colorBlock.highlightedColor;
                }
            }
        }
    }
}