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
        
        public Overridable<ColorBlock> m_ToggleColorBlock = new();
        public Overridable<ColorBlock> m_ToggleBorderColorBlock = new();
        public Overridable<ColorBlock> m_ToggleTextColorBlock = new();
        public Overridable<ColorBlock> m_ToggleIconColorBlock = new();
        
        [Header("Animations")] 
        public Overridable<float> m_SelectableHoverScale = new();
        public Overridable<float> m_SelectableClickScale = new();
        public Overridable<float> m_SelectableHoverAnimationDuration = new();
        public Overridable<float> m_SelectableClickAnimationDuration = new();
        public Overridable<float> m_SelectableBackToNormalDuration = new();
        public Overridable<float> m_ColorAnimationDuration = new();
        
        [Header("Text sizes")]
        public Overridable<float> m_LabelFontSize = new();
        public Overridable<float> m_Header1FontSize = new();
        public Overridable<float> m_Header2FontSize = new();
        public Overridable<float> m_Header3FontSize = new();
    }
    
    public class UIStyle : ScriptableObject
    {
        // --- Data storage (Parent style, semantics and its style values) ---
        
        [SerializeField] 
        private UIStyle m_ParentStyle;
        
        public List<StyleSemanticEntry> m_StyleSemantics = new();
        
        // --- Sprites ---
        
        public Sprite GetSprite (UIType uiType, UISemantic semantic)
        {
            if (!UITypeExtensions.s_SpriteSelectors.TryGetValue(uiType, out var selector))
            {
                this.LogError($"No sprite mapping for {uiType}");
                return default;
            }

            return Resolve(semantic, selector);
        }
        
        // --- Colors ---

        public Color GetColor(UIType uiType, UISemantic semantic)
        {
            if (!UITypeExtensions.s_ColorSelectors.TryGetValue(uiType, out var selector))
            {
                this.LogError($"No color mapping for {uiType}");
                return default;
            }

            return Resolve(semantic, selector);
        }
        
        // --- Color blocks ---
        
        public ColorBlock GetColorBlock(UIType uiType, UISemantic semantic)
        {
            if (!UITypeExtensions.s_ColorBlockSelectors.TryGetValue(uiType, out var selector))
            {
                this.LogError($"No color block mapping for {uiType}");
                return default;
            }

            return Resolve(semantic, selector);
        }

        // --- Selectable scales ---
        
        public float GetSelectableHoverScale(UISemantic semantic) => Resolve(semantic, e => e.m_SelectableHoverScale);

        public float GetSelectableClickScale(UISemantic semantic) => Resolve(semantic, e => e.m_SelectableClickScale);
        
        // --- Animations ---

        public float GetSelectableHoverAnimationDuration(UISemantic semantic) => Resolve(semantic, e => e.m_SelectableHoverAnimationDuration);

        public float GetSelectableClickAnimationDuration(UISemantic semantic) => Resolve(semantic, e => e.m_SelectableClickAnimationDuration);
        
        public float GetSelectableBackToNormalAnimationDuration(UISemantic semantic) => Resolve(semantic, e => e.m_SelectableBackToNormalDuration);
        
        public float GetColorAnimationDuration(UISemantic semantic) => Resolve(semantic, e => e.m_ColorAnimationDuration);
        
        // --- Text sizes ---

        public float GetTextSize(UIType uiType, UISemantic semantic)
        {
            if (!UITypeExtensions.s_TextSizeSelectors.TryGetValue(uiType, out var selector))
            {
                this.LogError($"No text size mapping for {uiType}");
                return 0f;
            }

            return Resolve(semantic, selector);
        }
        
        // --- Private methods ---
        
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
}