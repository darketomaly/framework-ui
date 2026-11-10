using System;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    // To do
    // Should be able to have Label 1, Label 2, Label 3 etc
    // Without having to re-define the enums or the extensions
    
    public enum UIType
    {
        // Colors (100–199)
        Label = 100,
        Header1,
        Header2,
        Header3,
        Panel,
        PanelBorder,

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
            return uiType switch
            {
                UIType.ButtonBackground => uiStyle.m_ButtonColorBlock,
                UIType.ButtonBorder => uiStyle.m_ButtonBorderColorBlock,
                UIType.ButtonText => uiStyle.m_ButtonTextColorBlock,
                UIType.ButtonIcon => uiStyle.m_ButtonIconColorBlock,
                _ => default
            };
        }

        public static Sprite GetSprite(this UIStyle uiStyle, UIType uiType)
        {
            return uiType switch
            {
                UIType.Logo => uiStyle.m_Logo,
                _ => null
            };
        }
        
        public static bool IsColor(this UIType type) => (int)type is >= 100 and < 200;
        
        public static bool IsColorBlock(this UIType type) => (int)type is >= 200 and < 300;
        
        public static bool IsSprite(this UIType type) => (int)type is >= 300 and < 400;
    }
    
    public enum UIEvents
    {
        ManagerInitialized,
        OnThemeChanged
    }
}