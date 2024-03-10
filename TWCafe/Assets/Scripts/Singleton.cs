using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    public static bool Exists => _instance != null;
    [SerializeField] protected bool dontDestroyOnLoad = false;
    private static T _instance;

    public static T Instance => _instance;

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
}