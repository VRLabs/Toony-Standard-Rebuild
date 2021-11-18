using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class ModuleUI
    {
        public string Name;
        public List<SectionUI> Sections;
        public List<UVSet> UVSets;

        public ModuleUI()
        {
            Sections = new List<SectionUI>();
            UVSets = new List<UVSet>();
        }
    }
}