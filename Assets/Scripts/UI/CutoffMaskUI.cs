using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class CutoffMaskUI : Image
    {
        public override Material materialForRendering {
            get
            {
                return base.materialForRendering;
            }
        }
    }
}