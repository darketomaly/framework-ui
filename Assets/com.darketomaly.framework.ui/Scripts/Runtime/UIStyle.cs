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
        public static implicit operator T(Overridable<T> overridable)
        {
            return overridable.Value;
        }

        // Implicit conversion from T to Overridable<T>
        public static implicit operator Overridable<T>(T value) => value is null ? null : new()
        { 
            Value = value, 
            IsOverriden = true 
        };

        public T ResolveValue(Overridable<T> parent)
        {
            if (IsOverriden || parent == null)
            {
                return Value;
            }

            return parent.Value;
        }
    }
    
    [CreateAssetMenu(fileName = "UI Style", menuName = "Framework/UI Style")]
    public class UIStyle : ScriptableObject
    {
        [SerializeField] 
        private UIStyle m_ParentStyle;
        
        [Header("Sprites")] 
        [SerializeField] private Overridable<Sprite> m_Logo = new();
        public Sprite Logo => m_Logo.ResolveValue(m_ParentStyle?.m_Logo);
        
        [Header("Regular colors")]
        [SerializeField] private Overridable<Color> m_LabelColor = new();
        public Color LabelColor => m_LabelColor.ResolveValue(m_ParentStyle?.m_LabelColor);
        
        [SerializeField] private Overridable<Color> m_Header1Color = new();
        public Color Header1Color => m_Header1Color.ResolveValue(m_ParentStyle?.m_Header1Color);
        
        [SerializeField] private Overridable<Color> m_Header2Color = new();
        public Color Header2Color => m_Header2Color.ResolveValue(m_ParentStyle?.m_Header2Color);
        
        [SerializeField] private Overridable<Color> m_Header3Color = new();
        public Color Header3Color => m_Header3Color.ResolveValue(m_ParentStyle?.m_Header3Color);
        
        [SerializeField] private Overridable<Color> m_PanelColor = new();
        public Color PanelColor => m_PanelColor.ResolveValue(m_ParentStyle?.m_PanelColor);
        
        [SerializeField] private Overridable<Color> m_PanelBorderColor = new();
        public Color PanelBorderColor => m_PanelBorderColor.ResolveValue(m_ParentStyle?.m_PanelBorderColor);
        
        [SerializeField] private Overridable<Color> m_PanelSeparatorColor = new();
        public Color PanelSeparatorColor => m_PanelSeparatorColor.ResolveValue(m_ParentStyle?.m_PanelSeparatorColor);
        
        [Header("Color blocks")]
        [SerializeField] private Overridable<ColorBlock> m_ButtonColorBlock = new();
        public ColorBlock ButtonColorBlock => m_ButtonColorBlock.ResolveValue(m_ParentStyle?.m_ButtonColorBlock);
        
        [SerializeField] private Overridable<ColorBlock> m_ButtonBorderColorBlock = new();
        public ColorBlock ButtonBorderColorBlock => m_ButtonBorderColorBlock.ResolveValue(m_ParentStyle?.m_ButtonBorderColorBlock);

        [SerializeField] private Overridable<ColorBlock> m_ButtonTextColorBlock = new();
        public ColorBlock ButtonTextColorBlock => m_ButtonTextColorBlock.ResolveValue(m_ParentStyle?.m_ButtonTextColorBlock);

        [SerializeField] private Overridable<ColorBlock> m_ButtonIconColorBlock = new();
        public ColorBlock ButtonIconColorBlock => m_ButtonIconColorBlock.ResolveValue(m_ParentStyle?.m_ButtonIconColorBlock);

        [Header("Animations")] 
        [SerializeField] private Overridable<float> m_ButtonHoverScale = new();
        public float ButtonHoverScale => m_ButtonHoverScale.ResolveValue(m_ParentStyle?.m_ButtonHoverScale);
        
        [SerializeField] private Overridable<float> m_ButtonClickScale = new();
        public float ButtonClickScale => m_ButtonClickScale.ResolveValue(m_ParentStyle?.m_ButtonClickScale);

        [Space] 
        [SerializeField] private Overridable<float> m_ButtonHoverAnimationDuration = new();
        public float ButtonHoverAnimationDuration => m_ButtonHoverAnimationDuration.ResolveValue(m_ParentStyle?.m_ButtonHoverAnimationDuration);
        
        [SerializeField] private Overridable<float> m_ButtonClickAnimationDuration = new();
        public float ButtonClickAnimationDuration => m_ButtonClickAnimationDuration.ResolveValue(m_ParentStyle?.m_ButtonClickAnimationDuration);
        
        [SerializeField] private Overridable<float> m_ButtonBackToNormalDuration = new();
        public float ButtonBackToNormalDuration => m_ButtonBackToNormalDuration.ResolveValue(m_ParentStyle?.m_ButtonBackToNormalDuration);

        [Space]
        [SerializeField] private Overridable<float> m_ColorAnimationDuration = new();
        public float ColorAnimationDuration => m_ColorAnimationDuration.ResolveValue(m_ParentStyle?.m_ColorAnimationDuration);
    }
}