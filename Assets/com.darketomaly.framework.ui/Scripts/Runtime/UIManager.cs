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

        public static Color GetColor(UIType uiType)
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
                currentStyle = Instance.CurrentStyle;
            }

            return currentStyle.GetColor(uiType);
        }

        public ColorBlock GetColorBlock(UIType uiType)
        {
            return CurrentStyle.GetColorBlock(uiType);
        }

        public Sprite GetSprite(UIType uiType)
        {
            return CurrentStyle.GetSprite(uiType);
        }

        public static void UpdateScale(RectTransform rt, ref Tween tween, ButtonState buttonState)
        {
            var currentStyle = Instance.CurrentStyle;
            var targetScaleMagnitude = 1f;
            var duration = currentStyle.ButtonBackToNormalDuration;
            
            switch (buttonState)
            {
                case ButtonState.Highlighted:
                    duration = currentStyle.ButtonHoverAnimationDuration;
                    targetScaleMagnitude = currentStyle.ButtonHoverScale;
                    break;
                case ButtonState.Pressed:
                    duration = currentStyle.ButtonClickAnimationDuration;
                    targetScaleMagnitude = currentStyle.ButtonClickScale;
                    break;
                case ButtonState.Selected:
                    duration = currentStyle.ButtonClickAnimationDuration;
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
                    Instance.CurrentStyle.ColorAnimationDuration                       
                );
                #else
                graphic.color = targetColor;
                #endif
            }
        }
    }
}
