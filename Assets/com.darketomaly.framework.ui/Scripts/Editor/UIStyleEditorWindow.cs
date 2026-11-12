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

namespace Framework.UI.Editor
{
    public class UIStyleEditorWindow : EditorWindow
    {
        private UIStyle m_TargetStyle;
        private UIStyle m_LastKnownParentStyle;
        private SerializedObject m_SerializedObject;
        private int m_SelectedSemanticIndex = 0;

        // UI Toolkit Root & Layout
        private DropdownField m_StyleDropdown;
        private List<UIStyle> m_AvailableStyles = new List<UIStyle>();
        private ScrollView m_LeftScrollView;
        private VisualElement m_RightPreviewPane;
        private ScrollView m_PreviewViewport;
        private GridBackground m_GridBackground;
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
        private VisualElement m_PreviewSeparator;

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
        private int m_BackgroundThemeIndex = 0; // 0 = Dark, 1 = Light, 2 = Checkered
        private float m_PreviewScale = 1f;

        // State Buttons
        private Dictionary<ButtonState, Button> m_StateButtons = new Dictionary<ButtonState, Button>();

        [MenuItem("Tools/Framework/UI Style Editor", false, 100)]
        public static void OpenWindow()
        {
            var window = GetWindow<UIStyleEditorWindow>();
            var icon = LoadBrandIcon() ?? EditorGUIUtility.FindTexture("d_StyleSheet Icon") ?? EditorGUIUtility.FindTexture("StyleSheet Icon");
            window.titleContent = new GUIContent("UI Style Editor", icon, "Darketomaly UI Style Editor");
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
            var window = GetWindow<UIStyleEditorWindow>();
            var icon = LoadBrandIcon() ?? EditorGUIUtility.FindTexture("d_StyleSheet Icon") ?? EditorGUIUtility.FindTexture("StyleSheet Icon");
            window.titleContent = new GUIContent("UI Style Editor", icon, "Darketomaly UI Style Editor");
            window.minSize = new Vector2(800, 520);
            window.Show();
            window.SetTarget(style);
        }

        private void OnEnable()
        {
            var icon = LoadBrandIcon() ?? EditorGUIUtility.FindTexture("d_StyleSheet Icon") ?? EditorGUIUtility.FindTexture("StyleSheet Icon");
            titleContent = new GUIContent("UI Style Editor", icon, "Darketomaly UI Style Editor");
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

        private void OnFocus()
        {
            RefreshTopToolbarDropdown();
        }

        private void OnUndoRedo()
        {
            if (m_SerializedObject != null && m_SerializedObject.targetObject != null)
            {
                m_SerializedObject.Update();
                BuildPropertiesUI();
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
            m_LeftScrollView.RegisterCallback<ChangeEvent<UnityEngine.Object>>(evt =>
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

            m_StyleDropdown = new DropdownField();
            m_StyleDropdown.style.width = 240;
            m_StyleDropdown.RegisterValueChangedCallback(evt =>
            {
                if (m_StyleDropdown.choices == null) return;
                int index = m_StyleDropdown.choices.IndexOf(evt.newValue);
                if (m_TargetStyle == null)
                {
                    if (index > 0 && index - 1 < m_AvailableStyles.Count)
                    {
                        SetTarget(m_AvailableStyles[index - 1]);
                    }
                }
                else
                {
                    if (index >= 0 && index < m_AvailableStyles.Count)
                    {
                        SetTarget(m_AvailableStyles[index]);
                    }
                }
            });
            toolbar.Add(m_StyleDropdown);
            RefreshTopToolbarDropdown();

            var newStyleButton = new Button(CreateNewStyleAsset)
            {
                text = "+ New",
                tooltip = "Create a new UI Style asset"
            };
            newStyleButton.style.marginLeft = 4;
            newStyleButton.style.paddingLeft = 8;
            newStyleButton.style.paddingRight = 8;
            newStyleButton.style.backgroundColor = new Color(0.22f, 0.45f, 0.70f, 1f);
            newStyleButton.style.color = Color.white;
            toolbar.Add(newStyleButton);

            return toolbar;
        }

        #region Semantic Variants

        private static UISemantic GetDefaultSemanticAsset()
        {
            var sem = Resources.Load<UISemantic>("Semantics/Default");
            if (sem != null) return sem;

            sem = AssetDatabase.LoadAssetAtPath<UISemantic>("Packages/com.darketomaly.framework.ui/Resources/Semantics/Default.asset");
            if (sem != null) return sem;

            sem = AssetDatabase.LoadAssetAtPath<UISemantic>("Assets/com.darketomaly.framework.ui/Resources/Semantics/Default.asset");
            if (sem != null) return sem;

            string[] guids = AssetDatabase.FindAssets("Default t:UISemantic");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var loaded = AssetDatabase.LoadAssetAtPath<UISemantic>(path);
                if (loaded != null && (loaded.name == "Default" || loaded.Name == "Default"))
                {
                    return loaded;
                }
            }

            guids = AssetDatabase.FindAssets("t:UISemantic");
            foreach (var guid in guids)
            {
                var loaded = AssetDatabase.LoadAssetAtPath<UISemantic>(AssetDatabase.GUIDToAssetPath(guid));
                if (loaded != null) return loaded;
            }

            return null;
        }

        private static void EnsureDefaultSemantic(UIStyle style)
        {
            if (style == null) return;
            if (style.m_StyleSemantics == null)
            {
                style.m_StyleSemantics = new List<StyleSemanticEntry>();
            }

            var defaultSem = GetDefaultSemanticAsset();
            if (style.m_StyleSemantics.Count == 0)
            {
                style.m_StyleSemantics.Add(new StyleSemanticEntry
                {
                    Semantic = defaultSem
                });
                EditorUtility.SetDirty(style);
                return;
            }

            bool hasDefault = false;
            for (int i = 0; i < style.m_StyleSemantics.Count; i++)
            {
                if (style.m_StyleSemantics[i].Semantic == defaultSem)
                {
                    hasDefault = true;
                    if (i != 0)
                    {
                        var entry = style.m_StyleSemantics[i];
                        style.m_StyleSemantics.RemoveAt(i);
                        style.m_StyleSemantics.Insert(0, entry);
                        EditorUtility.SetDirty(style);
                    }
                    break;
                }
            }

            if (!hasDefault)
            {
                if (style.m_StyleSemantics[0].Semantic == null)
                {
                    style.m_StyleSemantics[0].Semantic = defaultSem;
                }
                else
                {
                    style.m_StyleSemantics.Insert(0, new StyleSemanticEntry { Semantic = defaultSem });
                }
                EditorUtility.SetDirty(style);
            }
        }

        private static List<UISemantic> FindAllSemanticAssets()
        {
            var list = new List<UISemantic>();
            var set = new HashSet<UISemantic>();

            var guids = AssetDatabase.FindAssets("t:UISemantic");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var sem = AssetDatabase.LoadAssetAtPath<UISemantic>(path);
                if (sem != null && set.Add(sem))
                {
                    list.Add(sem);
                }
            }

            return list;
        }

        private SerializedProperty GetSelectedSemanticEntryProp()
        {
            if (m_SerializedObject == null) return null;
            var semanticsProp = m_SerializedObject.FindProperty("m_StyleSemantics");
            if (semanticsProp == null || semanticsProp.arraySize == 0) return null;

            int idx = Mathf.Clamp(m_SelectedSemanticIndex, 0, semanticsProp.arraySize - 1);
            return semanticsProp.GetArrayElementAtIndex(idx);
        }

        private UISemantic GetSelectedSemantic()
        {
            if (m_TargetStyle == null || m_TargetStyle.m_StyleSemantics == null || m_TargetStyle.m_StyleSemantics.Count == 0)
            {
                return GetDefaultSemanticAsset();
            }

            if (m_SelectedSemanticIndex < 0 || m_SelectedSemanticIndex >= m_TargetStyle.m_StyleSemantics.Count)
            {
                m_SelectedSemanticIndex = 0;
            }

            return m_TargetStyle.m_StyleSemantics[m_SelectedSemanticIndex].Semantic ?? GetDefaultSemanticAsset();
        }

        private void AddSemanticManagementUI(VisualElement parent, SerializedProperty semanticsProp)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 0;

            var choices = new List<string>();
            for (int i = 0; i < semanticsProp.arraySize; i++)
            {
                var entry = semanticsProp.GetArrayElementAtIndex(i);
                var sem = entry.FindPropertyRelative("Semantic").objectReferenceValue as UISemantic;
                string semName = sem != null ? sem.name : $"(Unassigned {i})";
                if (i == 0) semName += " [Default]";
                choices.Add(semName);
            }

            int activeIndex = Mathf.Clamp(m_SelectedSemanticIndex, 0, Mathf.Max(0, choices.Count - 1));
            var semanticDropdown = new DropdownField("Active Variant", choices, activeIndex);
            semanticDropdown.style.flexGrow = 1;
            semanticDropdown.style.marginLeft = 0;
            semanticDropdown.style.marginBottom = 0;

            semanticDropdown.RegisterValueChangedCallback(evt =>
            {
                int idx = semanticDropdown.choices.IndexOf(evt.newValue);
                if (idx >= 0 && idx < semanticsProp.arraySize && idx != m_SelectedSemanticIndex)
                {
                    m_SelectedSemanticIndex = idx;
                    BuildPropertiesUI();
                    UpdatePreview();
                }
            });
            row.Add(semanticDropdown);

            var addBtn = new Button(ShowAddSemanticMenu)
            {
                text = "+ Add",
                tooltip = "Add a new or existing semantic variant to this style"
            };
            addBtn.style.marginLeft = 4;
            addBtn.style.paddingLeft = 8;
            addBtn.style.paddingRight = 8;
            addBtn.style.backgroundColor = new Color(0.22f, 0.45f, 0.70f, 1f);
            addBtn.style.color = Color.white;
            row.Add(addBtn);

            if (m_SelectedSemanticIndex > 0)
            {
                var removeBtn = new Button(RemoveSelectedSemantic)
                {
                    text = "Remove",
                    tooltip = "Remove this semantic variant from this style"
                };
                removeBtn.style.marginLeft = 4;
                removeBtn.style.paddingLeft = 8;
                removeBtn.style.paddingRight = 8;
                removeBtn.style.backgroundColor = new Color(0.65f, 0.22f, 0.22f, 1f);
                removeBtn.style.color = Color.white;
                row.Add(removeBtn);
            }

            parent.Add(row);
        }

