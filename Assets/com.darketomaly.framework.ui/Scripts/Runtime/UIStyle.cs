using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    [CreateAssetMenu(fileName = "UI Style", menuName = "Framework/UI Style")]
    public class UIStyle : ScriptableObject
    {
        [Header("Regular colors")]
        public Color m_LabelColor;
        
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
    }
}