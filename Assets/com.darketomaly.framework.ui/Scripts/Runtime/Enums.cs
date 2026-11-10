namespace Framework.UI
{
    // To do
    // Should be able to have Label 1, Label 2, Label 3 etc
    // Without having to re-define the enums or the extensions
    
    public enum UIType
    {
        Label,
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
        public static bool IsColor(this UIType uiType)
        {
            return uiType is UIType.Label or UIType.Panel or UIType.PanelBorder;
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