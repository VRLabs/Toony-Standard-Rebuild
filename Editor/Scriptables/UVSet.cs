using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild
{
    public class UVSet
    {
        public string ID;
        public List<UVItem> Items;

        public UVSet()
        {
            Items = new List<UVItem>();
        }
    }
}