        private void ShowAddSemanticMenu()
        {
            if (m_TargetStyle == null) return;

            var menu = new GenericMenu();
            var allSemantics = FindAllSemanticAssets();
            var existingSemantics = new HashSet<UISemantic>();

            if (m_TargetStyle.m_StyleSemantics != null)
            {
                foreach (var entry in m_TargetStyle.m_StyleSemantics)
                {
                    if (entry.Semantic != null)
                    {
                        existingSemantics.Add(entry.Semantic);
                    }
                }
            }

            bool hasCandidates = false;
            foreach (var sem in allSemantics)
            {
                if (!existingSemantics.Contains(sem))
                {
                    hasCandidates = true;
                    menu.AddItem(new GUIContent($"Existing/{sem.name}"), false, () => AddSemanticToTargetStyle(sem));
                }
            }

            if (!hasCandidates)
            {
                menu.AddDisabledItem(new GUIContent("Existing/No unassigned semantics found"));
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Create New Semantic..."), false, CreateAndAddSemantic);
            menu.ShowAsContext();
        }

        private static StyleSemanticEntry CloneSemanticEntry(StyleSemanticEntry source, UISemantic newSemantic)
        {
            if (source == null) return new StyleSemanticEntry { Semantic = newSemantic };

            var copy = new StyleSemanticEntry { Semantic = newSemantic };
            var fields = typeof(StyleSemanticEntry).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.Name == "Semantic") continue;

                var srcVal = field.GetValue(source);
                if (srcVal == null) continue;

                var overridableType = srcVal.GetType();
                var destOverridable = System.Activator.CreateInstance(overridableType);

                var isOverriddenField = overridableType.GetField("IsOverriden", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?? overridableType.GetField("IsOverridden", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var valField = overridableType.GetField("Value", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (isOverriddenField != null)
                {
                    isOverriddenField.SetValue(destOverridable, isOverriddenField.GetValue(srcVal));
                }

                if (valField != null)
                {
                    valField.SetValue(destOverridable, valField.GetValue(srcVal));
                }

                field.SetValue(copy, destOverridable);
            }

            return copy;
        }

        private void AddSemanticToTargetStyle(UISemantic semantic)
        {
            if (m_TargetStyle == null || semantic == null) return;

            Undo.RecordObject(m_TargetStyle, "Add Semantic Variant");

            if (m_TargetStyle.m_StyleSemantics == null)
            {
                m_TargetStyle.m_StyleSemantics = new List<StyleSemanticEntry>();
            }

            StyleSemanticEntry newEntry;
            if (m_TargetStyle.m_StyleSemantics.Count > 0 && m_TargetStyle.m_StyleSemantics[0] != null)
            {
                newEntry = CloneSemanticEntry(m_TargetStyle.m_StyleSemantics[0], semantic);
            }
            else
            {
                newEntry = new StyleSemanticEntry
                {
                    Semantic = semantic
                };
            }

            m_TargetStyle.m_StyleSemantics.Add(newEntry);
            EditorUtility.SetDirty(m_TargetStyle);
            AssetDatabase.SaveAssets();

            m_SerializedObject.Update();
            m_SelectedSemanticIndex = m_TargetStyle.m_StyleSemantics.Count - 1;
            BuildPropertiesUI();
            UpdatePreview();
        }

        private void CreateAndAddSemantic()
        {
            string folder = "Assets/Resources/Semantics";
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Semantics"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Semantics");
            }

            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/New Semantic.asset");
            var newSemantic = CreateInstance<UISemantic>();
            newSemantic.Name = System.IO.Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(newSemantic, path);
            AssetDatabase.SaveAssets();

            AddSemanticToTargetStyle(newSemantic);
            Selection.activeObject = newSemantic;
            EditorGUIUtility.PingObject(newSemantic);
        }

        private void RemoveSelectedSemantic()
        {
            if (m_TargetStyle == null || m_SelectedSemanticIndex <= 0) return;
            if (m_TargetStyle.m_StyleSemantics == null || m_SelectedSemanticIndex >= m_TargetStyle.m_StyleSemantics.Count) return;

            var sem = m_TargetStyle.m_StyleSemantics[m_SelectedSemanticIndex].Semantic;
            string semName = sem != null ? sem.name : "this variant";

            if (EditorUtility.DisplayDialog("Remove Semantic Variant", $"Are you sure you want to remove '{semName}' from {m_TargetStyle.name}?", "Remove", "Cancel"))
            {
                Undo.RecordObject(m_TargetStyle, "Remove Semantic Variant");
                m_TargetStyle.m_StyleSemantics.RemoveAt(m_SelectedSemanticIndex);
                m_SelectedSemanticIndex = Mathf.Clamp(m_SelectedSemanticIndex - 1, 0, Mathf.Max(0, m_TargetStyle.m_StyleSemantics.Count - 1));
                EditorUtility.SetDirty(m_TargetStyle);
                AssetDatabase.SaveAssets();

                m_SerializedObject.Update();
                BuildPropertiesUI();
                UpdatePreview();
            }
        }

        #endregion

