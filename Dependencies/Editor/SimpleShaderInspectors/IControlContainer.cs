using System.Collections.Generic;

namespace VRLabs.ToonyStandardRebuild.SimpleShaderInspectors
{
    public interface IControlContainer<T> : IControlContainer where T : SimpleControl
    {
        void AddControl(T control);
        
        new IEnumerable<T> GetControlList();
    }

    public interface IControlContainer
    {
        void AddControl(SimpleControl control);
        
        IEnumerable<SimpleControl> GetControlList();
    }
}