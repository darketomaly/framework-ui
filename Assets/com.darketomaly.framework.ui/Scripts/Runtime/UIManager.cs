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

        public static ColorBlock GetColorBlock(UIType uiType, UISemantic semantic)
        {
            return CurrentStyle.GetColorBlock(uiType, semantic);
        }

        public static Sprite GetSprite(UIType uiType, UISemantic semantic)
        {
            return CurrentStyle.GetSprite(uiType, semantic);
        }

        public static void UpdateScale(RectTransform rt, SelectableState selectableState, UISemantic semantic)
        {
            var targetScaleMagnitude = 1f;
            var duration = CurrentStyle.GetSelectableBackToNormalAnimationDuration(semantic);

            switch (selectableState)
            {
                case SelectableState.Highlighted:
                    duration = CurrentStyle.GetSelectableHoverAnimationDuration(semantic);
                    targetScaleMagnitude = CurrentStyle.GetSelectableHoverScale(semantic);
                    break;
                case SelectableState.Pressed:
                    duration = CurrentStyle.GetSelectableClickAnimationDuration(semantic);
                    targetScaleMagnitude = CurrentStyle.GetSelectableClickScale(semantic);
                    break;
                case SelectableState.Selected:
                    duration = CurrentStyle.GetSelectableClickAnimationDuration(semantic);
                    break;
            }

            #if DOTWEEN
            rt.DOKill();
            rt.DOScale(targetScaleMagnitude * Vector3.one, duration);
            #elif PRIMETWEEN
            PrimeTween.Tween.StopAll(rt);
            PrimeTween.Tween.Scale(rt, targetScaleMagnitude * Vector3.one, duration);
            #else
            rt.localScale = targetScaleMagnitude * Vector3.one;
            #endif
        }

        public static void UpdateTextSize(TextMeshProUGUI txt, UIType uiType, UISemantic semantic)
        {
            var size = CurrentStyle.GetTextSize(uiType, semantic);
            txt.fontSize = size;
        }
        
        public static void UpdateGraphicColor(Graphic graphic, Color targetColor, bool instant, UISemantic semantic)
        {
            if (graphic == null) return;

            #if DOTWEEN
            graphic.DOKill();
            #elif PRIMETWEEN
            PrimeTween.Tween.StopAll(graphic);
            #endif

            if (instant || !Application.isPlaying)
            {
                graphic.color = targetColor;
            }
            else
            {
                var duration = CurrentStyle.GetColorAnimationDuration(semantic);
                #if DOTWEEN
                DOTween.To(() => graphic.color, x => graphic.color = x, targetColor, duration).SetTarget(graphic);
                #elif PRIMETWEEN
                PrimeTween.Tween.Color(graphic, targetColor, duration);
                #else
                graphic.color = targetColor;
                #endif
            }
        }
    }
}