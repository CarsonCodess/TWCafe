using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI sliderText;

    private int targetFPS;

    void Start()
    {
        Application.targetFrameRate = 60;
    }

    void Update(){
        slider.onValueChanged.AddListener((v) => {
            sliderText.text = v.ToString("0");
            targetFPS = (int)v;
        });
    }

    public void Apply()
    {
        Debug.Log(targetFPS);
        Application.targetFrameRate = targetFPS;
        Debug.Log("Applied");
    }
}

