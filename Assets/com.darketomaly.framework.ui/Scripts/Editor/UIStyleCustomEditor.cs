using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Framework.UI.Editor
{
    /*
    [CustomEditor(typeof(UIStyle))]
    public class UIStyleCustomEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.style.paddingTop = 6;
            root.style.paddingBottom = 6;
            root.style.paddingLeft = 4;
            root.style.paddingRight = 4;

            var openButton = new Button(() =>
            {
                UIStyleEditorWindow.OpenWindowWithTarget(target as UIStyle);
            })
            {
                text = "Open in UI Style Editor",
                style =
                {
                    height = 36,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 4,
                    marginBottom = 4,
                    backgroundColor = new Color(0.22f, 0.45f, 0.70f, 1f),
                    color = Color.white,
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6
                }
            };
            root.Add(openButton);

            return root;
        }
    }
    
    */
}
