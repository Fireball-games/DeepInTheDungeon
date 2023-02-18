using UnityEngine;
using static Scripts.Building.Tile.TileDescription;

namespace Scripts.MapEditor
{
    public class TileGizmo : MonoBehaviour
    {
        public ETileDirection direction;

        public void SetActive(bool isActive) => gameObject.SetActive(isActive);
    }
}
