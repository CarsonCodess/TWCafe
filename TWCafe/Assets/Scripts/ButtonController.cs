using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewBehaviourScript : MonoBehaviour
{
    [SerializeField] int sceneIndex;
    public void PlayScene(){
        SceneManager.LoadScene(sceneIndex);
    }

    public void QuitGame(){
        Application.Quit();
    }
}
