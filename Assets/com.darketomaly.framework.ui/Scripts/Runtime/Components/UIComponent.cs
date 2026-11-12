using Framework.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    public class UIComponent : MonoBehaviour
    {
        [SerializeField]
        private UIType m_Type;

        [SerializeField] 
        private UISemantic m_Semantic;

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
                img.sprite = UIManager.GetSprite(m_Type, m_Semantic);
            }
            
            if (m_Type.IsColor())
            {
                UIManager.UpdateGraphicColor(m_graphic, GetColor, true, m_Semantic);
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
                img.sprite = UIManager.GetSprite(m_Type, m_Semantic);
            }
            
            if (m_Type.IsColor())
            {
                UIManager.UpdateGraphicColor(m_graphic, GetColor , false, m_Semantic);
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
            if (!Application.isPlaying && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                var graphic = GetComponent<Graphic>();
                
                Initialize();
                graphic.SetAllDirty();
            }
        }
        
        #endif
    }
}