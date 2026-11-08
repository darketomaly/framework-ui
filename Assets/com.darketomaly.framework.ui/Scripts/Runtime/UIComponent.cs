using System;
using Framework.Events;
using TMPro;
using UnityEngine;

#if DOTWEEN
using DG.Tweening;
#endif

namespace Framework.UI
{
    public class UIComponent : MonoBehaviour
    {
        [SerializeField]
        private UIType m_Type;

        #if DOTWEEN
        private Tween m_tween;
        #endif

        private void OnEnable()
        {
            FrameworkEventsFactory.Subscribe(UIEvents.ManagerInitialized, OnManagerInitialized);
            FrameworkEventsFactory.Subscribe(UIEvents.OnThemeChanged, OnThemeChanged);
        }

        private void OnDisable()
        {
            FrameworkEventsFactory.Unsubscribe(UIEvents.ManagerInitialized, OnManagerInitialized);
            FrameworkEventsFactory.Unsubscribe(UIEvents.OnThemeChanged, OnThemeChanged);
        }
        
        private void OnManagerInitialized()
        {
            Initialize();
        }
        
        private void OnThemeChanged()
        {
            Initialize();
        }

        private void Initialize()
        {
            var color = UIManager.Instance.GetColor(m_Type);
            var txt = GetComponent<TextMeshProUGUI>();

            #if DOTWEEN

            m_tween?.Kill();

            m_tween = DOTween.To(
                () => txt.color,    
                x => txt.color = x,   
                color,                 
                0.5f                        
            );
            
            #endif
        }
    }
}