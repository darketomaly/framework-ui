using DG.Tweening;
using Framework.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    public class UIManager : Singleton<UIManager>
    {
        private UIStyle[] m_styles;
        private int m_currentStyleIndex;
        
        private UIStyle CurrentStyle => m_styles[m_currentStyleIndex]; 

        [RuntimeInitializeOnLoadMethod]
        private static void Spawn()
        {
            var myGo = new GameObject("UI Manager");
            myGo.AddComponent<UIManager>();
            
            DontDestroyOnLoad(myGo.gameObject);                
        }
        
        private void Start()
        {
            m_styles = Resources.LoadAll<UIStyle>("");
            
            m_currentStyleIndex = 0;
            FrameworkEventsFactory.Raise(UIEvents.ManagerInitialized);
        }

        [DarkButton]
        public void ChangeTheme()
        {
            m_currentStyleIndex = (m_currentStyleIndex + 1) % m_styles.Length;
            FrameworkEventsFactory.Raise(UIEvents.OnThemeChanged);
        }

        public Color GetColor(UIType uiType)
        {
            switch (uiType)
            {
                case UIType.Label:
                    return CurrentStyle.m_LabelColor;
            }

            return default;
        }

        public ColorBlock GetColorBlock(UIType uiType)
        {
            switch (uiType)
            {
                case UIType.ButtonBackground:
                    return CurrentStyle.m_ButtonColorBlock;
                
                case UIType.ButtonBorder:
                    return CurrentStyle.m_ButtonBorderColorBlock;
                
                case UIType.ButtonText:
                    return CurrentStyle.m_ButtonTextColorBlock;
                
                case UIType.ButtonIcon:
                    return CurrentStyle.m_ButtonIconColorBlock;
            }

            return default;
        }

        public Sprite GetSprite(UIType uiType)
        {
            switch (uiType)
            {
                case UIType.Logo:
                    return CurrentStyle.m_Logo;
                default:
                    return null;
            }
        }

        public static void UpdateScale(RectTransform rt, ref Tween tween, ButtonState buttonState)
        {
            var currentStyle = Instance.CurrentStyle;
            var targetScaleMagnitude = 1f;
            var duration = currentStyle.m_ButtonBackToNormalDuration;
            
            switch (buttonState)
            {
                case ButtonState.Highlighted:
                    duration = currentStyle.m_ButtonHoverAnimationDuration;
                    targetScaleMagnitude = currentStyle.m_ButtonHoverScale;
                    break;
                case ButtonState.Pressed:
                    duration = currentStyle.m_ButtonClickAnimationDuration;
                    targetScaleMagnitude = currentStyle.m_ButtonClickScale;
                    break;
                case ButtonState.Selected:
                    duration = currentStyle.m_ButtonClickAnimationDuration;
                    break;
            }

            Instance.Log($"{buttonState}, Changing scale to {targetScaleMagnitude}");
            
            #if DOTWEEN
            tween?.Kill();
            tween = rt.DOScale(targetScaleMagnitude * Vector3.one, duration);
            #else
            rt.localScale = targetScaleMagnitude * Vector3.one;
            #endif
        }
        
        public static void UpdateGraphicColor(Graphic graphic, Color targetColor, ref Tween tween, bool instant)
        {
            if (instant)
            {
                graphic.color = targetColor;
            }
            else
            {
                #if DOTWEEN
                tween?.Kill();
                
                tween = DOTween.To(
                    () => graphic.color,    
                    x => graphic.color = x,   
                    targetColor,                 
                    Instance.CurrentStyle.m_ColorAnimationDuration                       
                );
                #else
                graphic.color = targetColor;
                #endif
            }
        }
    }
}
