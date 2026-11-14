using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    public enum UIType
    {
        // Colors (100–199)
        [Color, Text] Label = 100,
        [Color, Text] Header1, 
        [Color, Text] Header2,
        [Color, Text] Header3,
        [Color] Panel,
        [Color] PanelBorder,
        [Color] PanelSeparator,

        // ColorBlocks (200–299)
        [ColorBlock] ButtonBackground = 200,
        [ColorBlock] ButtonBorder,
        [ColorBlock, Text] ButtonText,
        [ColorBlock] ButtonIcon,

        // Sprites (300–399)
        [Sprite] Logo = 300
    }

    public static class UITypeExtensions
    {
        public static readonly IReadOnlyDictionary<UIType, Func<StyleSemanticEntry, Overridable<float>>> s_TextSizeSelectors
            = new Dictionary<UIType, Func<StyleSemanticEntry, Overridable<float>>>
            {
                [UIType.Label]   = e => e.m_LabelFontSize,
                [UIType.Header1] = e => e.m_Header1FontSize,
                [UIType.Header2] = e => e.m_Header2FontSize,
                [UIType.Header3] = e => e.m_Header3FontSize,
            };
        
        public static Color GetColor(this UIStyle uiStyle, UIType uiType, UISemantic semantic)
        {
            return uiType switch
            {
                UIType.Label => uiStyle.GetLabelColor(semantic),
                UIType.Header1 => uiStyle.GetHeader1Color(semantic),
                UIType.Header2 => uiStyle.GetHeader2Color(semantic),
                UIType.Header3 => uiStyle.GetHeader3Color(semantic),
                UIType.Panel =>  uiStyle.GetPanelColor(semantic),
                UIType.PanelBorder => uiStyle.GetPanelBorderColor(semantic),
                UIType.PanelSeparator => uiStyle.GetPanelSeparatorColor(semantic),
                _ => default
            };
        }

        public static ColorBlock GetColorBlock(this UIStyle uiStyle, UIType uiType, UISemantic semantic)
        {
            return uiType switch
            {
                UIType.ButtonBackground => uiStyle.GetButtonColorBlock(semantic),
                UIType.ButtonBorder => uiStyle.GetButtonBorderColorBlock(semantic),
                UIType.ButtonText => uiStyle.GetButtonTextColorBlock(semantic),
                UIType.ButtonIcon => uiStyle.GetButtonIconColorBlock(semantic),
                _ => default
            };
        }

        public static Sprite GetSprite(this UIStyle uiStyle, UIType uiType, UISemantic semantic)
        {
            return uiType switch
            {
                UIType.Logo => uiStyle.GetLogo(semantic),
                _ => null
            };
        }
        
        public static float GetTextSize(this UIStyle uiStyle, UIType uiType, UISemantic semantic)
        {
            return uiStyle.GetTextSize(uiType, semantic);
        }
        
        public static bool IsColor(this UIType type) => type.HasAttribute<ColorAttribute>();

        public static bool IsColorBlock(this UIType type) => type.HasAttribute<ColorBlockAttribute>();
        
        public static bool IsSprite(this UIType type) =>  type.HasAttribute<SpriteAttribute>();

        public static bool IsText(this UIType type) =>  type.HasAttribute<TextAttribute>();
        
        private static bool HasAttribute<T>(this UIType type) where T : Attribute
                {
                    // Get the field info for the specific enum value
                    var field = typeof(UIType).GetField(type.ToString());
                
                    if (field == null)
                    {
                        return false;
                    }
        
                    // Check if the attribute is defined on that field
                    return Attribute.IsDefined(field, typeof(T));
                }
    }
    
    public enum UIEvents
    {
        ManagerInitialized,
        OnThemeChanged
    }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class ColorAttribute : PropertyAttribute { }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class ColorBlockAttribute : PropertyAttribute { }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class SpriteAttribute : PropertyAttribute { }
    
    [AttributeUsage(AttributeTargets.Field)]
    public class TextAttribute : PropertyAttribute { }
}