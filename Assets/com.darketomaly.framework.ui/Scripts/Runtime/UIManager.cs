using System;
using Framework.Events;
using UnityEngine;

namespace Framework.UI
{
    public class UIManager : Singleton<UIManager>
    {
        private UIStyle[] m_styles;
        private UIStyle m_currentStyle;

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
            
            m_currentStyle = m_styles[0];
            FrameworkEventsFactory.Raise(UIEvents.ManagerInitialized);
        }

        public Color GetColor(UIType uiType)
        {
            switch (uiType)
            {
                case UIType.Label:
                    return m_currentStyle.m_LabelColor;
            }

            return default;
        }
    }
}
