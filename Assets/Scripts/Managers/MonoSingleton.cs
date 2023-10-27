using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static object lockObj = new object();

    private static T instance;
    public static T Instance
    {
        get
        {
            lock (lockObj)
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();
                }

                return instance;
            }
        }
    }
}
