using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace VRLabs.ToonyStandardRebuild.ModularShaderSystem
{
    public interface IModularShaderDebuggerTab
    { 
        VisualElement TabContainer { get; set; }
        
        string TabName { get; set; }

        void UpdateTab(ModularShader shader);
    }
    
    public class ModularShaderDebugger : EditorWindow
    {
        [MenuItem(MSSConstants.WINDOW_PATH + "/Modular Shader Debugger")]
        public static void ShowExample()
        {
            ModularShaderDebugger wnd = GetWindow<ModularShaderDebugger>();
            wnd.titleContent = new GUIContent("Modular Shader Debugger");
            
            if (wnd.position.width < 400 || wnd.position.height < 400)
            {
                Rect size = wnd.position;
                size.width = 1280;
                size.height = 720;
                wnd.position = size;
            }
        }
        
        private ObjectField _modularShaderField;
        private ModularShader _modularShader;
        private VisualElement _selectedTab;

        private List<IModularShaderDebuggerTab> _tabs;

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            
            var styleSheet = Resources.Load<StyleSheet>(MSSConstants.RESOURCES_FOLDER + "/MSSUIElements/ModularShaderDebuggerStyle");
            root.styleSheets.Add(styleSheet);

            _modularShaderField = new ObjectField("Shader");
            _modularShaderField.objectType = typeof(ModularShader);
            _modularShaderField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(e =>
            {
                if (_modularShaderField.value != null)
                    _modularShader = (ModularShader)_modularShaderField.value;
                else
                    _modularShader = null;
                
                UpdateTabs();
            });

            _tabs = new List<IModularShaderDebuggerTab>();

            var buttonRow = new VisualElement();
            buttonRow.AddToClassList("button-tab-area");

            _selectedTab = new VisualElement();
            _selectedTab.style.flexGrow = 1;

            root.Add(_modularShaderField);
            root.Add(buttonRow);
            root.Add(_selectedTab);
            
            var tabTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.GetInterface(typeof(IModularShaderDebuggerTab).FullName) != null)
                .OrderBy(x => x.Name)
                .ToList();

            foreach (var type in tabTypes)
            {
                var tab = Activator.CreateInstance(type) as IModularShaderDebuggerTab;
                
                var tabButton = new Button();
                tabButton.text = tab?.TabName;
                tabButton.AddToClassList("button-tab");
                
                tabButton.clicked += () =>
                {
                    foreach (var button in buttonRow.Children())
                        if(button.ClassListContains("button-tab-selected"))
                            button.RemoveFromClassList("button-tab-selected");
                    
                    tabButton.AddToClassList("button-tab-selected");
                   
                    _selectedTab.Clear();
                    _selectedTab.Add(tab.TabContainer);
                };
                
                buttonRow.Add(tabButton);
                _tabs.Add(tab);
            }

            if (_tabs.Count == 0) return;
            var graph = _tabs.FirstOrDefault(x => x.GetType() == typeof(TemplateGraph));
            var timeline = _tabs.FirstOrDefault(x => x.GetType() == typeof(FunctionTimeline));

            if (timeline != null)
            {
                var index = _tabs.IndexOf(timeline);
                var button = buttonRow[index];
                _tabs.RemoveAt(index);
                buttonRow.RemoveAt(index);
                _tabs.Insert(0, timeline);
                buttonRow.Insert(0, button);
            }
            if (graph != null)
            {
                var index = _tabs.IndexOf(graph);
                var button = buttonRow[index];
                _tabs.RemoveAt(index);
                buttonRow.RemoveAt(index);
                _tabs.Insert(0, graph);
                buttonRow.Insert(0, button);
            }
            
            buttonRow[0].AddToClassList("button-tab-selected");
            _selectedTab.Add(_tabs[0].TabContainer);
        }

        private void UpdateTabs()
        {
            foreach (IModularShaderDebuggerTab tab in _tabs)
            {
                tab.UpdateTab(_modularShader);
            }
        }
    }
    
    public class CodeViewElement : VisualElement
    {
        private class LineItem : VisualElement
        {
            private static Font TextFont
            {
                get
                {
                    if (_font == null)
                        _font = Resources.Load<Font>(MSSConstants.RESOURCES_FOLDER + "/RobotoMono-Regular");
                    return _font;
                }
            }

            private static Font _font;
            private Label _lineNumber;
            private Label _line;
            public string Text { get; }

            public LineItem() : this(0, "") {}

            public LineItem(int number, string text, int digits = 0)
            {
                Text = text;
                _lineNumber = new Label("" + number);
                _lineNumber.style.color = Color.gray;
                _lineNumber.style.width = digits == 0 ? 30 : digits * 8;
                _lineNumber.style.unityTextAlign = TextAnchor.MiddleRight;
                _lineNumber.style.unityFont = TextFont;
                _lineNumber.style.marginRight = 4;
                _lineNumber.style.marginLeft = 4;
                
                _line = new Label(text);
                _line.style.flexGrow = 1;
                _line.style.unityFont = TextFont;

                style.flexDirection = FlexDirection.Row;
                Add(_lineNumber);
                Add(_line);
            }

            public void SetText(int i, string textLine, int digits)
            {
                _lineNumber.text = "" + i;
                _lineNumber.style.width = digits == 0 ? 30 : digits * 8;
                _line.text = textLine;
                _line.MeasureTextSize(textLine, 0, MeasureMode.Exactly, 0, MeasureMode.Exactly);
            }
        }
        
        public string Text
        {
            get => string.Join("\n", _textLines);
            set
            {
                _textLines = value == null ? Array.Empty<string>() : value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                _digits = (int)Math.Floor(Math.Log10(_textLines.Length) + 1);
                _listView.itemsSource = _textLines;

                float width =((_textLines.Length == 0 ? 0 : _textLines.Max(x => x.Length)) + _digits + 1) * 10;
                _listView.contentContainer.style.width = width;
            }
        }
        
        public int LineCount => _textLines.Length;
        
        private Label _templateLabel;
        private string[] _textLines;
        private ListView _listView;
        private int _digits;

        public CodeViewElement()
        {
            ScrollView s = new ScrollView(ScrollViewMode.Horizontal);
            _listView = new ListView();
            _listView.itemHeight = 15;
            _listView.AddToClassList("unity-base-text-field__input");
            _listView.AddToClassList("unity-text-field__input");
            _listView.AddToClassList("unity-base-field__input");
            _listView.style.flexGrow = 1;
            _listView.contentContainer.style.flexGrow = 1;
            
            Func<VisualElement> makeItem = () => new LineItem();
            Action<VisualElement, int> bindItem = (e, i) => (e as LineItem).SetText(i+1, _textLines[i], _digits);
            
            _listView.makeItem = makeItem;
            _listView.bindItem = bindItem;
            _listView.selectionType = SelectionType.None;
            s.Add(_listView);
            Add(s);
            s.style.flexGrow = 1;
            s.contentContainer.style.flexGrow = 1;

            style.flexGrow = 1;
        }
    }
}