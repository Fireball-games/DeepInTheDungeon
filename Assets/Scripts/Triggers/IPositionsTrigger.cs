using System.Collections.Generic;

namespace Scripts.Triggers
{
    /// <summary>
    /// Common interface for both triggers and trigger receivers which needs start position and have steps for movement.
    /// </summary>
    public interface IPositionsTrigger
    {
        public List<DoTweenMoveStep> GetSteps();
        public int GetStartPosition();
        public void SetStartPosition(int newStartPosition, bool isTargetPosition = true);
        public void SetPosition();

    }
}