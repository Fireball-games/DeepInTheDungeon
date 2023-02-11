using Scripts.UI.Components;
using UnityEngine.Events;

namespace Scripts.System.MonoBases
{
    public abstract class ConfigurableElement : UIElementBase, IConfigurableElement
    {
        public abstract void SetValue(object value);
        public abstract void SetLabel(object text);
        public abstract void SetOnValueChanged(UnityAction<object> onValueChanged);
    }
}