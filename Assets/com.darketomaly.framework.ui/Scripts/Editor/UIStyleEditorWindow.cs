using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Image = UnityEngine.UIElements.Image;
using Toggle = UnityEngine.UIElements.Toggle;
using Slider = UnityEngine.UIElements.Slider;

namespace Framework.UI.Editor
{
    public class UIStyleEditorWindow : EditorWindow
    {
        private UIStyle m_TargetStyle;
        private SerializedObject m_SerializedObject;

        // UI Toolkit Root & Layout
        private ObjectField m_StyleObjectField;
        private ScrollView m_LeftScrollView;
        private VisualElement m_RightPreviewPane;
        private VisualElement m_PreviewViewport;
        private VisualElement m_EmptyStateContainer;
        private VisualElement m_EditorContainer;

        // Preview Elements
        private VisualElement m_PreviewCanvas;
        private VisualElement m_PreviewPanel;
        private Image m_PreviewLogoImage;
        private Label m_PreviewHeader1;
        private Label m_PreviewHeader2;
        private Label m_PreviewHeader3;
        private Label m_PreviewLabel;

        // Primary Preview Button
        private VisualElement m_PreviewButton;
        private Image m_PreviewButtonIcon;
        private Label m_PreviewButtonText;

        // Secondary / Disabled Preview Button
        private VisualElement m_DisabledPreviewButton;
        private Image m_DisabledPreviewButtonIcon;
        private Label m_DisabledPreviewButtonText;

        // Status & State
        private Label m_StateInfoLabel;
        private ButtonState m_CurrentButtonState = ButtonState.Normal;
        private bool m_IsInteractive = true;
        private int m_BackgroundThemeIndex = 0; // 0 = Dark, 1 = Light, 2 = Checkered
        private float m_PreviewScale = 1f;

        // State Buttons
        private Dictionary<ButtonState, Button> m_StateButtons = new Dictionary<ButtonState, Button>();
        private Button m_InteractiveToggleButton;

        [MenuItem("Window/Framework/UI Style Editor", false, 100)]
        [MenuItem("Framework/UI Style Editor", false, 100)]
        public static void OpenWindow()
        {
            var window = GetWindow<UIStyleEditorWindow>("UI Style Editor");
            window.minSize = new Vector2(800, 520);
            window.Show();

            if (Selection.activeObject is UIStyle selectedStyle)
            {
                window.SetTarget(selectedStyle);
            }
            else if (window.m_TargetStyle == null)
            {
                var styles = Resources.LoadAll<UIStyle>("");
                if (styles != null && styles.Length > 0)
                {
                    window.SetTarget(styles[0]);
                }
            }
        }

        public static void OpenWindowWithTarget(UIStyle style)
        {
            var window = GetWindow<UIStyleEditorWindow>("UI Style Editor");
            window.minSize = new Vector2(800, 520);
            window.Show();
            window.SetTarget(style);
        }

        private void OnEnable()
        {
            var sheetIcon = EditorGUIUtility.FindTexture("d_StyleSheet Icon") ?? EditorGUIUtility.FindTexture("StyleSheet Icon");
            titleContent = new GUIContent("UI Style Editor", sheetIcon);
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            m_LeftScrollView?.Unbind();
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject is UIStyle selectedStyle && selectedStyle != m_TargetStyle)
            {
                SetTarget(selectedStyle);
            }
        }

        private void OnUndoRedo()
        {
            if (m_SerializedObject != null && m_SerializedObject.targetObject != null)
            {
                m_SerializedObject.Update();
                UpdatePreview();
            }
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;
            root.style.flexGrow = 1;
            root.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f, 1f);

            // 1. Top Navigation & Selection Bar
            root.Add(CreateTopToolbar());

            // 2. Main Content SplitView
            var splitView = new TwoPaneSplitView(0, 390, TwoPaneSplitViewOrientation.Horizontal);
            splitView.style.flexGrow = 1;

            // Left Pane: Properties ScrollView
            var leftPane = new VisualElement();
            leftPane.style.flexDirection = FlexDirection.Column;
            leftPane.style.flexGrow = 1;
            leftPane.style.backgroundColor = new Color(0.21f, 0.21f, 0.21f, 1f);

            var leftHeader = new VisualElement();
            leftHeader.style.paddingLeft = 14;
            leftHeader.style.paddingRight = 14;
            leftHeader.style.paddingTop = 10;
            leftHeader.style.paddingBottom = 10;
            leftHeader.style.borderBottomWidth = 1;
            leftHeader.style.borderBottomColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            leftHeader.style.backgroundColor = new Color(0.24f, 0.24f, 0.24f, 1f);

            var leftTitle = new Label("STYLE PROPERTIES");
            leftTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            leftTitle.style.fontSize = 12;
            leftTitle.style.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            leftHeader.Add(leftTitle);
            leftPane.Add(leftHeader);

            m_LeftScrollView = new ScrollView();
            m_LeftScrollView.style.flexGrow = 1;
            m_LeftScrollView.style.paddingLeft = 12;
            m_LeftScrollView.style.paddingRight = 12;
            m_LeftScrollView.style.paddingTop = 8;
            m_LeftScrollView.style.paddingBottom = 16;
            m_LeftScrollView.RegisterCallback<SerializedPropertyChangeEvent>(evt =>
            {
                UpdatePreview();
            });
            leftPane.Add(m_LeftScrollView);

            splitView.Add(leftPane);

            // Right Pane: Live Preview
            m_RightPreviewPane = CreateRightPreviewPane();
            splitView.Add(m_RightPreviewPane);

