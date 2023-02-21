using System.Collections.Generic;

namespace Scripts.Triggers
{
    /// <summary>
    /// Common interface for both triggers and trigger receivers which needs start position and have steps for movement.
    /// </summary>
    public interface IPositionsTrigger
    {
        public List<DoTweenMoveStep> GetSteps();
        public int GetCurrentPosition();

        /// <summary>
        /// Sets start position for trigger or trigger receiver - sets position physically as well.
        /// </summary>
        /// <param name="newPosition"></param>
        public void SetCurrentPosition(int newPosition);
    }
}