using System;
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

        public float GetButtonClickScale()
        {
            return CurrentStyle.m_ButtonClickScale;
        }

        public float GetButtonHoverScale()
        {
            return CurrentStyle.m_ButtonHoverScale;
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

        public static void UpdateScale(RectTransform rt, Vector3 targetScale, ref Tween tween)
        {
            #if DOTWEEN
            tween?.Kill();
            rt.DOScale(targetScale, 0.5f);
            #else
            rt.localScale = targetScale;
            #endif
        }
        
        public static void UpdateGraphic(Graphic graphic, Color targetColor, UIType uiType, ref Tween tween, bool instant)
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
                    0.5f                        
                );
                #else
                graphic.color = targetColor;
                #endif
            }
        }
    }
}
