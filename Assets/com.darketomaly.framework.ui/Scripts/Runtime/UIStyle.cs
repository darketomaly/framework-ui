using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    [CreateAssetMenu(fileName = "UI Style", menuName = "Framework/UI Style")]
    public class UIStyle : ScriptableObject
    {
        [Header("Sprites")] 
        public Sprite m_Logo;
        
        [Header("Regular colors")]
        public Color m_LabelColor;
        public Color m_Header1Color;
        public Color m_Header2Color;
        public Color m_Header3Color;
        public Color m_PanelColor;
        public Color m_PanelBorderColor;
        public Color m_PanelSeparatorColor;
        
        [Header("Button color block")]
        public ColorBlock m_ButtonColorBlock;
        
        [Header("Button border color block")]
        public ColorBlock m_ButtonBorderColorBlock;
        
        [Header("Button text color block")]
        public ColorBlock m_ButtonTextColorBlock;
        
        [Header("Button icon color block")]
        public ColorBlock m_ButtonIconColorBlock;

        [Header("Animations")] 
        public float m_ButtonHoverScale = 1.0f;
        public float m_ButtonClickScale = 1.0f;
        
        [Space]
        public float m_ButtonHoverAnimationDuration = 0.5f;
        public float m_ButtonClickAnimationDuration = 0.25f;
        public float m_ButtonBackToNormalDuration = 0.5f;
        
        [Space]
        public float m_ColorAnimationDuration = 0.5f;
    }
}