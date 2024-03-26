using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : Singleton<LoadingScreen>
{
    [SerializeField] private GameObject content;
    [SerializeField] private Image fill;
    [SerializeField] private float maxRectValue;
    [SerializeField, Range(0f, 1f)] private float value;
    [SerializeField, Range(0f, 10f)] private float loadTime = 2f;
    [SerializeField, Range(0f, 10f)] private int steps = 3;
    private float _timePerStep = -1;
    private float _timer;

    public void Show()
    {
        content.SetActive(true);
    }
    
    private void Hide()
    {
        content.SetActive(false);
        _timePerStep = -1;
    }

    public void LoadFake()
    {
        Show();
        value = 0f;
        _timePerStep = loadTime / steps;
        _timer = _timePerStep;
    }

    private void Update()
    {
        if (content.activeSelf)
        {
            fill.rectTransform.sizeDelta = new Vector2(-Mathf.Lerp(0f, maxRectValue * 2, 1 - value),
                fill.rectTransform.sizeDelta.y);
            fill.rectTransform.anchoredPosition = new Vector2(-Mathf.Lerp(0f, maxRectValue, 1 - value),
                fill.rectTransform.anchoredPosition.y);
            if (value < 1)
            {
                _timer += Time.deltaTime;
                if (_timer >= _timePerStep)
                {
                    DOVirtual.Float(value, Mathf.Min(1, value + 1f / steps), _timePerStep, val =>
                    {
                        value = val;
                    });
                    _timer -= _timePerStep;
                }
            }

            else
                Hide();
        }
    }

    public void SetValue(float val)
    {
        value = val;
    }
}
