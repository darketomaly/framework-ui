using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    [System.Serializable]
    public class StyleSemanticEntry
    {
        public UISemantic Semantic;
        
        [Header("Sprites")] 
        public Overridable<Sprite> m_Logo = new();
        
        [Header("Regular colors")]
        public Overridable<Color> m_LabelColor = new();
        public Overridable<Color> m_Header1Color = new();
        public Overridable<Color> m_Header2Color = new();
        public Overridable<Color> m_Header3Color = new();
        public Overridable<Color> m_PanelColor = new();
        public Overridable<Color> m_PanelBorderColor = new();
        public Overridable<Color> m_PanelSeparatorColor = new();
        
        [Header("Animations")] 
        public Overridable<float> m_ButtonHoverScale = new();
        
        [Header("Text sizes")]
        public Overridable<float> m_LabelFontSize = new();
        public Overridable<float> m_Header1FontSize = new();
        public Overridable<float> m_Header2FontSize = new();
        public Overridable<float> m_Header3FontSize = new();
    }
    
    [System.Serializable]
    public class Overridable<T>
    {
        public bool IsOverriden;
        
        [SerializeField]
        private T Value;
        
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
        
        public List<StyleSemanticEntry> m_StyleSemantics = new();

        private StyleSemanticEntry GetSemanticEntry(UISemantic semantic)
        {
            // Always default to the default semantic
            StyleSemanticEntry semanticEntry = m_StyleSemantics[0];
            
            foreach (var entry in m_StyleSemantics)
            {
                if (entry.Semantic == semantic)
                {
                    semanticEntry = entry;
                }
            }

            return semanticEntry;
        }

        // ---
        
        // --- Sprites ---
        
        public Sprite GetLogo(UISemantic semantic)
        {
            return GetSemanticEntry(semantic).m_Logo.ResolveValue(m_ParentStyle?.GetLogo(semantic));
        }
        
        // --- Colors ---
        
        public Color GetLabelColor(UISemantic semantic)
        {
            return GetSemanticEntry(semantic).m_LabelColor.ResolveValue(m_ParentStyle?.GetLabelColor(semantic));
        }

        public Color GetHeader1Color(UISemantic semantic)
        {
            return GetSemanticEntry(semantic).m_Header1Color.ResolveValue(m_ParentStyle?.GetHeader1Color(semantic));
        }

        public Color GetHeader2Color(UISemantic semantic)
        {
            return GetSemanticEntry(semantic).m_Header2Color.ResolveValue(m_ParentStyle?.GetHeader2Color(semantic));
        }

        public Color GetHeader3Color(UISemantic semantic)
        {
            return GetSemanticEntry(semantic).m_Header3Color.ResolveValue(m_ParentStyle?.GetHeader3Color(semantic));
        }

        public Color GetPanelColor(UISemantic semantic)
        {
            return GetSemanticEntry(semantic).m_PanelColor.ResolveValue(m_ParentStyle?.GetPanelColor(semantic));
        }

        public Color GetPanelBorderColor(UISemantic semantic)
        {
            return GetSemanticEntry(semantic).m_PanelBorderColor.ResolveValue(m_ParentStyle?.GetPanelBorderColor(semantic));
        }

        public Color GetPanelSeparatorColor(UISemantic semantic)
        {
            return GetSemanticEntry(semantic).m_PanelSeparatorColor.ResolveValue(m_ParentStyle?.GetPanelSeparatorColor(semantic));
        }
        
        [Header("Color blocks")]
        [SerializeField] private Overridable<ColorBlock> m_ButtonColorBlock = new();
        public ColorBlock ButtonColorBlock => m_ButtonColorBlock.ResolveValue(m_ParentStyle?.m_ButtonColorBlock);
        
        [SerializeField] private Overridable<ColorBlock> m_ButtonBorderColorBlock = new();
        public ColorBlock ButtonBorderColorBlock => m_ButtonBorderColorBlock.ResolveValue(m_ParentStyle?.m_ButtonBorderColorBlock);

        [SerializeField] private Overridable<ColorBlock> m_ButtonTextColorBlock = new();
        public ColorBlock ButtonTextColorBlock => m_ButtonTextColorBlock.ResolveValue(m_ParentStyle?.m_ButtonTextColorBlock);

        [SerializeField] private Overridable<ColorBlock> m_ButtonIconColorBlock = new();
        public ColorBlock ButtonIconColorBlock => m_ButtonIconColorBlock.ResolveValue(m_ParentStyle?.m_ButtonIconColorBlock);

        // --- Animations ---

        public float GetButtonHoverScale(UISemantic semantic)
        {
            return GetSemanticEntry(semantic).m_ButtonHoverScale.ResolveValue(m_ParentStyle?.GetButtonHoverScale(semantic));
        }
        
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
        
        // --- Text sizes ---

        public float GetLabelFontSize(UISemantic semantic)
        {
            return GetSemanticEntry(semantic).m_LabelFontSize.ResolveValue(m_ParentStyle?.GetLabelFontSize(semantic));
        }

        public float GetHeader1FontSize(UISemantic semantic)
        {
            return GetSemanticEntry(semantic).m_Header1FontSize.ResolveValue(m_ParentStyle?.GetHeader1FontSize(semantic));
        }

        public float GetHeader2FontSize(UISemantic semantic)
        {
            return GetSemanticEntry(semantic).m_Header2FontSize.ResolveValue(m_ParentStyle?.GetHeader2FontSize(semantic));
        }
        
        public float GetHeader3FontSize(UISemantic semantic)
        {
            return GetSemanticEntry(semantic).m_Header3FontSize.ResolveValue(m_ParentStyle?.GetHeader3FontSize(semantic));
        }
    }
}