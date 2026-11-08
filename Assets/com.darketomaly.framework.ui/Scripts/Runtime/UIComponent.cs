using Framework.Events;
using UnityEngine;
using UnityEngine.UI;

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
            var color = UIManager.Instance.GetColor(m_Type);
            UIManager.UpdateGraphic(GetComponent<Graphic>(), color, m_Type, ref m_tween, false);
        }

        private void Initialize()
        {
            var color = UIManager.Instance.GetColor(m_Type);
            UIManager.UpdateGraphic(GetComponent<Graphic>(), color, m_Type, ref m_tween, true);
        }
    }
}