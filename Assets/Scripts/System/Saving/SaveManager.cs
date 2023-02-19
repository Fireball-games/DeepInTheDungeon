using System.Collections.Generic;
using Scripts.System.MonoBases;

namespace Scripts.System.Saving
{
    /// <summary>
    /// Class handling the saving and loading of the game.
    /// </summary>
    public class SaveManager : Singleton<SaveManager>
    {
        private List<Save> _saves = new();

        protected override void Awake()
        {
            base.Awake();
            
            
        }
    }
}