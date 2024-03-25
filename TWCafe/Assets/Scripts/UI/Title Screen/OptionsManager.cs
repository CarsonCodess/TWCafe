using System.Collections;
using System.Collections.Generic;
using FlatKit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    [SerializeField] private Slider fpsSlider;
    [SerializeField] private TextMeshProUGUI fpsSliderText;    
    [SerializeField] private Slider outlineSizeSlider;
    [SerializeField] private TextMeshProUGUI outlineSizeText;
    [SerializeField] private OutlineSettings outlineSettings;

    private int _targetFPS;
    private int _outlineSize;

    private void Awake()
    {
        Application.targetFrameRate = 60;
        fpsSlider.onValueChanged.AddListener(value => 
        {
            fpsSliderText.text = value.ToString("0");
            _targetFPS = (int) value;
        });
        
        outlineSizeSlider.onValueChanged.AddListener(value => 
        {
            outlineSizeText.text = value.ToString("0");
            _outlineSize = (int) value;
        });
    }

    public void Apply()
    {
        Application.targetFrameRate = _targetFPS;
        outlineSettings.thickness = _outlineSize;
    }
}

