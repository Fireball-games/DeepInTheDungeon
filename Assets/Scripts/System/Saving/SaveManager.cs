using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Helpers;
using Scripts.System.MonoBases;

namespace Scripts.System.Saving
{
    /// <summary>
    /// Class handling the saving and loading of the game.
    /// </summary>
    public class SaveManager : SingletonNotPersisting<SaveManager>
    {
        private IEnumerable<Save> _saves = Enumerable.Empty<Save>();

        private void Start()
        {
            if (!_saves.Any() && (FileOperationsHelper.LoadAllSaves(out IEnumerable<Save> saves)))
            {
                _saves = saves;
            }
        }
    }
}