        private void CreateNewStyleAsset()
        {
            string folder = "Assets";
            if (m_TargetStyle != null)
            {
                string targetPath = AssetDatabase.GetAssetPath(m_TargetStyle);
                if (!string.IsNullOrEmpty(targetPath))
                {
                    folder = System.IO.Path.GetDirectoryName(targetPath)?.Replace('\\', '/');
                }
            }
            else
            {
                var existing = FindAllStyleAssets();
                if (existing != null && existing.Count > 0)
                {
                    string existingPath = AssetDatabase.GetAssetPath(existing[0]);
                    if (!string.IsNullOrEmpty(existingPath))
                    {
                        folder = System.IO.Path.GetDirectoryName(existingPath)?.Replace('\\', '/');
                    }
                }
                else if (AssetDatabase.IsValidFolder("Assets/Resources/Styles"))
                {
                    folder = "Assets/Resources/Styles";
                }
            }

            if (string.IsNullOrEmpty(folder)) folder = "Assets";

            string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/New UI Style.asset");

            var newStyle = CreateInstance<UIStyle>();
            var defaultSem = GetDefaultSemanticAsset();
            newStyle.m_StyleSemantics = new List<StyleSemanticEntry>
            {
                new StyleSemanticEntry { Semantic = defaultSem }
            };
            AssetDatabase.CreateAsset(newStyle, path);

            var so = new SerializedObject(newStyle);
            var semanticsProp = so.FindProperty("m_StyleSemantics");
            var entryProp = semanticsProp != null && semanticsProp.arraySize > 0
                ? semanticsProp.GetArrayElementAtIndex(0)
                : null;

            void SetColor(string name, Color color)
            {
                var prop = entryProp?.FindPropertyRelative(name) ?? so.FindProperty(name);
                if (prop == null) return;
                var isOverridden = prop.FindPropertyRelative("IsOverriden") ?? prop.FindPropertyRelative("IsOverridden");
                if (isOverridden != null) isOverridden.boolValue = true;
                var val = prop.FindPropertyRelative("Value") ?? prop;
                val.colorValue = color;
            }

            void SetFloat(string name, float value)
            {
                var prop = entryProp?.FindPropertyRelative(name) ?? so.FindProperty(name);
                if (prop == null) return;
                var isOverridden = prop.FindPropertyRelative("IsOverriden") ?? prop.FindPropertyRelative("IsOverridden");
                if (isOverridden != null) isOverridden.boolValue = true;
                var val = prop.FindPropertyRelative("Value") ?? prop;
                val.floatValue = value;
            }

            void SetColorBlock(string name, Color normal, Color highlighted, Color pressed, Color selected, Color disabled)
            {
                var prop = entryProp?.FindPropertyRelative(name) ?? so.FindProperty(name);
                if (prop == null) return;
                var isOverridden = prop.FindPropertyRelative("IsOverriden") ?? prop.FindPropertyRelative("IsOverridden");
                if (isOverridden != null) isOverridden.boolValue = true;
                var val = prop.FindPropertyRelative("Value") ?? prop;
                var n = val.FindPropertyRelative("m_NormalColor"); if (n != null) n.colorValue = normal;
                var h = val.FindPropertyRelative("m_HighlightedColor"); if (h != null) h.colorValue = highlighted;
                var p = val.FindPropertyRelative("m_PressedColor"); if (p != null) p.colorValue = pressed;
                var s = val.FindPropertyRelative("m_SelectedColor"); if (s != null) s.colorValue = selected;
                var d = val.FindPropertyRelative("m_DisabledColor"); if (d != null) d.colorValue = disabled;
                var m = val.FindPropertyRelative("m_ColorMultiplier"); if (m != null) m.floatValue = 1f;
                var f = val.FindPropertyRelative("m_FadeDuration"); if (f != null) f.floatValue = 0.2f;
            }

            SetColor("m_LabelColor", new Color(0.85f, 0.85f, 0.85f, 1f));
            SetColor("m_Header1Color", new Color(1f, 1f, 1f, 1f));
            SetColor("m_Header2Color", new Color(0.88f, 0.88f, 0.88f, 1f));
            SetColor("m_Header3Color", new Color(0.75f, 0.75f, 0.75f, 1f));
            SetColor("m_PanelColor", new Color(0.18f, 0.22f, 0.28f, 1f));
            SetColor("m_PanelBorderColor", new Color(0.28f, 0.52f, 0.80f, 1f));
            SetColor("m_PanelSeparatorColor", new Color(0.28f, 0.52f, 0.80f, 0.4f));

            SetColorBlock("m_ButtonColorBlock",
                new Color(0.24f, 0.42f, 0.65f, 1f),
                new Color(0.32f, 0.55f, 0.85f, 1f),
                new Color(0.18f, 0.32f, 0.52f, 1f),
                new Color(0.30f, 0.50f, 0.80f, 1f),
                new Color(0.25f, 0.25f, 0.25f, 0.6f));

            SetColorBlock("m_ButtonBorderColorBlock",
                new Color(0.40f, 0.65f, 0.95f, 1f),
                new Color(0.60f, 0.80f, 1f, 1f),
                new Color(0.25f, 0.45f, 0.75f, 1f),
                new Color(0.50f, 0.75f, 1f, 1f),
                new Color(0.35f, 0.35f, 0.35f, 0.4f));

            SetColorBlock("m_ButtonTextColorBlock",
                Color.white,
                Color.white,
                new Color(0.9f, 0.9f, 0.9f, 1f),
                Color.white,
                new Color(0.6f, 0.6f, 0.6f, 1f));

            SetColorBlock("m_ButtonIconColorBlock",
                Color.white,
                Color.white,
                new Color(0.9f, 0.9f, 0.9f, 1f),
                Color.white,
                new Color(0.6f, 0.6f, 0.6f, 1f));

            SetFloat("m_ButtonHoverScale", 1.05f);
            SetFloat("m_ButtonClickScale", 0.95f);
            SetFloat("m_ButtonHoverAnimationDuration", 0.2f);
            SetFloat("m_ButtonClickAnimationDuration", 0.1f);
            SetFloat("m_ButtonBackToNormalDuration", 0.2f);
            SetFloat("m_ColorAnimationDuration", 0.2f);

            SetFloat("m_LabelFontSize", 12f);
            SetFloat("m_Header1FontSize", 18f);
            SetFloat("m_Header2FontSize", 14f);
            SetFloat("m_Header3FontSize", 12f);

            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            SetTarget(newStyle);
            Selection.activeObject = newStyle;
            EditorGUIUtility.PingObject(newStyle);
        }

