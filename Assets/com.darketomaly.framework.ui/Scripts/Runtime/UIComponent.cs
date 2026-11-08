using System;
using Framework.Events;
using UnityEngine;

namespace Framework.UI
{
    public class UIComponent : MonoBehaviour
    {
        [SerializeField]
        private UIType m_Type;

        private void OnEnable()
        {
            FrameworkEventsFactory.Subscribe(UIEvents.ManagerInitialized, OnManagerInitialized);
        }

        private void OnDisable()
        {
            FrameworkEventsFactory.Unsubscribe(UIEvents.ManagerInitialized, OnManagerInitialized);
        }
        
        private void OnManagerInitialized()
        {
            Initialize();
        }

        private void Initialize()
        {
            // To do
            // Change color based on the type here

            var color = UIManager.Instance.GetColor(m_Type);
            
            this.Log($"Color should be: {color}");
        }
    }
}