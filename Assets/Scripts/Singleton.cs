using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour
where T : class 
{
    public static T Instance;

    protected virtual void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this as T;
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this as T)
        {
            Instance = null;
        }
    }
}