        public void SetTarget(UIStyle style)
        {
            m_TargetStyle = style;
            RefreshTopToolbarDropdown();

            if (m_TargetStyle == null)
            {
                m_LastKnownParentStyle = null;
                m_SelectedSemanticIndex = 0;
                ShowEmptyState();
            }
            else
            {
                EnsureDefaultSemantic(m_TargetStyle);
                m_SelectedSemanticIndex = 0;
                m_SerializedObject = new SerializedObject(m_TargetStyle);
                m_LastKnownParentStyle = GetParentStyle();
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

            EnsureDefaultSemantic(m_TargetStyle);
            m_SerializedObject.Update();

            var semanticsProp = m_SerializedObject.FindProperty("m_StyleSemantics");
            if (semanticsProp == null || semanticsProp.arraySize == 0)
            {
                EnsureDefaultSemantic(m_TargetStyle);
                m_SerializedObject.Update();
                semanticsProp = m_SerializedObject.FindProperty("m_StyleSemantics");
            }

            if (m_SelectedSemanticIndex < 0 || (semanticsProp != null && m_SelectedSemanticIndex >= semanticsProp.arraySize))
            {
                m_SelectedSemanticIndex = 0;
            }

            var currentEntryProp = semanticsProp != null && semanticsProp.arraySize > 0
                ? semanticsProp.GetArrayElementAtIndex(m_SelectedSemanticIndex)
                : null;

            // Section 0: Hierarchy & Inheritance
            var inheritanceCard = CreateSectionCard("Inheritance", new Color(0.20f, 0.70f, 0.85f, 1f));
            AddParentStyleField(inheritanceCard);
            m_LeftScrollView.Add(inheritanceCard);

            // Section 0.5: Semantic Variants
            if (semanticsProp != null)
            {
                var semanticCard = CreateSectionCard("Semantic Variants", new Color(0.35f, 0.80f, 0.35f, 1f));
                AddSemanticManagementUI(semanticCard, semanticsProp);
                m_LeftScrollView.Add(semanticCard);
            }

            // Section 1: Sprites
            var spritesCard = CreateSectionCard("Sprites & Branding", new Color(0.25f, 0.60f, 0.95f, 1f));
            AddPropertyField(spritesCard, currentEntryProp, "m_Logo", "Logo Sprite");
            m_LeftScrollView.Add(spritesCard);

            // Section 2: Regular Colors
            var regularColorsCard = CreateSectionCard("Regular Colors", new Color(0.70f, 0.40f, 0.95f, 1f));
            AddPropertyField(regularColorsCard, currentEntryProp, "m_LabelColor", "Label Color");
            AddPropertyField(regularColorsCard, currentEntryProp, "m_Header1Color", "Header 1 Color");
            AddPropertyField(regularColorsCard, currentEntryProp, "m_Header2Color", "Header 2 Color");
            AddPropertyField(regularColorsCard, currentEntryProp, "m_Header3Color", "Header 3 Color");
            AddPropertyField(regularColorsCard, currentEntryProp, "m_PanelColor", "Panel Color");
            AddPropertyField(regularColorsCard, currentEntryProp, "m_PanelBorderColor", "Panel Border Color");
            AddPropertyField(regularColorsCard, currentEntryProp, "m_PanelSeparatorColor", "Panel Separator Color");
            m_LeftScrollView.Add(regularColorsCard);

            // Section 3: Button Color Blocks
            var colorBlocksCard = CreateSectionCard("Button Color Blocks", new Color(0.95f, 0.65f, 0.20f, 1f));
            AddColorBlockField(colorBlocksCard, currentEntryProp, "m_ButtonColorBlock", "Button Background Colors");
            AddColorBlockField(colorBlocksCard, currentEntryProp, "m_ButtonBorderColorBlock", "Button Border Colors");
            AddColorBlockField(colorBlocksCard, currentEntryProp, "m_ButtonTextColorBlock", "Button Text Colors");
            AddColorBlockField(colorBlocksCard, currentEntryProp, "m_ButtonIconColorBlock", "Button Icon Colors");
            m_LeftScrollView.Add(colorBlocksCard);

            // Section 4: Animations
            var animCard = CreateSectionCard("Animations & Scale", new Color(0.20f, 0.80f, 0.50f, 1f));
            AddPropertyField(animCard, currentEntryProp, "m_ButtonHoverScale", "Hover Scale");
            AddPropertyField(animCard, currentEntryProp, "m_ButtonClickScale", "Click Scale");
            AddPropertyField(animCard, currentEntryProp, "m_ButtonHoverAnimationDuration", "Hover Animation Duration (s)");
            AddPropertyField(animCard, currentEntryProp, "m_ButtonClickAnimationDuration", "Click Animation Duration (s)");
            AddPropertyField(animCard, currentEntryProp, "m_ButtonBackToNormalDuration", "Return To Normal Duration (s)");
            AddPropertyField(animCard, currentEntryProp, "m_ColorAnimationDuration", "Color Transition Duration (s)");
            m_LeftScrollView.Add(animCard);

            // Section 5: Text Sizes
            var textSizesCard = CreateSectionCard("Text Sizes", new Color(0.95f, 0.40f, 0.55f, 1f));
            AddPropertyField(textSizesCard, currentEntryProp, "m_LabelFontSize", "Label Font Size");
            AddPropertyField(textSizesCard, currentEntryProp, "m_Header1FontSize", "Header 1 Font Size");
            AddPropertyField(textSizesCard, currentEntryProp, "m_Header2FontSize", "Header 2 Font Size");
            AddPropertyField(textSizesCard, currentEntryProp, "m_Header3FontSize", "Header 3 Font Size");
            m_LeftScrollView.Add(textSizesCard);

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

        private UIStyle GetParentStyle()
        {
            var parentProp = m_SerializedObject?.FindProperty("m_ParentStyle");
            return parentProp?.objectReferenceValue as UIStyle;
        }

        #region Style Asset Lookup & Inheritance Helpers

        private static List<UIStyle> FindAllStyleAssets()
        {
            var list = new List<UIStyle>();
            var set = new HashSet<UIStyle>();

            var guids = AssetDatabase.FindAssets("t:UIStyle");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var style = AssetDatabase.LoadAssetAtPath<UIStyle>(path);
                if (style != null && set.Add(style))
                {
                    list.Add(style);
                }
            }

            if (list.Count == 0)
            {
                var resStyles = Resources.LoadAll<UIStyle>("");
                if (resStyles != null)
                {
                    foreach (var s in resStyles)
                    {
                        if (s != null && set.Add(s)) list.Add(s);
                    }
                }
            }

            list.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
            return list;
        }

        private static string GetStyleDisplayName(UIStyle style, List<UIStyle> allStyles)
        {
            if (style == null) return "None";
            bool duplicate = false;
            for (int i = 0; i < allStyles.Count; i++)
            {
                if (allStyles[i] != style && allStyles[i].name == style.name)
                {
                    duplicate = true;
                    break;
                }
            }

            if (!duplicate) return style.name;

            string path = AssetDatabase.GetAssetPath(style);
            return $"{style.name} ({path})";
        }

        private static bool WouldCauseCircularInheritance(UIStyle candidate, UIStyle target)
        {
            if (candidate == null || target == null) return false;
            if (candidate == target) return true;

            var visited = new HashSet<UIStyle> { candidate };
            var current = candidate;

            while (current != null)
            {
                var so = new SerializedObject(current);
                var parent = so.FindProperty("m_ParentStyle")?.objectReferenceValue as UIStyle;
                if (parent == target) return true;
                if (parent == null || !visited.Add(parent)) break;
                current = parent;
            }

            return false;
        }

        private void RefreshTopToolbarDropdown()
        {
            if (m_StyleDropdown == null) return;

            m_AvailableStyles = FindAllStyleAssets();
            var choices = new List<string>();

            if (m_AvailableStyles.Count == 0)
            {
                choices.Add("(No UI Styles in Project)");
                m_StyleDropdown.choices = choices;
                m_StyleDropdown.SetValueWithoutNotify("(No UI Styles in Project)");
                return;
            }

            int selectedIndex = -1;
            for (int i = 0; i < m_AvailableStyles.Count; i++)
            {
                var style = m_AvailableStyles[i];
                string displayName = GetStyleDisplayName(style, m_AvailableStyles);
                choices.Add(displayName);
                if (style == m_TargetStyle)
                {
                    selectedIndex = i;
                }
            }

            if (m_TargetStyle == null)
            {
                choices.Insert(0, "(Select a UI Style...)");
                m_StyleDropdown.choices = choices;
                m_StyleDropdown.SetValueWithoutNotify("(Select a UI Style...)");
            }
            else
            {
                m_StyleDropdown.choices = choices;
                if (selectedIndex >= 0)
                {
                    m_StyleDropdown.SetValueWithoutNotify(choices[selectedIndex]);
                }
                else
                {
                    m_StyleDropdown.SetValueWithoutNotify(m_TargetStyle.name);
                }
            }
        }

        private void AddParentStyleField(VisualElement parent)
        {
            var allStyles = FindAllStyleAssets();
            var candidates = new List<UIStyle>();

            foreach (var style in allStyles)
            {
                if (style != m_TargetStyle && !WouldCauseCircularInheritance(style, m_TargetStyle))
                {
                    candidates.Add(style);
                }
            }

            var choices = new List<string> { "None (Root Style)" };
            foreach (var cand in candidates)
            {
                choices.Add(GetStyleDisplayName(cand, allStyles));
            }

            var currentParent = GetParentStyle();
            int selectedIndex = 0;
            if (currentParent != null)
            {
                int candIdx = candidates.IndexOf(currentParent);
                if (candIdx >= 0)
                {
                    selectedIndex = candIdx + 1;
                }
            }

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 4;

            var parentDropdown = new DropdownField("Parent Style", choices, selectedIndex);
            parentDropdown.style.flexGrow = 1;
            parentDropdown.style.marginLeft = 0;
            parentDropdown.style.marginBottom = 0;

            parentDropdown.RegisterValueChangedCallback(evt =>
            {
                int idx = parentDropdown.choices.IndexOf(evt.newValue);
                UIStyle newParent = (idx > 0 && idx - 1 < candidates.Count) ? candidates[idx - 1] : null;

                var parentProp = m_SerializedObject.FindProperty("m_ParentStyle");
                if (parentProp != null)
                {
                    parentProp.objectReferenceValue = newParent;
                    m_SerializedObject.ApplyModifiedProperties();
                    m_SerializedObject.Update();
                    UpdatePreview();
                }
            });

            row.Add(parentDropdown);
            parent.Add(row);
        }

        #endregion

        private void AddPropertyField(VisualElement parent, SerializedProperty entryProp, string propertyPath, string customLabel = null)
        {
            var prop = entryProp != null ? entryProp.FindPropertyRelative(propertyPath) : m_SerializedObject.FindProperty(propertyPath);
            if (prop == null) return;

            var isOverriddenProp = prop.FindPropertyRelative("IsOverriden") ?? prop.FindPropertyRelative("IsOverridden");
            var valueProp = prop.FindPropertyRelative("Value") ?? prop;

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 4;

            Toggle toggle = null;
            if (isOverriddenProp != null)
            {
                toggle = new Toggle
                {
                    bindingPath = isOverriddenProp.propertyPath,
                    tooltip = "Override"
                };
                toggle.style.marginLeft = 0;
                toggle.style.marginRight = 2;
                toggle.style.marginTop = 0;
                toggle.style.marginBottom = 0;
                toggle.style.paddingLeft = 0;
                toggle.style.paddingRight = 0;
                toggle.style.flexShrink = 0;
                toggle.style.display = GetParentStyle() != null ? DisplayStyle.Flex : DisplayStyle.None;
                toggle.RegisterCallback<AttachToPanelEvent>(_ =>
                {
                    var tl = toggle.Q<Label>();
                    if (tl != null) tl.style.display = DisplayStyle.None;
                });
                row.Add(toggle);
            }

            var propField = new PropertyField(valueProp, customLabel ?? prop.displayName);
            propField.style.flexGrow = 1;
            propField.style.marginLeft = 0;
            propField.style.marginBottom = 0;
            propField.RegisterValueChangeCallback(evt => UpdatePreview());
            row.Add(propField);

            if (toggle != null)
            {
                bool? lastApplied = null;
                void ApplyOverrideState(bool isOverridden, bool force = false)
                {
                    bool hasParent = GetParentStyle() != null;
                    toggle.style.display = hasParent ? DisplayStyle.Flex : DisplayStyle.None;

                    bool isEditable = !hasParent || isOverridden;
                    propField.SetEnabled(isEditable);

                    if (!force && lastApplied == isOverridden && hasParent) return;
                    lastApplied = isOverridden;

                    var label = propField.Q<Label>();
                    if (label != null)
                    {
                        label.style.color = isEditable
                            ? new Color(0.85f, 0.85f, 0.85f, 1f)
                            : new Color(0.48f, 0.48f, 0.48f, 1f);
                    }
                }

                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (isOverriddenProp != null)
                    {
                        isOverriddenProp.boolValue = evt.newValue;
                        m_SerializedObject.ApplyModifiedProperties();
                    }
                    ApplyOverrideState(evt.newValue, force: true);
                    UpdatePreview();
                });

                propField.RegisterCallback<AttachToPanelEvent>(_ =>
                {
                    propField.schedule.Execute(() => ApplyOverrideState(toggle.value, force: true));
                });

                propField.RegisterCallback<GeometryChangedEvent>(_ =>
                {
                    if (lastApplied == null)
                    {
                        ApplyOverrideState(toggle.value, force: true);
                    }
                });
            }

