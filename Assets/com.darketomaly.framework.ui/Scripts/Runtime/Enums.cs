namespace Framework.UI
{
    public enum UIType
    {
        Label,
        ButtonBackground,
        ButtonBorder,
        ButtonText,
        ButtonIcon,
        Logo
    }

    public static class UITypeExtensions
    {
        public static bool IsColor(this UIType uiType)
        {
            return uiType is UIType.Label;
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