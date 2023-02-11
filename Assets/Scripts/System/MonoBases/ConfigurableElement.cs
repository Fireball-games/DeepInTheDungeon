using UnityEngine.Events;

namespace Scripts.System.MonoBases
{
    public abstract class ConfigurableElement : UIElementBase
    {
        public abstract void SetValue(object value);
        public abstract void SetLabel(object text);
        public abstract void SetOnValueChanged(UnityAction<object> onValueChanged);
    }
}