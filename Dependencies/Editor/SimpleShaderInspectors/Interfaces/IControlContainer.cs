using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public interface IControlContainer<T> : IControlContainer where T : SimpleControl
    {
        void AddControl(T control, string alias = "");
        
        new IEnumerable<T> GetControlList();
    }

    public interface IControlContainer
    {
        void AddControl(SimpleControl control, string alias = "");
        
        IEnumerable<SimpleControl> GetControlList();
    }

    public static class ContainerExtensions
    {
        public static void AddControl<T>(this IList<T> items, T control, string alias = "") where T : SimpleControl
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                items.Add(control);
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ControlAlias != alias) continue;
                items.Insert(i+1, control);
                return;
            }
            
            items.Add(control);
        }
    }
    
}