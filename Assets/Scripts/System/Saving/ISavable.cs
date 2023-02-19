namespace Scripts.System.Saving
{
    /// <summary>
    /// Implement in any component that has state to save/restore.
    /// </summary>
    public interface ISavable
    {
        /// <summary>
        /// Key under which the state of the component will be saved.
        /// </summary>
        public string Guid { get; set; }
        
        /// <summary>
        /// Called when saving to capture the state of the component.
        /// </summary>
        /// <returns>
        /// Return a `System.Serializable` object that represents the state of the
        /// component.
        /// </returns>
        object CaptureState();

        /// <summary>
        /// Called when restoring the state of a scene.
        /// </summary>
        /// <param name="state">
        /// The same `System.Serializable` object that was returned by
        /// CaptureState when saving.
        /// </param>
        void RestoreState(object state);
    }
}