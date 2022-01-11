using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors.Controls.Sections
{
    public class OrderedSectionGroup : SimpleControl, IControlContainer<OrderedSection>
    {
        private bool? _areNewSectionsAvailable;

        public List<OrderedSection> Controls { get; set; }

        public GUIStyle ButtonStyle { get; set; }

        public Color ButtonColor { get; set; }

        public OrderedSectionGroup(string alias) : base(alias)
        {
            Controls = new List<OrderedSection>();

            ButtonStyle = new GUIStyle(Styles.Bubble);
            ButtonColor = Color.white;
        }

        protected override void ControlGUI(MaterialEditor materialEditor)
        {
            bool needsOrderUpdate = false;
            bool needsSectionAvailabilityUpdate = false;
            foreach (var t in Controls)
                t.PredrawUpdate(materialEditor);

            if (_areNewSectionsAvailable == null)
                UpdateSectionsOrder();
            
            for (int i = 0; i < Controls.Count; i++)
            {
                if (!Controls[i].Enabled) continue;
                
                Controls[i].DrawControl(materialEditor);
                if (Controls[i].PushState != 0)
                {
                    if (Controls[i].PushState == 1 && i < Controls.Count - 1)
                    {
                        Controls[i].SectionPosition++;
                        Controls[i + 1].SectionPosition--;
                    }
                    else if (Controls[i].PushState == -1 && i > 0 && Controls[i - 1].Enabled)
                    {
                        Controls[i].SectionPosition--;
                        Controls[i - 1].SectionPosition++;
                    }
                    Controls[i].PushState = 0;
                    needsOrderUpdate = true;
                }
                else if (Controls[i].HasActivatePropertyUpdated)
                {
                    needsSectionAvailabilityUpdate = true;
                }
            }
            if (_areNewSectionsAvailable == null || needsSectionAvailabilityUpdate)
                _areNewSectionsAvailable = AreNewSectionsAvailable();
            
            if (needsOrderUpdate)
                UpdateSectionsOrder();
            
            EditorGUILayout.Space();
            DrawAddButton();
        }

        private void DrawAddButton()
        {
            if (!(_areNewSectionsAvailable ?? true)) return;
            
            Color bCol = GUI.backgroundColor;
            GUI.backgroundColor = ButtonColor;
            var buttonRect = GUILayoutUtility.GetRect(Content, ButtonStyle);
            bool buttonPressed = GUI.Button(buttonRect, Content, ButtonStyle);
            
            if (buttonPressed)
            {
                List<OrderedSection> items = new List<OrderedSection>();
                foreach (var section in Controls)
                    if (section.HasAtLeastOneMaterialDisabled())
                        items.Add(section);
                
                var dropdown = new OrderedSectionDropdown(Content.text, new AdvancedDropdownState(), items, TurnOnSection);
                dropdown.Show(buttonRect);
            }
            GUI.backgroundColor = bCol;
        }

        private void TurnOnSection(OrderedSection section)
        {
            section.SectionPosition = 753;
            section.HasSectionTurnedOn = true;
            _areNewSectionsAvailable = AreNewSectionsAvailable();
            UpdateSectionsOrder();
        }

        private void UpdateSectionsOrder()
        {
            Controls.Sort(CompareSectionsOrder);
            int j = 1;
            foreach (var section in Controls)
            {
                if (section.SectionPosition != 0 && !section.AdditionalProperties[0].Property.hasMixedValue)
                {
                    section.SectionPosition = j;
                    j++;
                }
            }
        }

        private bool AreNewSectionsAvailable()
        {
            bool yesThereAre = false;
            foreach (var section in Controls)
            {
                yesThereAre = section.HasAtLeastOneMaterialDisabled();
                if (yesThereAre) break;
            }
            return yesThereAre;
        }

        private static int CompareSectionsOrder(OrderedSection x, OrderedSection y)
        {
            if (x == null) return y == null ? 0 : -1;

            if (y == null) return 1;
            if (x.SectionPosition > y.SectionPosition)
                return 1;
            if (x.SectionPosition < y.SectionPosition)
                return -1;
            return 0;
        }
        
        public IEnumerable<OrderedSection> GetControlList() => Controls;

        public void AddControl(OrderedSection control, string alias = "") => Controls.AddControl(control, alias);

        void IControlContainer.AddControl(SimpleControl control, string alias = "")
        {
            if(control is OrderedSection section)
                Controls.AddControl(section, alias);
        }

        IEnumerable<SimpleControl> IControlContainer.GetControlList() => Controls;
    }
}