using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    public enum UIType
    {
        // Colors (100–199)
        Label = 100,
        Header1,
        Header2,
        Header3,
        Panel,
        PanelBorder,
        PanelSeparator,

        // ColorBlocks (200–299)
        ButtonBackground = 200,
        ButtonBorder,
        ButtonText,
        ButtonIcon,

        // Sprites (300–399)
        Logo = 300
    }

    public static class UITypeExtensions
    {
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
            return uiType switch
            {
                UIType.Label => uiStyle.GetLabelFontSize(semantic),
                UIType.Header1 => uiStyle.GetHeader1FontSize(semantic),
                UIType.Header2 => uiStyle.GetHeader2FontSize(semantic),
                UIType.Header3 => uiStyle.GetHeader3FontSize(semantic),
                _ => 0
            };
        }
        
        public static bool IsColor(this UIType type) => (int)type is >= 100 and < 200;
        
        public static bool IsColorBlock(this UIType type) => (int)type is >= 200 and < 300;
        
        public static bool IsSprite(this UIType type) => (int)type is >= 300 and < 400;

        public static bool IsText(this UIType type) => type is 
            UIType.Label or 
            UIType.ButtonText or
            UIType.Header1 or 
            UIType.Header2 or 
            UIType.Header3;
    }
    
    public enum UIEvents
    {
        ManagerInitialized,
        OnThemeChanged
    }
}