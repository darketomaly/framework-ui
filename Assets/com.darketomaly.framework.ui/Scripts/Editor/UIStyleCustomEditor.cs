using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Framework.UI.Editor
{
    [CustomEditor(typeof(UIStyle))]
    public class UIStyleCustomEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

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
                    marginTop = 6,
                    marginBottom = 10,
                    backgroundColor = new Color(0.22f, 0.45f, 0.70f, 1f),
                    color = Color.white,
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6
                }
            };
            root.Add(openButton);

            var iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
            {
                do
                {
                    if (iterator.name == "m_Script")
                    {
                        var scriptField = new PropertyField(iterator);
                        scriptField.SetEnabled(false);
                        root.Add(scriptField);
                    }
                    else if (iterator.name is "m_ButtonColorBlock" or "m_ButtonBorderColorBlock" or "m_ButtonTextColorBlock" or "m_ButtonIconColorBlock")
                    {
                        root.Add(CreateColorBlockInspectorField(iterator.Copy()));
                    }
                    else
                    {
                        root.Add(new PropertyField(iterator.Copy()));
                    }
                } while (iterator.NextVisible(false));
            }

            return root;
        }

        private VisualElement CreateColorBlockInspectorField(SerializedProperty blockProp)
        {
            var foldout = new Foldout();
            foldout.text = blockProp.displayName;
            foldout.value = true;
            foldout.style.marginBottom = 4;

            var normalProp = blockProp.FindPropertyRelative("m_NormalColor");
            var highlightedProp = blockProp.FindPropertyRelative("m_HighlightedColor");
            var pressedProp = blockProp.FindPropertyRelative("m_PressedColor");
            var selectedProp = blockProp.FindPropertyRelative("m_SelectedColor");
            var disabledProp = blockProp.FindPropertyRelative("m_DisabledColor");

            if (normalProp != null) foldout.Add(new PropertyField(normalProp, "Normal Color"));
            if (highlightedProp != null) foldout.Add(new PropertyField(highlightedProp, "Highlighted Color"));
            if (pressedProp != null) foldout.Add(new PropertyField(pressedProp, "Pressed Color"));
            if (selectedProp != null) foldout.Add(new PropertyField(selectedProp, "Selected Color"));
            if (disabledProp != null) foldout.Add(new PropertyField(disabledProp, "Disabled Color"));

            return foldout;
        }
    }
}
