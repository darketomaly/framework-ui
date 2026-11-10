using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    // To do
    // Should be able to have Label 1, Label 2, Label 3 etc
    // Without having to re-define the enums or the extensions
    
    public enum UIType
    {
        Label,
        Header1,
        Header2,
        Header3,
        Panel,
        PanelBorder,
        ButtonBackground,
        ButtonBorder,
        ButtonText,
        ButtonIcon,
        Logo
    }

    public static class UITypeExtensions
    {
        public static Color GetColor(this UIStyle uiStyle, UIType uiType)
        {
            return uiType switch
            {
                UIType.Label => uiStyle.m_LabelColor,
                UIType.Header1 => uiStyle.m_Header1Color,
                UIType.Header2 => uiStyle.m_Header2Color,
                UIType.Header3 => uiStyle.m_Header3Color,
                UIType.Panel =>  uiStyle.m_PanelColor,
                UIType.PanelBorder => uiStyle.m_PanelBorderColor,
                _ => default
            };
        }

        public static ColorBlock GetColorBlock(this UIStyle uiStyle, UIType uiType)
        {
            switch (uiType)
            {
                case UIType.ButtonBackground:
                    return uiStyle.m_ButtonColorBlock;
                
                case UIType.ButtonBorder:
                    return uiStyle.m_ButtonBorderColorBlock;
                
                case UIType.ButtonText:
                    return uiStyle.m_ButtonTextColorBlock;
                
                case UIType.ButtonIcon:
                    return uiStyle.m_ButtonIconColorBlock;
            }

            return default;
        }

        public static Sprite GetSprite(this UIStyle uiStyle, UIType uiType)
        {
            switch (uiType)
            {
                case UIType.Logo:
                    return uiStyle.m_Logo;
                default:
                    return null;
            }
        }
        
        public static bool IsColor(this UIType uiType)
        {
            return uiType is 
                UIType.Label or
                UIType.Header1 or
                UIType.Header2 or
                UIType.Header3 or
                UIType.Panel or 
                UIType.PanelBorder;
        }
        
        public static bool IsColorBlock(this UIType uiType)
        {
            return uiType is UIType.ButtonBackground or UIType.ButtonBorder or UIType.ButtonText or UIType.ButtonIcon;
        }
        
        public static bool IsSprite(this UIType uiType)
        {
            return uiType is UIType.Logo;
        }
    }
    
    public enum UIEvents
    {
        ManagerInitialized,
        OnThemeChanged
    }
}