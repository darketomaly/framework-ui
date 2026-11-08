using System;
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
            }

            return default;
        }
    }
}
