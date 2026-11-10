using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    [System.Serializable]
    public class Overridable<T>
    {
        public bool IsOverriden;
        public T Value;
        
        // Implicit conversion from Overridable<T> to T
        public static implicit operator T(Overridable<T> overridable) => overridable.Value;

        // Implicit conversion from T to Overridable<T>
        public static implicit operator Overridable<T>(T value) => new()
        { 
            Value = value, 
            IsOverriden = true 
        };
    }
    
    [CreateAssetMenu(fileName = "UI Style", menuName = "Framework/UI Style")]
    public class UIStyle : ScriptableObject
    {
        [Header("Sprites")] 
        public Overridable<Sprite> m_Logo;
        
        [Header("Regular colors")]
        public Overridable<Color> m_LabelColor;
        public Overridable<Color> m_Header1Color;
        public Overridable<Color> m_Header2Color;
        public Overridable<Color> m_Header3Color;
        public Overridable<Color> m_PanelColor;
        public Overridable<Color> m_PanelBorderColor;
        public Overridable<Color> m_PanelSeparatorColor;
        
        [Header("Button color block")]
        public Overridable<ColorBlock> m_ButtonColorBlock;
        
        [Header("Button border color block")]
        public Overridable<ColorBlock> m_ButtonBorderColorBlock;
        
        [Header("Button text color block")]
        public Overridable<ColorBlock> m_ButtonTextColorBlock;
        
        [Header("Button icon color block")]
        public Overridable<ColorBlock> m_ButtonIconColorBlock;

        [Header("Animations")] 
        public Overridable<float> m_ButtonHoverScale;
        public Overridable<float> m_ButtonClickScale;

        [Space] 
        public Overridable<float> m_ButtonHoverAnimationDuration;
        public Overridable<float> m_ButtonClickAnimationDuration;
        public Overridable<float> m_ButtonBackToNormalDuration;

        [Space] 
        public Overridable<float> m_ColorAnimationDuration;
    }
}