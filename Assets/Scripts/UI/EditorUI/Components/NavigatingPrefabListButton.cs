using Scripts.Helpers;
using UnityEngine.EventSystems;

namespace Scripts.UI.EditorUI.Components
{
    public class NavigatingPrefabListButton : PrefabListButton, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            Logger.Log(">>>>");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Logger.Log("<<<<");
        }
    }
}