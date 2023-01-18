using System.Collections.Generic;

namespace Scripts.Triggers
{
    public interface IPositionsTrigger
    {
        public List<DoTweenMoveStep> GetSteps();
        public int GetStartPosition();
        public void SetPosition();

    }
}