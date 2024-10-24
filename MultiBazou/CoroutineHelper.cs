using System.Collections;

namespace MultiBazou
{
    using UnityEngine;

    public class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper _instance;

        public static CoroutineHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject obj = new GameObject("CoroutineRunner");
                    _instance = obj.AddComponent<CoroutineHelper>();
                    DontDestroyOnLoad(obj);
                }
                return _instance;
            }
        }

        public void StartRoutine(IEnumerator coroutine)
        {
            StartCoroutine(coroutine);
        }
    }

}