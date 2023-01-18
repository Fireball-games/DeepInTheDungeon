using System.Collections.Generic;
using static Scripts.Enums;
using NotImplementedException = System.NotImplementedException;

namespace Scripts.Triggers
{
    public class DoTweenVolumeTrigger : VolumeTrigger, IPositionsTrigger
    {
        protected override void OnTriggerActivated(ETriggerActivatedDetail activatedDetail = ETriggerActivatedDetail.None)
        {
            
        }

        public List<DoTweenMoveStep> GetSteps()
        {
            throw new NotImplementedException();
        }

        public int GetStartPosition()
        {
            throw new NotImplementedException();
        }

        public void SetPosition()
        {
            throw new NotImplementedException();
        }
    }
}