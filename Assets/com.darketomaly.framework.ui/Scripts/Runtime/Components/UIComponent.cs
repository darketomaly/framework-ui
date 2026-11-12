using Framework.Events;
using TMPro;
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

        [SerializeField] 
        private UISemantic m_Semantic;

        #if DOTWEEN
        private Tween m_tween;
        #endif

        private Graphic m_graphic;
        private TextMeshProUGUI m_text;
        
        private Color GetColor => UIManager.GetColor(m_Type, m_Semantic);

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
        
        private void Initialize()
        {
            m_graphic = GetComponent<Graphic>();
            m_text = GetComponent<TextMeshProUGUI>();

            if (m_Type.IsSprite())
            {
                var img = m_graphic as Image;
                img.sprite = UIManager.Instance.GetSprite(m_Type, m_Semantic);
            }
            
            if (m_Type.IsColor())
            {
                UIManager.UpdateGraphicColor(m_graphic, GetColor, ref m_tween, true, m_Semantic);
            }

            if (m_Type.IsText())
            {
                UIManager.UpdateTextSize(m_text, m_Type, m_Semantic);
            }
        }
        
        private void OnThemeChanged()
        {
            if (m_Type.IsSprite())
            {
                var img = m_graphic as Image;
                img.sprite = UIManager.Instance.GetSprite(m_Type, m_Semantic);
            }
            
            if (m_Type.IsColor())
            {
                UIManager.UpdateGraphicColor(m_graphic, GetColor, ref m_tween, false, m_Semantic);
            }
            
            if (m_Type.IsText())
            {
                UIManager.UpdateTextSize(m_text, m_Type, m_Semantic);
            }
        }
        
        #if UNITY_EDITOR

        private void OnValidate()
        {
            Refresh();
        }

        [DarkButton("Refresh")]
        private void Refresh()
        {
            if (!Application.isPlaying)
            {
                var graphic = GetComponent<Graphic>();
                
                if (m_Type.IsSprite())
                {
                    var img = graphic as Image;
                    UIManager.UpdateSprite(img, m_Type, m_Semantic);
                }
                
                if (m_Type.IsColor())
                {
                    UIManager.UpdateGraphicColor(graphic, GetColor, ref m_tween, true, m_Semantic);
                }
            
                if (m_Type.IsText())
                {
                    var txt = GetComponent<TextMeshProUGUI>();
                    UIManager.UpdateTextSize(txt, m_Type, m_Semantic);
                }
                
                graphic.SetAllDirty();
            }
        }
        
        #endif
    }
}