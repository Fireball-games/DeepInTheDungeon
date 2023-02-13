using System.Threading.Tasks;
using UnityEngine;

namespace Scripts.System
{
    public class GameStarter : MonoBehaviour
    {
        private async void Start()
        {
            await Task.Delay(1000);
            
            GameManager.Instance.StartMainScene();
        }
    }
}