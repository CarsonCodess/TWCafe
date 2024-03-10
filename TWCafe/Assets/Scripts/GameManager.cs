using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int fps = 60;
    
    private void Awake()
    {
        Application.targetFrameRate = fps;
    }
}