            root.Add(splitView);

            // Initial Target Selection Setup
            if (m_TargetStyle != null)
            {
                BuildPropertiesUI();
                UpdatePreview();
            }
            else
            {
                if (Selection.activeObject is UIStyle selectedStyle)
                {
                    SetTarget(selectedStyle);
                }
                else
                {
                    var styles = Resources.LoadAll<UIStyle>("");
                    if (styles != null && styles.Length > 0)
                    {
                        SetTarget(styles[0]);
                    }
                    else
                    {
                        ShowEmptyState();
                    }
                }
            }
        }

        #region Top Toolbar

        private VisualElement CreateTopToolbar()
        {
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.alignItems = Align.Center;
            toolbar.style.paddingLeft = 12;
            toolbar.style.paddingRight = 12;
            toolbar.style.paddingTop = 6;
            toolbar.style.paddingBottom = 6;
            toolbar.style.borderBottomWidth = 1;
            toolbar.style.borderBottomColor = new Color(0.13f, 0.13f, 0.13f, 1f);
            toolbar.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f, 1f);

            var sheetIcon = EditorGUIUtility.FindTexture("d_StyleSheet Icon") ?? EditorGUIUtility.FindTexture("StyleSheet Icon");
            if (sheetIcon != null)
            {
                var titleIcon = new Image();
                titleIcon.image = sheetIcon;
                titleIcon.style.width = 18;
                titleIcon.style.height = 18;
                titleIcon.style.marginRight = 8;
                toolbar.Add(titleIcon);
            }

            var titleLabel = new Label("UI Style:");
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.fontSize = 12;
            titleLabel.style.marginRight = 8;
            toolbar.Add(titleLabel);

            m_StyleObjectField = new ObjectField
            {
                objectType = typeof(UIStyle),
                allowSceneObjects = false,
                value = m_TargetStyle
            };
            m_StyleObjectField.style.width = 240;
            m_StyleObjectField.RegisterValueChangedCallback(evt =>
            {
                SetTarget(evt.newValue as UIStyle);
            });
            toolbar.Add(m_StyleObjectField);

            var pingButton = new Button(() =>
            {
                if (m_TargetStyle != null)
                {
                    EditorGUIUtility.PingObject(m_TargetStyle);
                    Selection.activeObject = m_TargetStyle;
                }
            })
            {
                text = "Ping"
            };
            pingButton.style.marginLeft = 6;
            pingButton.style.paddingLeft = 10;
            pingButton.style.paddingRight = 10;
            toolbar.Add(pingButton);

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            toolbar.Add(spacer);

            var createNewButton = new Button(CreateNewStyleAsset)
            {
                text = "+ Create New Style"
            };
            createNewButton.style.backgroundColor = new Color(0.22f, 0.45f, 0.70f, 1f);
            createNewButton.style.color = Color.white;
            createNewButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            createNewButton.style.paddingLeft = 12;
            createNewButton.style.paddingRight = 12;
            toolbar.Add(createNewButton);

            return toolbar;
        }

        private void CreateNewStyleAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Create UI Style", "New UI Style", "asset", "Save new UI Style asset");
            if (string.IsNullOrEmpty(path)) return;

            var newStyle = CreateInstance<UIStyle>();
            // Sensible defaults
            newStyle.m_LabelColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            newStyle.m_Header1Color = new Color(1f, 1f, 1f, 1f);
            newStyle.m_Header2Color = new Color(0.88f, 0.88f, 0.88f, 1f);
            newStyle.m_Header3Color = new Color(0.75f, 0.75f, 0.75f, 1f);
            newStyle.m_PanelColor = new Color(0.18f, 0.22f, 0.28f, 1f);
            newStyle.m_PanelBorderColor = new Color(0.28f, 0.52f, 0.80f, 1f);

            newStyle.m_ButtonColorBlock = new ColorBlock
            {
                normalColor = new Color(0.24f, 0.42f, 0.65f, 1f),
                highlightedColor = new Color(0.32f, 0.55f, 0.85f, 1f),
                pressedColor = new Color(0.18f, 0.32f, 0.52f, 1f),
                selectedColor = new Color(0.30f, 0.50f, 0.80f, 1f),
                disabledColor = new Color(0.25f, 0.25f, 0.25f, 0.6f),
                colorMultiplier = 1f,
                fadeDuration = 0.2f
            };

            newStyle.m_ButtonBorderColorBlock = new ColorBlock
            {
                normalColor = new Color(0.40f, 0.65f, 0.95f, 1f),
                highlightedColor = new Color(0.60f, 0.80f, 1f, 1f),
                pressedColor = new Color(0.25f, 0.45f, 0.75f, 1f),
                selectedColor = new Color(0.50f, 0.75f, 1f, 1f),
                disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.4f),
                colorMultiplier = 1f,
                fadeDuration = 0.2f
            };

            newStyle.m_ButtonTextColorBlock = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = Color.white,
                pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f),
                selectedColor = Color.white,
                disabledColor = new Color(0.6f, 0.6f, 0.6f, 1f),
                colorMultiplier = 1f,
                fadeDuration = 0.2f
            };

            newStyle.m_ButtonIconColorBlock = new ColorBlock
            {
                normalColor = Color.white,
                highlightedColor = Color.white,
                pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f),
                selectedColor = Color.white,
                disabledColor = new Color(0.6f, 0.6f, 0.6f, 1f),
                colorMultiplier = 1f,
                fadeDuration = 0.2f
            };

            newStyle.m_ButtonHoverScale = 1.05f;
            newStyle.m_ButtonClickScale = 0.95f;
            newStyle.m_ButtonHoverAnimationDuration = 0.2f;
            newStyle.m_ButtonClickAnimationDuration = 0.1f;
            newStyle.m_ButtonBackToNormalDuration = 0.2f;
            newStyle.m_ColorAnimationDuration = 0.2f;

            AssetDatabase.CreateAsset(newStyle, path);
            AssetDatabase.SaveAssets();
            SetTarget(newStyle);
        }

        public void SetTarget(UIStyle style)
        {
            m_TargetStyle = style;
            if (m_StyleObjectField != null && m_StyleObjectField.value != style)
            {
                m_StyleObjectField.SetValueWithoutNotify(style);
            }

            if (m_TargetStyle == null)
            {
                ShowEmptyState();
            }
            else
            {
                m_SerializedObject = new SerializedObject(m_TargetStyle);
                BuildPropertiesUI();
                UpdatePreview();
            }
        }

        #endregion

        #region Left Pane: Properties

        private void ShowEmptyState()
        {
            if (m_LeftScrollView == null) return;
            m_LeftScrollView.Unbind();
            m_LeftScrollView.Clear();

            var emptyBox = new VisualElement();
            emptyBox.style.alignItems = Align.Center;
            emptyBox.style.justifyContent = Justify.Center;
            emptyBox.style.paddingTop = 40;
            emptyBox.style.paddingBottom = 40;

            var warnIcon = EditorGUIUtility.FindTexture("console.warnicon.sml") ?? EditorGUIUtility.FindTexture("Warning");
            if (warnIcon != null)
            {
                var emptyIcon = new Image();
                emptyIcon.image = warnIcon;
                emptyIcon.style.width = 32;
                emptyIcon.style.height = 32;
                emptyIcon.style.marginBottom = 12;
                emptyBox.Add(emptyIcon);
            }

            var emptyLabel = new Label("No UIStyle selected.");
            emptyLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            emptyLabel.style.fontSize = 14;
            emptyLabel.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            emptyLabel.style.marginBottom = 6;
            emptyBox.Add(emptyLabel);

            var emptySubLabel = new Label("Select a UIStyle asset from the toolbar or project window to edit.");
            emptySubLabel.style.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            emptySubLabel.style.marginBottom = 16;
            emptyBox.Add(emptySubLabel);

            var createBtn = new Button(CreateNewStyleAsset)
            {
                text = "Create New Style"
            };
            createBtn.style.paddingLeft = 16;
            createBtn.style.paddingRight = 16;
            createBtn.style.height = 30;
            emptyBox.Add(createBtn);

            m_LeftScrollView.Add(emptyBox);

            if (m_PreviewPanel != null)
            {
                m_PreviewPanel.style.display = DisplayStyle.None;
            }
        }

        private void BuildPropertiesUI()
        {
            if (m_LeftScrollView == null || m_SerializedObject == null) return;
            m_LeftScrollView.Unbind();
            m_LeftScrollView.Clear();

            if (m_PreviewPanel != null)
            {
                m_PreviewPanel.style.display = DisplayStyle.Flex;
            }

            // Section 1: Sprites
            var spritesCard = CreateSectionCard("Sprites & Branding", new Color(0.25f, 0.60f, 0.95f, 1f));
            AddPropertyField(spritesCard, "m_Logo", "Logo Sprite");
            m_LeftScrollView.Add(spritesCard);

            // Section 2: Regular Colors
            var regularColorsCard = CreateSectionCard("Regular Colors", new Color(0.70f, 0.40f, 0.95f, 1f));
            AddPropertyField(regularColorsCard, "m_LabelColor", "Label Color");
            AddPropertyField(regularColorsCard, "m_Header1Color", "Header 1 Color");
            AddPropertyField(regularColorsCard, "m_Header2Color", "Header 2 Color");
            AddPropertyField(regularColorsCard, "m_Header3Color", "Header 3 Color");
            AddPropertyField(regularColorsCard, "m_PanelColor", "Panel Color");
            AddPropertyField(regularColorsCard, "m_PanelBorderColor", "Panel Border Color");
            m_LeftScrollView.Add(regularColorsCard);

            // Section 3: Button Color Blocks
            var colorBlocksCard = CreateSectionCard("Button Color Blocks", new Color(0.95f, 0.65f, 0.20f, 1f));
            AddColorBlockField(colorBlocksCard, "m_ButtonColorBlock", "Button Background Colors");
            AddColorBlockField(colorBlocksCard, "m_ButtonBorderColorBlock", "Button Border Colors");
            AddColorBlockField(colorBlocksCard, "m_ButtonTextColorBlock", "Button Text Colors");
            AddColorBlockField(colorBlocksCard, "m_ButtonIconColorBlock", "Button Icon Colors");
            m_LeftScrollView.Add(colorBlocksCard);

            // Section 4: Animations
            var animCard = CreateSectionCard("Animations & Scale", new Color(0.20f, 0.80f, 0.50f, 1f));
            AddPropertyField(animCard, "m_ButtonHoverScale", "Hover Scale");
            AddPropertyField(animCard, "m_ButtonClickScale", "Click Scale");
            AddPropertyField(animCard, "m_ButtonHoverAnimationDuration", "Hover Animation Duration (s)");
            AddPropertyField(animCard, "m_ButtonClickAnimationDuration", "Click Animation Duration (s)");
            AddPropertyField(animCard, "m_ButtonBackToNormalDuration", "Return To Normal Duration (s)");
            AddPropertyField(animCard, "m_ColorAnimationDuration", "Color Transition Duration (s)");
            m_LeftScrollView.Add(animCard);

            // Bind the SerializedObject so undo/redo, auto-serialization, and dirty tracking work
            m_LeftScrollView.Bind(m_SerializedObject);
        }

        private VisualElement CreateSectionCard(string title, Color accentColor)
        {
            var card = new VisualElement();
            card.style.backgroundColor = new Color(0.24f, 0.24f, 0.24f, 1f);
            card.style.borderTopWidth = 1;
            card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1;
            card.style.borderRightWidth = 1;
            card.style.borderTopColor = new Color(0.18f, 0.18f, 0.18f, 1f);
            card.style.borderBottomColor = new Color(0.18f, 0.18f, 0.18f, 1f);
            card.style.borderLeftColor = new Color(0.18f, 0.18f, 0.18f, 1f);
            card.style.borderRightColor = new Color(0.18f, 0.18f, 0.18f, 1f);
            card.style.borderTopLeftRadius = 6;
            card.style.borderTopRightRadius = 6;
            card.style.borderBottomLeftRadius = 6;
            card.style.borderBottomRightRadius = 6;
            card.style.marginBottom = 12;
            card.style.paddingLeft = 10;
            card.style.paddingRight = 10;
            card.style.paddingTop = 8;
            card.style.paddingBottom = 8;

            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 8;
            header.style.paddingBottom = 4;
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = new Color(0.20f, 0.20f, 0.20f, 1f);

            var accentBar = new VisualElement();
            accentBar.style.width = 4;
            accentBar.style.height = 14;
            accentBar.style.borderTopLeftRadius = 2;
            accentBar.style.borderTopRightRadius = 2;
            accentBar.style.borderBottomLeftRadius = 2;
            accentBar.style.borderBottomRightRadius = 2;
            accentBar.style.backgroundColor = accentColor;
            accentBar.style.marginRight = 8;
            header.Add(accentBar);

            var label = new Label(title);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.fontSize = 11;
            label.style.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            header.Add(label);

            card.Add(header);
            return card;
        }

        private void AddPropertyField(VisualElement parent, string propertyPath, string customLabel = null)
        {
            var prop = m_SerializedObject.FindProperty(propertyPath);
            if (prop == null) return;

            var propField = new PropertyField(prop, customLabel ?? prop.displayName);
            propField.style.marginBottom = 4;
            parent.Add(propField);
        }

        private void AddColorBlockField(VisualElement parent, string propertyPath, string customLabel)
        {
            var blockProp = m_SerializedObject.FindProperty(propertyPath);
            if (blockProp == null) return;

            var foldout = new Foldout();
            foldout.text = customLabel;
            foldout.value = true;
            foldout.style.marginBottom = 8;
            foldout.style.backgroundColor = new Color(0.20f, 0.20f, 0.20f, 0.5f);
            foldout.style.paddingLeft = 8;
            foldout.style.paddingRight = 6;
            foldout.style.paddingTop = 4;
            foldout.style.paddingBottom = 6;
            foldout.style.borderTopLeftRadius = 4;
            foldout.style.borderTopRightRadius = 4;
            foldout.style.borderBottomLeftRadius = 4;
            foldout.style.borderBottomRightRadius = 4;

            var normalProp = blockProp.FindPropertyRelative("m_NormalColor");
            var highlightedProp = blockProp.FindPropertyRelative("m_HighlightedColor");
            var pressedProp = blockProp.FindPropertyRelative("m_PressedColor");
            var selectedProp = blockProp.FindPropertyRelative("m_SelectedColor");
            var disabledProp = blockProp.FindPropertyRelative("m_DisabledColor");

            if (normalProp != null)
            {
                var f = new PropertyField(normalProp, "Normal Color");
                f.style.marginBottom = 2;
                foldout.Add(f);
            }
            if (highlightedProp != null)
            {
                var f = new PropertyField(highlightedProp, "Highlighted Color");
                f.style.marginBottom = 2;
                foldout.Add(f);
            }
            if (pressedProp != null)
            {
                var f = new PropertyField(pressedProp, "Pressed Color");
                f.style.marginBottom = 2;
                foldout.Add(f);
            }
            if (selectedProp != null)
            {
                var f = new PropertyField(selectedProp, "Selected Color");
                f.style.marginBottom = 2;
                foldout.Add(f);
            }
            if (disabledProp != null)
            {
                var f = new PropertyField(disabledProp, "Disabled Color");
                f.style.marginBottom = 2;
                foldout.Add(f);
            }

            parent.Add(foldout);
        }

        #endregion

        #region Right Pane: Live Preview

        private VisualElement CreateRightPreviewPane()
        {
            var pane = new VisualElement();
            pane.style.flexDirection = FlexDirection.Column;
            pane.style.flexGrow = 1;
            pane.style.backgroundColor = new Color(0.14f, 0.14f, 0.14f, 1f);

            // Preview Toolbar
            var toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.alignItems = Align.Center;
            toolbar.style.paddingLeft = 12;
            toolbar.style.paddingRight = 12;
            toolbar.style.paddingTop = 8;
            toolbar.style.paddingBottom = 8;
            toolbar.style.borderBottomWidth = 1;
            toolbar.style.borderBottomColor = new Color(0.11f, 0.11f, 0.11f, 1f);
            toolbar.style.backgroundColor = new Color(0.18f, 0.18f, 0.18f, 1f);

            var previewTitle = new Label("LIVE PREVIEW");
            previewTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            previewTitle.style.fontSize = 12;
            previewTitle.style.color = new Color(0.85f, 0.85f, 0.85f, 1f);
            previewTitle.style.marginRight = 16;
            toolbar.Add(previewTitle);

            // Interactive Mode Toggle Button
            m_InteractiveToggleButton = new Button(() =>
            {
                SetInteractiveMode(!m_IsInteractive);
            })
            {
                text = "Interactive: ON"
            };
            m_InteractiveToggleButton.style.paddingLeft = 10;
            m_InteractiveToggleButton.style.paddingRight = 10;
            m_InteractiveToggleButton.style.marginRight = 8;
            m_InteractiveToggleButton.style.backgroundColor = new Color(0.20f, 0.50f, 0.35f, 1f);
            m_InteractiveToggleButton.style.color = Color.white;
            m_InteractiveToggleButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            toolbar.Add(m_InteractiveToggleButton);

            // State Preview Buttons
            var statesLabel = new Label("State:");
            statesLabel.style.fontSize = 11;
            statesLabel.style.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            statesLabel.style.marginRight = 4;
            toolbar.Add(statesLabel);

            var stateGroup = new VisualElement();
            stateGroup.style.flexDirection = FlexDirection.Row;
            stateGroup.style.marginRight = 12;

            ButtonState[] states = { ButtonState.Normal, ButtonState.Highlighted, ButtonState.Pressed, ButtonState.Selected, ButtonState.Disabled };
            foreach (var state in states)
            {
                var stateBtn = new Button(() =>
                {
                    SetInteractiveMode(false);
                    SetButtonState(state);
                })
                {
                    text = state.ToString()
                };
                stateBtn.style.paddingLeft = 6;
                stateBtn.style.paddingRight = 6;
                stateBtn.style.fontSize = 10;
                m_StateButtons[state] = stateBtn;
                stateGroup.Add(stateBtn);
            }
            toolbar.Add(stateGroup);

            var spacer = new VisualElement();
            spacer.style.flexGrow = 1;
            toolbar.Add(spacer);

            // Zoom / Scale Slider
            var zoomLabel = new Label("Zoom:");
            zoomLabel.style.fontSize = 10;
            zoomLabel.style.color = new Color(0.65f, 0.65f, 0.65f, 1f);
            zoomLabel.style.marginRight = 4;
            toolbar.Add(zoomLabel);

            var zoomSlider = new Slider(0.6f, 1.4f)
            {
                value = m_PreviewScale
            };
            zoomSlider.style.width = 75;
            zoomSlider.style.marginRight = 12;
            zoomSlider.RegisterValueChangedCallback(evt =>
            {
                m_PreviewScale = evt.newValue;
                ApplyPreviewScale();
            });
            toolbar.Add(zoomSlider);

            // Background Theme Toggle
            var bgLabel = new Label("Canvas BG:");
            bgLabel.style.fontSize = 10;
            bgLabel.style.color = new Color(0.65f, 0.65f, 0.65f, 1f);
            bgLabel.style.marginRight = 4;
            toolbar.Add(bgLabel);

            var bgBtn = new Button(ToggleBackgroundTheme)
            {
                text = "Dark"
            };
            bgBtn.name = "bgThemeBtn";
            bgBtn.style.fontSize = 10;
            bgBtn.style.paddingLeft = 8;
            bgBtn.style.paddingRight = 8;
            toolbar.Add(bgBtn);

            pane.Add(toolbar);

            // Viewport ScrollView / Canvas Container
            m_PreviewViewport = new ScrollView();
            m_PreviewViewport.style.flexGrow = 1;
            m_PreviewViewport.contentContainer.style.flexGrow = 1;
            m_PreviewViewport.contentContainer.style.alignItems = Align.Center;
            m_PreviewViewport.contentContainer.style.justifyContent = Justify.Center;
            m_PreviewViewport.contentContainer.style.paddingTop = 24;
            m_PreviewViewport.contentContainer.style.paddingBottom = 24;
            m_PreviewViewport.contentContainer.style.paddingLeft = 24;
            m_PreviewViewport.contentContainer.style.paddingRight = 24;

            UpdateBackgroundTheme();

            // Preview Panel inside Viewport
            m_PreviewPanel = CreatePreviewPanel();
            m_PreviewViewport.Add(m_PreviewPanel);

            pane.Add(m_PreviewViewport);

            // Status Bar at Bottom
            var statusBar = new VisualElement();
            statusBar.style.flexDirection = FlexDirection.Row;
            statusBar.style.alignItems = Align.Center;
            statusBar.style.paddingLeft = 12;
            statusBar.style.paddingRight = 12;
            statusBar.style.paddingTop = 4;
            statusBar.style.paddingBottom = 4;
            statusBar.style.borderTopWidth = 1;
            statusBar.style.borderTopColor = new Color(0.11f, 0.11f, 0.11f, 1f);
            statusBar.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 1f);

            m_StateInfoLabel = new Label("State: Normal | Hover / Click the button to test live animations.");
            m_StateInfoLabel.style.fontSize = 11;
            m_StateInfoLabel.style.color = new Color(0.6f, 0.75f, 0.9f, 1f);
            statusBar.Add(m_StateInfoLabel);

            pane.Add(statusBar);

            return pane;
        }

        private VisualElement CreatePreviewPanel()
        {
            var panel = new VisualElement();
            panel.style.minWidth = 360;
            panel.style.maxWidth = 480;
            panel.style.paddingLeft = 22;
            panel.style.paddingRight = 22;
            panel.style.paddingTop = 20;
            panel.style.paddingBottom = 20;
            panel.style.borderTopWidth = 2;
            panel.style.borderRightWidth = 2;
            panel.style.borderBottomWidth = 2;
            panel.style.borderLeftWidth = 2;
            panel.style.borderTopLeftRadius = 12;
            panel.style.borderTopRightRadius = 12;
            panel.style.borderBottomLeftRadius = 12;
            panel.style.borderBottomRightRadius = 12;

            // Header Container (Logo + Titles)
            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.marginBottom = 12;

            m_PreviewLogoImage = new Image();
            m_PreviewLogoImage.style.width = 36;
            m_PreviewLogoImage.style.height = 36;
            m_PreviewLogoImage.style.marginRight = 12;
            m_PreviewLogoImage.style.display = DisplayStyle.None;
            headerRow.Add(m_PreviewLogoImage);

            var titleCol = new VisualElement();
            titleCol.style.flexDirection = FlexDirection.Column;
            titleCol.style.flexGrow = 1;

            m_PreviewHeader1 = new Label("Header 1 Title");
            m_PreviewHeader1.style.fontSize = 18;
            m_PreviewHeader1.style.unityFontStyleAndWeight = FontStyle.Bold;
            m_PreviewHeader1.style.marginBottom = 2;
            titleCol.Add(m_PreviewHeader1);

            m_PreviewHeader2 = new Label("Header 2 Subtitle");
            m_PreviewHeader2.style.fontSize = 14;
            m_PreviewHeader2.style.unityFontStyleAndWeight = FontStyle.Bold;
            m_PreviewHeader2.style.marginBottom = 2;
            titleCol.Add(m_PreviewHeader2);

            m_PreviewHeader3 = new Label("Header 3 Section");
            m_PreviewHeader3.style.fontSize = 12;
            m_PreviewHeader3.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleCol.Add(m_PreviewHeader3);

            headerRow.Add(titleCol);
            panel.Add(headerRow);

            // Body Label
            m_PreviewLabel = new Label("This is a preview label text demonstrating regular typography, contrast, and layout inside the styled panel.");
            m_PreviewLabel.style.fontSize = 12;
            m_PreviewLabel.style.whiteSpace = WhiteSpace.Normal;
            m_PreviewLabel.style.marginBottom = 16;
            panel.Add(m_PreviewLabel);

            // Divider Line
            var divider = new VisualElement();
            divider.style.height = 1;
            divider.style.backgroundColor = new Color(1f, 1f, 1f, 0.15f);
            divider.style.marginBottom = 16;
            panel.Add(divider);

            // Buttons Container
            var buttonsContainer = new VisualElement();
            buttonsContainer.style.flexDirection = FlexDirection.Column;
            buttonsContainer.style.alignItems = Align.Center;

            var buttonsRow = new VisualElement();
            buttonsRow.style.flexDirection = FlexDirection.Row;
            buttonsRow.style.alignItems = Align.Center;
            buttonsRow.style.justifyContent = Justify.Center;

            // Primary Interactive Preview Button
            m_PreviewButton = CreatePreviewButtonElement(false);
            buttonsRow.Add(m_PreviewButton);

            // Secondary / Disabled Preview Button
            m_DisabledPreviewButton = CreatePreviewButtonElement(true);
            m_DisabledPreviewButton.style.marginLeft = 12;
            buttonsRow.Add(m_DisabledPreviewButton);

            buttonsContainer.Add(buttonsRow);

            panel.Add(buttonsContainer);

            return panel;
        }

        private VisualElement CreatePreviewButtonElement(bool isDisabledButton)
        {
            var btn = new VisualElement();
            btn.style.flexDirection = FlexDirection.Row;
            btn.style.alignItems = Align.Center;
            btn.style.justifyContent = Justify.Center;
            btn.style.paddingLeft = 18;
            btn.style.paddingRight = 18;
            btn.style.paddingTop = 10;
            btn.style.paddingBottom = 10;
            btn.style.borderTopWidth = 2;
            btn.style.borderRightWidth = 2;
            btn.style.borderBottomWidth = 2;
            btn.style.borderLeftWidth = 2;
            btn.style.borderTopLeftRadius = 6;
            btn.style.borderTopRightRadius = 6;
            btn.style.borderBottomLeftRadius = 6;
            btn.style.borderBottomRightRadius = 6;
            btn.style.transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(50));

            // Setup CSS transitions for smooth animations
            btn.style.transitionProperty = new List<StylePropertyName>
            {
                new StylePropertyName("scale"),
                new StylePropertyName("background-color"),
                new StylePropertyName("border-top-color"),
                new StylePropertyName("border-right-color"),
                new StylePropertyName("border-bottom-color"),
                new StylePropertyName("border-left-color"),
                new StylePropertyName("color")
            };

            var icon = new Image();
            icon.style.width = 16;
            icon.style.height = 16;
            icon.style.marginRight = 8;

            var text = new Label(isDisabledButton ? "Disabled Button" : "Interactive Button");
            text.style.fontSize = 13;
            text.style.unityFontStyleAndWeight = FontStyle.Bold;

            btn.Add(icon);
            btn.Add(text);

            if (isDisabledButton)
            {
                m_DisabledPreviewButtonIcon = icon;
                m_DisabledPreviewButtonText = text;
            }
            else
            {
                m_PreviewButtonIcon = icon;
                m_PreviewButtonText = text;

                // Mouse Event Handlers for Live Interactive Hover and Click Scaling
                btn.RegisterCallback<PointerEnterEvent>(evt =>
                {
                    if (m_IsInteractive)
                    {
                        SetButtonState(ButtonState.Highlighted);
                    }
                });

                btn.RegisterCallback<PointerLeaveEvent>(evt =>
                {
                    if (m_IsInteractive)
                    {
                        SetButtonState(ButtonState.Normal);
                    }
                });

                btn.RegisterCallback<PointerDownEvent>(evt =>
                {
                    if (m_IsInteractive)
                    {
                        btn.CapturePointer(evt.pointerId);
                        SetButtonState(ButtonState.Pressed);
                    }
                });

                btn.RegisterCallback<PointerUpEvent>(evt =>
                {
                    if (m_IsInteractive)
                    {
                        if (btn.HasPointerCapture(evt.pointerId))
                        {
                            btn.ReleasePointer(evt.pointerId);
                        }

                        if (btn.worldBound.Contains(evt.position))
                        {
                            SetButtonState(ButtonState.Highlighted);
                        }
                        else
                        {
                            SetButtonState(ButtonState.Normal);
                        }
                    }
                });
            }

            return btn;
        }

        #endregion

        #region Preview Update Logic

        private void SetInteractiveMode(bool interactive)
        {
            m_IsInteractive = interactive;
            if (m_InteractiveToggleButton != null)
            {
                m_InteractiveToggleButton.text = m_IsInteractive ? "Interactive: ON" : "Interactive: OFF";
                m_InteractiveToggleButton.style.backgroundColor = m_IsInteractive
                    ? new Color(0.20f, 0.50f, 0.35f, 1f)
                    : new Color(0.40f, 0.40f, 0.40f, 1f);
            }

            if (m_IsInteractive)
            {
                SetButtonState(ButtonState.Normal);
            }
        }

        private void SetButtonState(ButtonState state)
        {
            m_CurrentButtonState = state;
            UpdateStateButtonVisuals();
            UpdatePreviewButtonVisuals();
        }

        private void UpdateStateButtonVisuals()
        {
            foreach (var kvp in m_StateButtons)
            {
                if (kvp.Key == m_CurrentButtonState)
                {
                    kvp.Value.style.backgroundColor = new Color(0.24f, 0.48f, 0.82f, 1f);
                    kvp.Value.style.color = Color.white;
                    kvp.Value.style.unityFontStyleAndWeight = FontStyle.Bold;
                }
                else
                {
                    kvp.Value.style.backgroundColor = new Color(0.28f, 0.28f, 0.28f, 1f);
                    kvp.Value.style.color = new Color(0.8f, 0.8f, 0.8f, 1f);
                    kvp.Value.style.unityFontStyleAndWeight = FontStyle.Normal;
                }
            }
        }

        private void UpdatePreview()
        {
            if (m_TargetStyle == null || m_PreviewPanel == null) return;

            if (m_SerializedObject != null && m_SerializedObject.targetObject != null)
            {
                m_SerializedObject.ApplyModifiedProperties();
                m_SerializedObject.Update();
            }

            // 1. Panel Colors
            m_PreviewPanel.style.backgroundColor = m_TargetStyle.m_PanelColor;
            m_PreviewPanel.style.borderTopColor = m_TargetStyle.m_PanelBorderColor;
            m_PreviewPanel.style.borderRightColor = m_TargetStyle.m_PanelBorderColor;
            m_PreviewPanel.style.borderBottomColor = m_TargetStyle.m_PanelBorderColor;
            m_PreviewPanel.style.borderLeftColor = m_TargetStyle.m_PanelBorderColor;

            // 2. Regular Text Colors
            m_PreviewLabel.style.color = m_TargetStyle.m_LabelColor;
            m_PreviewHeader1.style.color = m_TargetStyle.m_Header1Color;
            m_PreviewHeader2.style.color = m_TargetStyle.m_Header2Color;
            m_PreviewHeader3.style.color = m_TargetStyle.m_Header3Color;

            // 3. Logo Sprites
            if (m_TargetStyle.m_Logo != null)
            {
                m_PreviewLogoImage.sprite = m_TargetStyle.m_Logo;
                m_PreviewLogoImage.style.display = DisplayStyle.Flex;

                m_PreviewButtonIcon.sprite = m_TargetStyle.m_Logo;
                m_DisabledPreviewButtonIcon.sprite = m_TargetStyle.m_Logo;
            }
            else
            {
                m_PreviewLogoImage.style.display = DisplayStyle.None;

                // Fallback icon for preview buttons if available
                var defaultIcon = EditorGUIUtility.FindTexture("d_PlayButton") ?? EditorGUIUtility.FindTexture("PlayButton");
                if (defaultIcon != null)
                {
                    m_PreviewButtonIcon.image = defaultIcon;
                    m_PreviewButtonIcon.style.display = DisplayStyle.Flex;
                    m_DisabledPreviewButtonIcon.image = defaultIcon;
                    m_DisabledPreviewButtonIcon.style.display = DisplayStyle.Flex;
                }
                else
                {
                    m_PreviewButtonIcon.style.display = DisplayStyle.None;
                    m_DisabledPreviewButtonIcon.style.display = DisplayStyle.None;
                }
            }

            // 4. Update Button Visuals (Interactive Button and Disabled Button)
            UpdatePreviewButtonVisuals();
            UpdateDisabledButtonVisuals();
            ApplyPreviewScale();
        }

        private void ApplyPreviewScale()
        {
            if (m_PreviewPanel != null)
            {
                m_PreviewPanel.style.scale = new Scale(new Vector3(m_PreviewScale, m_PreviewScale, 1f));
                m_PreviewPanel.style.transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(50));
            }
        }

        private void UpdatePreviewButtonVisuals()
        {
            if (m_TargetStyle == null || m_PreviewButton == null) return;

            // Compute colors from ColorBlocks for current state
            Color bgColor = GetEffectiveColor(m_TargetStyle.m_ButtonColorBlock, m_CurrentButtonState);
            Color borderColor = GetEffectiveColor(m_TargetStyle.m_ButtonBorderColorBlock, m_CurrentButtonState);
            Color textColor = GetEffectiveColor(m_TargetStyle.m_ButtonTextColorBlock, m_CurrentButtonState);
            Color iconColor = GetEffectiveColor(m_TargetStyle.m_ButtonIconColorBlock, m_CurrentButtonState);

            // Compute Scale and Animation Duration
            float targetScale = 1.0f;
            float animDuration = m_TargetStyle.m_ButtonBackToNormalDuration;

            switch (m_CurrentButtonState)
            {
                case ButtonState.Highlighted:
                    targetScale = m_TargetStyle.m_ButtonHoverScale;
                    animDuration = m_TargetStyle.m_ButtonHoverAnimationDuration;
                    break;
                case ButtonState.Pressed:
                    targetScale = m_TargetStyle.m_ButtonClickScale;
                    animDuration = m_TargetStyle.m_ButtonClickAnimationDuration;
                    break;
                case ButtonState.Selected:
                    targetScale = m_TargetStyle.m_ButtonClickScale;
                    animDuration = m_TargetStyle.m_ButtonClickAnimationDuration;
                    break;
            }

            // Apply transition durations
            float colorDuration = m_TargetStyle.m_ColorAnimationDuration;
            m_PreviewButton.style.transitionDuration = new List<TimeValue>
            {
                new TimeValue(animDuration, TimeUnit.Second),
                new TimeValue(colorDuration, TimeUnit.Second),
                new TimeValue(colorDuration, TimeUnit.Second),
                new TimeValue(colorDuration, TimeUnit.Second),
                new TimeValue(colorDuration, TimeUnit.Second),
                new TimeValue(colorDuration, TimeUnit.Second),
                new TimeValue(colorDuration, TimeUnit.Second)
            };

            // Apply Scale
            m_PreviewButton.style.scale = new Scale(new Vector3(targetScale, targetScale, 1f));

            // Apply Colors
            m_PreviewButton.style.backgroundColor = bgColor;
            m_PreviewButton.style.borderTopColor = borderColor;
            m_PreviewButton.style.borderRightColor = borderColor;
            m_PreviewButton.style.borderBottomColor = borderColor;
            m_PreviewButton.style.borderLeftColor = borderColor;

            m_PreviewButtonText.style.color = textColor;
            m_PreviewButtonIcon.tintColor = iconColor;

            // Status message
            if (m_StateInfoLabel != null)
            {
                m_StateInfoLabel.text = $"State: {m_CurrentButtonState}  |  Scale: {targetScale:F2}x ({animDuration:F2}s)  |  Color Transition: {colorDuration:F2}s" +
                    (m_IsInteractive ? "  [Interactive Mode]" : "  [Fixed State Preview]");
            }
        }

        private void UpdateDisabledButtonVisuals()
        {
            if (m_TargetStyle == null || m_DisabledPreviewButton == null) return;

            Color bgColor = GetEffectiveColor(m_TargetStyle.m_ButtonColorBlock, ButtonState.Disabled);
            Color borderColor = GetEffectiveColor(m_TargetStyle.m_ButtonBorderColorBlock, ButtonState.Disabled);
            Color textColor = GetEffectiveColor(m_TargetStyle.m_ButtonTextColorBlock, ButtonState.Disabled);
            Color iconColor = GetEffectiveColor(m_TargetStyle.m_ButtonIconColorBlock, ButtonState.Disabled);

            m_DisabledPreviewButton.style.backgroundColor = bgColor;
            m_DisabledPreviewButton.style.borderTopColor = borderColor;
            m_DisabledPreviewButton.style.borderRightColor = borderColor;
            m_DisabledPreviewButton.style.borderBottomColor = borderColor;
            m_DisabledPreviewButton.style.borderLeftColor = borderColor;

            m_DisabledPreviewButtonText.style.color = textColor;
            m_DisabledPreviewButtonIcon.tintColor = iconColor;
        }

        private static Color GetEffectiveColor(ColorBlock block, ButtonState state)
        {
            return state switch
            {
                ButtonState.Normal => block.normalColor,
                ButtonState.Highlighted => block.highlightedColor,
                ButtonState.Pressed => block.pressedColor,
                ButtonState.Selected => block.selectedColor,
                ButtonState.Disabled => block.disabledColor,
                _ => block.normalColor
            };
        }

        #endregion

        #region Background Themes

        private void ToggleBackgroundTheme()
        {
            m_BackgroundThemeIndex = (m_BackgroundThemeIndex + 1) % 3;
            UpdateBackgroundTheme();

            var bgBtn = m_RightPreviewPane.Q<Button>("bgThemeBtn");
            if (bgBtn != null)
            {
                bgBtn.text = m_BackgroundThemeIndex switch
                {
                    0 => "Dark",
                    1 => "Light",
                    2 => "Neutral",
                    _ => "Dark"
                };
            }
        }

        private void UpdateBackgroundTheme()
        {
            if (m_PreviewViewport == null) return;

            switch (m_BackgroundThemeIndex)
            {
                case 0: // Dark
                    m_PreviewViewport.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f, 1f);
                    break;
                case 1: // Light
                    m_PreviewViewport.style.backgroundColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                    break;
                case 2: // Neutral
                    m_PreviewViewport.style.backgroundColor = new Color(0.28f, 0.28f, 0.28f, 1f);
                    break;
            }
        }

        #endregion
    }
}
