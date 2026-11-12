#if DOTWEEN
using DG.Tweening;
#endif

using Framework.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Framework.UI
{
    public class UIManager : Singleton<UIManager>
    {
        private UIStyle[] m_styles;
        private int m_currentStyleIndex;
        
        private static UIStyle CurrentStyle
        {
            get
            {
                UIStyle currentStyle = null;
            
                #if UNITY_EDITOR

                if (!Application.isPlaying)
                {
                    currentStyle = Resources.LoadAll<UIStyle>("")[0];
                }
            
                #endif

                if (!currentStyle)
                {
                    currentStyle = Instance.m_styles[Instance.m_currentStyleIndex];
                }

                return currentStyle;
            }
        }

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

        public static Color GetColor(UIType uiType, UISemantic semantic)
        {
            return CurrentStyle.GetColor(uiType, semantic);
        }

        public ColorBlock GetColorBlock(UIType uiType)
        {
            return CurrentStyle.GetColorBlock(uiType);
        }

        public Sprite GetSprite(UIType uiType, UISemantic semantic)
        {
            return CurrentStyle.GetSprite(uiType, semantic);
        }

        public static void UpdateScale(RectTransform rt, ref Tween tween, ButtonState buttonState)
        {
            var targetScaleMagnitude = 1f;
            var duration = CurrentStyle.ButtonBackToNormalDuration;
            
            switch (buttonState)
            {
                case ButtonState.Highlighted:
                    duration = CurrentStyle.ButtonHoverAnimationDuration;
                    targetScaleMagnitude = CurrentStyle.ButtonHoverScale;
                    break;
                case ButtonState.Pressed:
                    duration = CurrentStyle.ButtonClickAnimationDuration;
                    targetScaleMagnitude = CurrentStyle.ButtonClickScale;
                    break;
                case ButtonState.Selected:
                    duration = CurrentStyle.ButtonClickAnimationDuration;
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

        public static void UpdateSprite(Image image, UIType uiType, UISemantic semantic)
        {
            image.sprite = CurrentStyle.GetSprite(uiType, semantic);
        }

        public static void UpdateTextSize(TextMeshProUGUI txt, UIType uiType, UISemantic semantic)
        {
            var size = CurrentStyle.GetTextSize(uiType, semantic);
            txt.fontSize = size;
        }
        
        public static void UpdateGraphicColor(Graphic graphic, Color targetColor, ref Tween tween, bool instant)
        {
            if (instant || !Application.isPlaying)
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
                    CurrentStyle.ColorAnimationDuration                       
                );
                #else
                graphic.color = targetColor;
                #endif
            }
        }
    }
}
