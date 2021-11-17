using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class SectionUI
    {
        public string SectionName;
        public bool IsPermanent;
        public string ActivatePropertyName;
        public float EnableValue;
        public float DisableValue;
        public List<ControlUI> Controls;
        
        public UpdateData OnSectionDisableData;

        public SectionUI()
        {
            SectionName = "";
            ActivatePropertyName = "";
            EnableValue = 1;
            DisableValue = 0;
            Controls = new List<ControlUI>();
            OnSectionDisableData = new UpdateData();
        }
    }
}