            parent.Add(row);
        }

        private void AddColorBlockField(VisualElement parent, SerializedProperty entryProp, string propertyPath, string customLabel)
        {
            var blockProp = entryProp != null ? entryProp.FindPropertyRelative(propertyPath) : m_SerializedObject.FindProperty(propertyPath);
            if (blockProp == null) return;

            var isOverriddenProp = blockProp.FindPropertyRelative("IsOverriden") ?? blockProp.FindPropertyRelative("IsOverridden");
            var targetBlockProp = blockProp.FindPropertyRelative("Value") ?? blockProp;

            var container = new VisualElement();
            container.style.marginBottom = 8;
            container.style.backgroundColor = new Color(0.20f, 0.20f, 0.20f, 0.5f);
            container.style.paddingLeft = 4;
            container.style.paddingRight = 6;
            container.style.paddingTop = 4;
            container.style.paddingBottom = 6;
            container.style.borderTopLeftRadius = 4;
            container.style.borderTopRightRadius = 4;
            container.style.borderBottomLeftRadius = 4;
            container.style.borderBottomRightRadius = 4;

            var headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.FlexStart;

            Toggle toggle = null;
            if (isOverriddenProp != null)
            {
                toggle = new Toggle
                {
                    bindingPath = isOverriddenProp.propertyPath,
                    tooltip = "Override"
                };
                toggle.style.marginLeft = 4;
                toggle.style.marginRight = 2;
                toggle.style.marginTop = 2;
                toggle.style.marginBottom = 0;
                toggle.style.paddingLeft = 0;
                toggle.style.paddingRight = 0;
                toggle.style.flexShrink = 0;
                toggle.style.display = GetParentStyle() != null ? DisplayStyle.Flex : DisplayStyle.None;
                toggle.RegisterCallback<AttachToPanelEvent>(_ =>
                {
                    var tl = toggle.Q<Label>();
                    if (tl != null) tl.style.display = DisplayStyle.None;
                });
                headerRow.Add(toggle);
            }

            var foldout = new Foldout();
            foldout.text = customLabel;
            foldout.value = true;
            foldout.style.flexGrow = 1;
            foldout.style.marginLeft = 0;
            foldout.style.paddingLeft = 0;
            headerRow.Add(foldout);
            container.Add(headerRow);

            if (toggle != null)
            {
                void ApplyBlockOverrideState(bool isOverridden)
                {
                    bool hasParent = GetParentStyle() != null;
                    toggle.style.display = hasParent ? DisplayStyle.Flex : DisplayStyle.None;

                    bool isEditable = !hasParent || isOverridden;
                    foldout.contentContainer.SetEnabled(isEditable);

                    var fText = foldout.Q<TextElement>() ?? foldout.Q<Label>();
                    if (fText != null)
                    {
                        fText.style.color = isEditable
                            ? new Color(0.85f, 0.85f, 0.85f, 1f)
                            : new Color(0.48f, 0.48f, 0.48f, 1f);
                    }
                }

                toggle.RegisterValueChangedCallback(evt =>
                {
                    if (isOverriddenProp != null)
                    {
                        isOverriddenProp.boolValue = evt.newValue;
                        m_SerializedObject.ApplyModifiedProperties();
                    }
                    ApplyBlockOverrideState(evt.newValue);
                    UpdatePreview();
                });

                foldout.RegisterCallback<AttachToPanelEvent>(_ =>
                {
                    foldout.schedule.Execute(() => ApplyBlockOverrideState(toggle.value));
                });
            }

            var normalProp = targetBlockProp.FindPropertyRelative("m_NormalColor");
            var highlightedProp = targetBlockProp.FindPropertyRelative("m_HighlightedColor");
            var pressedProp = targetBlockProp.FindPropertyRelative("m_PressedColor");
            var selectedProp = targetBlockProp.FindPropertyRelative("m_SelectedColor");
            var disabledProp = targetBlockProp.FindPropertyRelative("m_DisabledColor");

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

            parent.Add(container);
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

            // Background Theme Toggle
            var bgLabel = new Label("Canvas:");
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

            // Canvas Container with Grid Background
            var canvasContainer = new VisualElement();
            canvasContainer.style.flexGrow = 1;
            canvasContainer.style.position = Position.Relative;
            canvasContainer.style.overflow = Overflow.Hidden;

            m_GridBackground = new GridBackground();
            m_GridBackground.style.position = Position.Absolute;
            m_GridBackground.style.left = 0;
            m_GridBackground.style.top = 0;
            m_GridBackground.style.right = 0;
            m_GridBackground.style.bottom = 0;
            m_GridBackground.pickingMode = PickingMode.Ignore;
            canvasContainer.Add(m_GridBackground);

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

            // Mouse wheel zoom (clamped between 0.6x and 1.4x)
            m_PreviewViewport.RegisterCallback<WheelEvent>(evt =>
            {
                float step = Mathf.Abs(evt.delta.y) > 1f ? Mathf.Sign(evt.delta.y) * 0.05f : evt.delta.y * 0.05f;
                m_PreviewScale = Mathf.Clamp(m_PreviewScale - step, 0.6f, 1.4f);
                ApplyPreviewScale();
                evt.StopPropagation();
            });

            // Preview Panel inside Viewport
            m_PreviewPanel = CreatePreviewPanel();
            m_PreviewViewport.Add(m_PreviewPanel);

            canvasContainer.Add(m_PreviewViewport);
            pane.Add(canvasContainer);

            UpdateBackgroundTheme();

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

            m_StateInfoLabel = new Label("State: Normal | Hover / Click button for live animations | Mouse wheel to zoom.");
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
            m_PreviewLogoImage.scaleMode = ScaleMode.ScaleToFit;
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

            // Divider / Separator Line
            m_PreviewSeparator = new VisualElement();
            m_PreviewSeparator.style.height = 1;
            m_PreviewSeparator.style.marginBottom = 16;
            panel.Add(m_PreviewSeparator);

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
                    SetButtonState(ButtonState.Highlighted);
                });

                btn.RegisterCallback<PointerLeaveEvent>(evt =>
                {
                    SetButtonState(ButtonState.Normal);
                });

                btn.RegisterCallback<PointerDownEvent>(evt =>
                {
                    btn.CapturePointer(evt.pointerId);
                    SetButtonState(ButtonState.Pressed);
                });

                btn.RegisterCallback<PointerUpEvent>(evt =>
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
                });
            }

            return btn;
        }

        #endregion

        #region Preview Update Logic

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

        private Color GetResolvedColor(System.Func<Color> getter, string propertyPath, Color fallback = default)
        {
            try
            {
                if (m_TargetStyle != null)
                {
                    return getter();
                }
            }
            catch
            {
                // Fall back to serialized object if runtime property evaluation fails
            }

            var entryProp = GetSelectedSemanticEntryProp();
            var prop = entryProp?.FindPropertyRelative(propertyPath) ?? m_SerializedObject?.FindProperty(propertyPath);
            var val = prop?.FindPropertyRelative("Value") ?? prop;
            return val != null ? val.colorValue : fallback;
        }

        private float GetResolvedFloat(System.Func<float> getter, string propertyPath, float fallback = 1.0f)
        {
            try
            {
                if (m_TargetStyle != null)
                {
                    return getter();
                }
            }
            catch
            {
                // Fall back to serialized object if runtime property evaluation fails
            }

            var entryProp = GetSelectedSemanticEntryProp();
            var prop = entryProp?.FindPropertyRelative(propertyPath) ?? m_SerializedObject?.FindProperty(propertyPath);
            var val = prop?.FindPropertyRelative("Value") ?? prop;
            return val != null ? val.floatValue : fallback;
        }

        private ColorBlock GetResolvedColorBlock(System.Func<ColorBlock> getter, string propertyPath)
        {
            try
            {
                if (m_TargetStyle != null)
                {
                    return getter();
                }
            }
            catch
            {
                // Fall back to serialized object if runtime property evaluation fails
            }

            var entryProp = GetSelectedSemanticEntryProp();
            var prop = entryProp?.FindPropertyRelative(propertyPath) ?? m_SerializedObject?.FindProperty(propertyPath);
            var val = prop?.FindPropertyRelative("Value") ?? prop;
            if (val != null)
            {
                var cb = new ColorBlock();
                var n = val.FindPropertyRelative("m_NormalColor"); if (n != null) cb.normalColor = n.colorValue;
                var h = val.FindPropertyRelative("m_HighlightedColor"); if (h != null) cb.highlightedColor = h.colorValue;
                var p = val.FindPropertyRelative("m_PressedColor"); if (p != null) cb.pressedColor = p.colorValue;
                var s = val.FindPropertyRelative("m_SelectedColor"); if (s != null) cb.selectedColor = s.colorValue;
                var d = val.FindPropertyRelative("m_DisabledColor"); if (d != null) cb.disabledColor = d.colorValue;
                cb.colorMultiplier = 1f;
                cb.fadeDuration = 0.2f;
                return cb;
            }
            return default;
        }

        private Sprite GetResolvedSprite(System.Func<Sprite> getter, string propertyPath)
        {
            try
            {
                if (m_TargetStyle != null)
                {
                    var resolved = getter();
                    if (resolved != null) return resolved;
                }
            }
            catch
            {
                // Fall back to serialized object if runtime property evaluation fails
            }

            var entryProp = GetSelectedSemanticEntryProp();
            var prop = entryProp?.FindPropertyRelative(propertyPath) ?? m_SerializedObject?.FindProperty(propertyPath);
            var isOverridden = prop?.FindPropertyRelative("IsOverriden") ?? prop?.FindPropertyRelative("IsOverridden");
            bool hasParent = GetParentStyle() != null;
            if (!hasParent || (isOverridden != null && isOverridden.boolValue))
            {
                var val = prop?.FindPropertyRelative("Value") ?? prop;
                if (val?.objectReferenceValue is Sprite s) return s;
            }
            else if (hasParent)
            {
                var parentStyle = GetParentStyle();
                if (parentStyle != null) return parentStyle.GetLogo(GetSelectedSemantic());
            }

            var fallbackVal = prop?.FindPropertyRelative("Value") ?? prop;
            return fallbackVal?.objectReferenceValue as Sprite;
        }

        private void UpdatePreview()
        {
            if (m_TargetStyle == null || m_PreviewPanel == null) return;

            if (m_SerializedObject != null && m_SerializedObject.targetObject != null)
            {
                var parentProp = m_SerializedObject.FindProperty("m_ParentStyle");
                if (parentProp != null && parentProp.objectReferenceValue == m_TargetStyle)
                {
                    parentProp.objectReferenceValue = null;
                }

                var currentParent = parentProp?.objectReferenceValue as UIStyle;
                if (currentParent != m_LastKnownParentStyle)
                {
                    m_LastKnownParentStyle = currentParent;
                    m_SerializedObject.ApplyModifiedProperties();
                    m_SerializedObject.Update();
                    BuildPropertiesUI();
                }
                else
                {
                    m_SerializedObject.ApplyModifiedProperties();
                    m_SerializedObject.Update();
                }
            }

            var sem = GetSelectedSemantic();

            // 1. Panel & Separator Colors
            m_PreviewPanel.style.backgroundColor = GetResolvedColor(() => m_TargetStyle.GetPanelColor(sem), "m_PanelColor");
            m_PreviewPanel.style.borderTopColor = GetResolvedColor(() => m_TargetStyle.GetPanelBorderColor(sem), "m_PanelBorderColor");
            m_PreviewPanel.style.borderRightColor = GetResolvedColor(() => m_TargetStyle.GetPanelBorderColor(sem), "m_PanelBorderColor");
            m_PreviewPanel.style.borderBottomColor = GetResolvedColor(() => m_TargetStyle.GetPanelBorderColor(sem), "m_PanelBorderColor");
            m_PreviewPanel.style.borderLeftColor = GetResolvedColor(() => m_TargetStyle.GetPanelBorderColor(sem), "m_PanelBorderColor");

            if (m_PreviewSeparator != null)
            {
                m_PreviewSeparator.style.backgroundColor = GetResolvedColor(() => m_TargetStyle.GetPanelSeparatorColor(sem), "m_PanelSeparatorColor");
            }

            // 2. Regular Text Colors & Sizes
            m_PreviewLabel.style.color = GetResolvedColor(() => m_TargetStyle.GetLabelColor(sem), "m_LabelColor");
            m_PreviewHeader1.style.color = GetResolvedColor(() => m_TargetStyle.GetHeader1Color(sem), "m_Header1Color");
            m_PreviewHeader2.style.color = GetResolvedColor(() => m_TargetStyle.GetHeader2Color(sem), "m_Header2Color");
            m_PreviewHeader3.style.color = GetResolvedColor(() => m_TargetStyle.GetHeader3Color(sem), "m_Header3Color");

            float labelFontSize = GetResolvedFloat(() => m_TargetStyle.GetLabelFontSize(sem), "m_LabelFontSize", 12f);
            m_PreviewLabel.style.fontSize = labelFontSize > 0 ? labelFontSize : 12f;

            float h1FontSize = GetResolvedFloat(() => m_TargetStyle.GetHeader1FontSize(sem), "m_Header1FontSize", 18f);
            m_PreviewHeader1.style.fontSize = h1FontSize > 0 ? h1FontSize : 18f;

            float h2FontSize = GetResolvedFloat(() => m_TargetStyle.GetHeader2FontSize(sem), "m_Header2FontSize", 14f);
            m_PreviewHeader2.style.fontSize = h2FontSize > 0 ? h2FontSize : 14f;

            float h3FontSize = GetResolvedFloat(() => m_TargetStyle.GetHeader3FontSize(sem), "m_Header3FontSize", 12f);
            m_PreviewHeader3.style.fontSize = h3FontSize > 0 ? h3FontSize : 12f;

            // 3. Logo Sprites
            var logoSprite = GetResolvedSprite(() => m_TargetStyle.GetLogo(sem), "m_Logo");
            if (logoSprite != null)
            {
                m_PreviewLogoImage.image = null;
                m_PreviewLogoImage.sprite = logoSprite;
                m_PreviewLogoImage.style.display = DisplayStyle.Flex;

                m_PreviewButtonIcon.image = null;
                m_PreviewButtonIcon.sprite = logoSprite;
                m_PreviewButtonIcon.style.display = DisplayStyle.Flex;

                m_DisabledPreviewButtonIcon.image = null;
                m_DisabledPreviewButtonIcon.sprite = logoSprite;
                m_DisabledPreviewButtonIcon.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_PreviewLogoImage.sprite = null;
                m_PreviewLogoImage.image = null;
                m_PreviewLogoImage.style.display = DisplayStyle.None;

                // Fallback icon for preview buttons if available
                var defaultIcon = EditorGUIUtility.FindTexture("d_PlayButton") ?? EditorGUIUtility.FindTexture("PlayButton");
                if (defaultIcon != null)
                {
                    m_PreviewButtonIcon.sprite = null;
                    m_PreviewButtonIcon.image = defaultIcon;
                    m_PreviewButtonIcon.style.display = DisplayStyle.Flex;

                    m_DisabledPreviewButtonIcon.sprite = null;
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

            UpdatePreviewButtonVisuals();
        }

        private void UpdatePreviewButtonVisuals()
        {
            if (m_TargetStyle == null || m_PreviewButton == null) return;

            var sem = GetSelectedSemantic();

            // Compute colors from ColorBlocks for current state
            Color bgColor = GetEffectiveColor(GetResolvedColorBlock(() => m_TargetStyle.GetButtonColorBlock(sem), "m_ButtonColorBlock"), m_CurrentButtonState);
            Color borderColor = GetEffectiveColor(GetResolvedColorBlock(() => m_TargetStyle.GetButtonBorderColorBlock(sem), "m_ButtonBorderColorBlock"), m_CurrentButtonState);
            Color textColor = GetEffectiveColor(GetResolvedColorBlock(() => m_TargetStyle.GetButtonTextColorBlock(sem), "m_ButtonTextColorBlock"), m_CurrentButtonState);
            Color iconColor = GetEffectiveColor(GetResolvedColorBlock(() => m_TargetStyle.GetButtonIconColorBlock(sem), "m_ButtonIconColorBlock"), m_CurrentButtonState);

            // Compute Scale and Animation Duration
            float targetScale = 1.0f;
            float animDuration = GetResolvedFloat(() => m_TargetStyle.GetButtonBackToNormalAnimationDuration(sem), "m_ButtonBackToNormalDuration", 0.5f);

            switch (m_CurrentButtonState)
            {
                case ButtonState.Highlighted:
                    targetScale = GetResolvedFloat(() => m_TargetStyle.GetButtonHoverScale(sem), "m_ButtonHoverScale", 1.0f);
                    animDuration = GetResolvedFloat(() => m_TargetStyle.GetButtonHoverAnimationDuration(sem), "m_ButtonHoverAnimationDuration", 0.5f);
                    break;
                case ButtonState.Pressed:
                    targetScale = GetResolvedFloat(() => m_TargetStyle.GetButtonClickScale(sem), "m_ButtonClickScale", 1.0f);
                    animDuration = GetResolvedFloat(() => m_TargetStyle.GetButtonClickAnimationDuration(sem), "m_ButtonClickAnimationDuration", 0.25f);
                    break;
                case ButtonState.Selected:
                    targetScale = GetResolvedFloat(() => m_TargetStyle.GetButtonClickScale(sem), "m_ButtonClickScale", 1.0f);
                    animDuration = GetResolvedFloat(() => m_TargetStyle.GetButtonClickAnimationDuration(sem), "m_ButtonClickAnimationDuration", 0.25f);
                    break;
            }

            // Apply transition durations
            float colorDuration = GetResolvedFloat(() => m_TargetStyle.GetColorAnimationDuration(sem), "m_ColorAnimationDuration", 0.5f);
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
                string semName = sem != null ? sem.name : "Default";
                m_StateInfoLabel.text = $"Variant: {semName}  |  State: {m_CurrentButtonState}  |  Scale: {targetScale:F2}x ({animDuration:F2}s)  |  Zoom: {m_PreviewScale * 100:F0}%  |  Color Transition: {colorDuration:F2}s";
            }
        }

        private void UpdateDisabledButtonVisuals()
        {
            if (m_TargetStyle == null || m_DisabledPreviewButton == null) return;

            var sem = GetSelectedSemantic();

            Color bgColor = GetEffectiveColor(GetResolvedColorBlock(() => m_TargetStyle.GetButtonColorBlock(sem), "m_ButtonColorBlock"), ButtonState.Disabled);
            Color borderColor = GetEffectiveColor(GetResolvedColorBlock(() => m_TargetStyle.GetButtonBorderColorBlock(sem), "m_ButtonBorderColorBlock"), ButtonState.Disabled);
            Color textColor = GetEffectiveColor(GetResolvedColorBlock(() => m_TargetStyle.GetButtonTextColorBlock(sem), "m_ButtonTextColorBlock"), ButtonState.Disabled);
            Color iconColor = GetEffectiveColor(GetResolvedColorBlock(() => m_TargetStyle.GetButtonIconColorBlock(sem), "m_ButtonIconColorBlock"), ButtonState.Disabled);

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

        #region Background Themes & Grid

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
            if (m_GridBackground == null) return;

            switch (m_BackgroundThemeIndex)
            {
                case 0: // Dark
                    m_GridBackground.SetColors(
                        new Color(0.12f, 0.12f, 0.12f, 1f),
                        new Color(1f, 1f, 1f, 0.035f),
                        new Color(1f, 1f, 1f, 0.08f)
                    );
                    break;
                case 1: // Light
                    m_GridBackground.SetColors(
                        new Color(0.88f, 0.88f, 0.88f, 1f),
                        new Color(0f, 0f, 0f, 0.045f),
                        new Color(0f, 0f, 0f, 0.10f)
                    );
                    break;
                case 2: // Neutral
                    m_GridBackground.SetColors(
                        new Color(0.24f, 0.24f, 0.24f, 1f),
                        new Color(1f, 1f, 1f, 0.04f),
                        new Color(1f, 1f, 1f, 0.09f)
                    );
                    break;
            }
        }

        #endregion

        #region Grid Background Element

        private class GridBackground : VisualElement
        {
            private Color m_MinorGridColor;
            private Color m_MajorGridColor;
            private bool m_ShowGrid = true;

            public bool ShowGrid
            {
                get => m_ShowGrid;
                set
                {
                    if (m_ShowGrid != value)
                    {
                        m_ShowGrid = value;
                        MarkDirtyRepaint();
                    }
                }
            }

            public GridBackground()
            {
                generateVisualContent += OnGenerateVisualContent;
            }

            public void SetColors(Color baseColor, Color minorGrid, Color majorGrid)
            {
                style.backgroundColor = baseColor;
                m_MinorGridColor = minorGrid;
                m_MajorGridColor = majorGrid;
                MarkDirtyRepaint();
            }

            private void OnGenerateVisualContent(MeshGenerationContext mgc)
            {
                if (!m_ShowGrid) return;

                var rect = contentRect;
                if (rect.width <= 0 || rect.height <= 0) return;

                var painter = mgc.painter2D;
                float width = rect.width;
                float height = rect.height;

                const float spacing = 20f;
                const int majorStep = 5;
                const float majorSpacing = spacing * majorStep;

                float centerX = Mathf.Round(width * 0.5f);
                float centerY = Mathf.Round(height * 0.5f);

                // Minor grid lines
                painter.strokeColor = m_MinorGridColor;
                painter.lineWidth = 1f;
                painter.BeginPath();

                for (float x = centerX + spacing; x <= width; x += spacing)
                {
                    if (Mathf.Abs(x - centerX) % majorSpacing < 0.1f) continue;
                    painter.MoveTo(new Vector2(x, 0));
                    painter.LineTo(new Vector2(x, height));
                }
                for (float x = centerX - spacing; x >= 0; x -= spacing)
                {
                    if (Mathf.Abs(x - centerX) % majorSpacing < 0.1f) continue;
                    painter.MoveTo(new Vector2(x, 0));
                    painter.LineTo(new Vector2(x, height));
                }
                for (float y = centerY + spacing; y <= height; y += spacing)
                {
                    if (Mathf.Abs(y - centerY) % majorSpacing < 0.1f) continue;
                    painter.MoveTo(new Vector2(0, y));
                    painter.LineTo(new Vector2(width, y));
                }
                for (float y = centerY - spacing; y >= 0; y -= spacing)
                {
                    if (Mathf.Abs(y - centerY) % majorSpacing < 0.1f) continue;
                    painter.MoveTo(new Vector2(0, y));
                    painter.LineTo(new Vector2(width, y));
                }
                painter.Stroke();

                // Major grid lines
                painter.strokeColor = m_MajorGridColor;
                painter.lineWidth = 1f;
                painter.BeginPath();

                for (float x = centerX; x <= width; x += majorSpacing)
                {
                    painter.MoveTo(new Vector2(x, 0));
                    painter.LineTo(new Vector2(x, height));
                }
                for (float x = centerX - majorSpacing; x >= 0; x -= majorSpacing)
                {
                    painter.MoveTo(new Vector2(x, 0));
                    painter.LineTo(new Vector2(x, height));
                }
                for (float y = centerY; y <= height; y += majorSpacing)
                {
                    painter.MoveTo(new Vector2(0, y));
                    painter.LineTo(new Vector2(width, y));
                }
                for (float y = centerY - majorSpacing; y >= 0; y -= majorSpacing)
                {
                    painter.MoveTo(new Vector2(0, y));
                    painter.LineTo(new Vector2(width, y));
                }
                painter.Stroke();
            }
        }

        #endregion

        #region Brand Icon

        private static Texture2D s_CachedBrandIcon;

        private const string EmbeddedBrandIconBase64 =
            "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAEUUlEQVR4nMVXTU8bVxT1eGz8gQOJBCYrxJIVRKZuqasgJbQUCoIV" +
            "GxZIJY1UiJIlYsFH+VhG2bBAahaoyg8I0CbdJGlTohBFKRtAbNrYRiBBa6JWJMYez3B67xuPYxtjZuSkedLxeOyZd88797z73rPZ" +
            "cptECBDuECKEFAElQiGECT8QPiPYbSc0N+EmIfEOghYjcysd61jw5+8xcD6eZ5Owp1n9X8ENsNqccpEXxcrLkiRlUAIBTjX7TZjD" +
            "dECzMEmCzS4capqA3W4XKBTU+D37/1P6jthOk1+SdNjtEjxuD1paWtDa2oqOjg50d3ejt7cXfX196O/vF9f29nZUVVVlSJxCgKf5" +
            "SSO25UCWJVxovIAjTcPR0VEO8tvW1hYaGxtz1CpCorjcxlWWZZ1AOiB/akRGVVUBLY/YxsYGGhoaxHunpKOwAkZQhjGSpkBTOoCK" +
            "ZCqJF7+/wOPHv2Jl5SnW1tawu7sLRVGQUjVo9NzOzo4ZEoUJBAIBzM7O4vbt7zE4+C0RkBEMfmKMHdvUeV1dHbxej4DP50NtbS3m" +
            "5+cRP0xCUVNEQsPm5qYgUSQVhQkMDAzg9esDHBz8i0ePHsLhkNHU1ETxSWpNRXQrivM159Mdv1XN7XZjanoaSooIaKrA6uoqXGUu" +
            "awSGhoaQSMRxePiGZP5FyBgMfiwIaDS6SCQCf7UfUoHUecu9WFxcolQRWaQQ+/svVJypsEagvr4eN25cx7Vrg+jq6iQFnGj+NCQ6" +
            "VVMKotEoavy5BHRIYsq2ffElkskkPZ/EP6/2UVlZaY6AYRYescPhgEMmM1KHDroPhUJCfkVJEIEIEagpQEDv49KlVlIwQSlI4tV+" +
            "jAictaaALNnhIONlIDvQ3NwscppMJigFYfhJgZPev3z5c0FAVRXsM4EKkwoYHXB+v6Jqxwh+FIRTdiJEKeD5zgqEwy8FgfxODQXb" +
            "2tpECgSBGKWAPWCzQODKla+FCRnPnj2F06krwHUgRTWACVRXVxec32xELsm6B1TEYjHdhGYJcAdXr35DMyCOePwAT578JvzAHuDG" +
            "lY9N6CcPvF14jHmuz4Surm5RlLhm7O3tocJ3BnYrCnR2dmJ5eZlGv4K5uTlB4OLFFqEAE+D8PnjwEAsLi7h7d4GuC1ha+hE/3buH" +
            "+/d/xp9/vMysDdvb2/B5fRZNSK73eDwoLy+Hy+US92IWEAH2gdG4LmavD/lLE/+3vr6OsrIy8wSyzZS9MAWDQd0DXOFYCaoJ/F3l" +
            "RYmQvyDxPftgZGREvH9CrMIEChE6d/YcRkdHMTYxjsmpSUzNTGN6ZgaTk1MYn/gOEwITGBsbF88NDw+jp6eH1gpv0dXQ1N7/2I6I" +
            "ipNAadsz3gyJbZFpFfI7LVQJLSDMBO6U0EGp4A2x2Bq/z9NQMfn5SCAOBzc/AAE+DGXOiR/0aJZN4pbN4inJIhJptY8FNxpLwnlh" +
            "c4TfERme5hGbbnb2m5Qd8D/Aa6oEwk9/mwAAAABJRU5ErkJggg==";

        private static Texture2D LoadBrandIcon()
        {
            if (s_CachedBrandIcon != null) return s_CachedBrandIcon;

            // 1. Try finding asset by search in AssetDatabase
            var guids = AssetDatabase.FindAssets("darketomaly_icon t:Texture2D");
            if (guids != null && guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                s_CachedBrandIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (s_CachedBrandIcon != null) return s_CachedBrandIcon;
            }

            // 2. Try direct package / project paths
            string[] directPaths = new[]
            {
                "Assets/com.darketomaly.framework.ui/Scripts/Editor/Icons/darketomaly_icon.png",
                "Packages/com.darketomaly.framework.ui/Scripts/Editor/Icons/darketomaly_icon.png"
            };
            foreach (var p in directPaths)
            {
                s_CachedBrandIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                if (s_CachedBrandIcon != null) return s_CachedBrandIcon;
            }

            // 3. Fallback: decode embedded PNG bytes
            try
            {
                byte[] bytes = System.Convert.FromBase64String(EmbeddedBrandIconBase64);
                var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
                tex.LoadImage(bytes);
                tex.hideFlags = HideFlags.HideAndDontSave;
                tex.name = "darketomaly_icon";
                s_CachedBrandIcon = tex;
                return s_CachedBrandIcon;
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}