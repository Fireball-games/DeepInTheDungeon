using System.Collections;
using System.Threading.Tasks;
using Scripts.ScenesManagement;
using UnityEngine;

namespace Scripts.System
{
    public class GameStarter : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1);

            await SceneLoader.Instance.LoadMainMenuScene(false, () => GameManager.Instance.StartGame());
        }
    }
}