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
        
        [Header("Color blocks")]
        public Overridable<ColorBlock> m_ButtonColorBlock = new();
        public Overridable<ColorBlock> m_ButtonBorderColorBlock = new();
        public Overridable<ColorBlock> m_ButtonTextColorBlock = new();
        public Overridable<ColorBlock> m_ButtonIconColorBlock = new();
        
        [Header("Animations")] 
        public Overridable<float> m_ButtonHoverScale = new();
        public Overridable<float> m_ButtonClickScale = new();
        public Overridable<float> m_ButtonHoverAnimationDuration = new();
        public Overridable<float> m_ButtonClickAnimationDuration = new();
        public Overridable<float> m_ButtonBackToNormalDuration = new();
        public Overridable<float> m_ColorAnimationDuration = new();
        
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
    }
    
    [CreateAssetMenu(fileName = "UI Style", menuName = "Framework/UI Style")]
    public class UIStyle : ScriptableObject
    {
        // --- Data storage (Parent style, semantics and its style values) ---
        
        [SerializeField] 
        private UIStyle m_ParentStyle;
        
        public List<StyleSemanticEntry> m_StyleSemantics = new();

        private StyleSemanticEntry GetSemanticEntry(UISemantic semantic)
        {
            if (m_StyleSemantics == null || m_StyleSemantics.Count == 0)
            {
                this.LogError("Style has no semantics, this should not happen. It should always have at least the default semantic");
                return null;
            }
            
            // Always default to the default semantic
            StyleSemanticEntry semanticEntry = m_StyleSemantics[0];
            
            foreach (var entry in m_StyleSemantics)
            {
                if (entry.Semantic == semantic)
                {
                    semanticEntry = entry;
                    break;
                }
            }

            return semanticEntry;
        }
        
        private T Resolve<T>(UISemantic semantic, System.Func<StyleSemanticEntry, Overridable<T>> selector)
        {
            var entry = GetSemanticEntry(semantic);
            
            if (entry == null)
            {
                this.LogError("Semantic entry not found. This should not happen");
                return default;
            }
            
            var field = selector(entry);
            
            if (field == null)
            {
                this.LogError("Overridable field not found. This should not happen");
                return default;
            }
            
            // If there's no override on the semantic, grab the default semantic
            // The default semantic may or may not have an override itself for the style

            if (entry != m_StyleSemantics[0] && !field.IsOverriden)
            {
                return Resolve(m_StyleSemantics[0].Semantic, selector);
            }

            if (field.IsOverriden || m_ParentStyle == null)
            {
                return field;
            }

            return m_ParentStyle.Resolve(semantic, selector);
        }

        // --- Sprites ---

        public Sprite GetLogo(UISemantic semantic) => Resolve(semantic, e => e.m_Logo);
        
        // --- Colors ---
        
        public Color GetLabelColor(UISemantic semantic) => Resolve(semantic, e => e.m_LabelColor);

        public Color GetHeader1Color(UISemantic semantic) => Resolve(semantic, e=> e.m_Header1Color);

        public Color GetHeader2Color(UISemantic semantic) => Resolve(semantic, e => e.m_Header2Color);

        public Color GetHeader3Color(UISemantic semantic) => Resolve(semantic, e => e.m_Header3Color);

        public Color GetPanelColor(UISemantic semantic) => Resolve(semantic, e => e.m_PanelColor);

        public Color GetPanelBorderColor(UISemantic semantic) => Resolve(semantic, e => e.m_PanelBorderColor);

        public Color GetPanelSeparatorColor(UISemantic semantic) => Resolve(semantic, e => e.m_PanelSeparatorColor);
        
        // --- Color blocks ---

        public ColorBlock GetButtonColorBlock(UISemantic semantic) => Resolve(semantic, e => e.m_ButtonColorBlock);

        public ColorBlock GetButtonBorderColorBlock(UISemantic semantic) => Resolve(semantic, e => e.m_ButtonBorderColorBlock);

        public ColorBlock GetButtonTextColorBlock(UISemantic semantic) => Resolve(semantic, e => e.m_ButtonTextColorBlock);

        public ColorBlock GetButtonIconColorBlock(UISemantic semantic) => Resolve(semantic, e => e.m_ButtonIconColorBlock);

        // --- Animations ---

        public float GetButtonHoverScale(UISemantic semantic) => Resolve(semantic, e => e.m_ButtonHoverScale);

        public float GetButtonClickScale(UISemantic semantic) => Resolve(semantic, e => e.m_ButtonClickScale);

        public float GetButtonHoverAnimationDuration(UISemantic semantic) => Resolve(semantic, e => e.m_ButtonHoverAnimationDuration);

        public float GetButtonClickAnimationDuration(UISemantic semantic) => Resolve(semantic, e => e.m_ButtonClickAnimationDuration);
        
        public float GetButtonBackToNormalAnimationDuration(UISemantic semantic) => Resolve(semantic, e => e.m_ButtonBackToNormalDuration);
        
        public float GetColorAnimationDuration(UISemantic semantic) => Resolve(semantic, e => e.m_ColorAnimationDuration);
        
        // --- Text sizes ---

        public float GetLabelFontSize(UISemantic semantic) => Resolve(semantic, e => e.m_LabelFontSize);

        public float GetHeader1FontSize(UISemantic semantic) => Resolve(semantic, e => e.m_Header1FontSize);

        public float GetHeader2FontSize(UISemantic semantic) => Resolve(semantic, e => e.m_Header2FontSize);
        
        public float GetHeader3FontSize(UISemantic semantic) => Resolve(semantic, e => e.m_Header3FontSize);
    }
}