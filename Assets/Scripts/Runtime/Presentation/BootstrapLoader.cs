using UnityEngine;
using UnityEngine.SceneManagement;

namespace CurioClerk.Presentation
{
    public sealed class BootstrapLoader : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
        }
    }
}
