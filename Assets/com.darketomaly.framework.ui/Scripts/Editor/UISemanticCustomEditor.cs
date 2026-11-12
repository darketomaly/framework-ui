using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Framework.UI.Editor
{
    [CustomEditor(typeof(UISemantic))]
    public class UISemanticCustomEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.style.paddingTop = 6;
            root.style.paddingBottom = 6;
            root.style.paddingLeft = 4;
            root.style.paddingRight = 4;

            var helpBox = new HelpBox(
                "Semantics represent visual roles (e.g., Default, Primary, Danger, Warning). Imagine you have two buttons in a window, and you want one of the buttons to stand out. This will use a separate style, but is part of the same theme.\n\nAssign a semantic to UI Components to apply role-specific styling configured in your UI Styles.\n\nA style doesn't need a semantic, some can have some don't. The default semantic will be used in case it's missing.",
                HelpBoxMessageType.Info);
            helpBox.style.marginBottom = 8;
            root.Add(helpBox);

            var nameProp = serializedObject.FindProperty("Name");
            if (nameProp != null)
            {
                var nameField = new PropertyField(nameProp);
                root.Add(nameField);
            }

            return root;
        }
    }
}
