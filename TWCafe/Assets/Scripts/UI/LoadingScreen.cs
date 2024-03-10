using System;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : Singleton<LoadingScreen>
{
    [SerializeField] private GameObject content;
    [SerializeField] private Image fill;
    [SerializeField] private float maxRectValue;
    [SerializeField, Range(0f, 1f)] private float value;
    private AsyncOperation _op;

    public void Show()
    {
        content.SetActive(true);
    }
    
    public void Hide()
    {
        content.SetActive(false);
    }

    public void LoadSceneOperation(AsyncOperation operation)
    {
        Show();
        _op = operation;
    }

    private void Update()
    {
        if (content.activeSelf)
        {
            if (_op == null)
            {
                fill.rectTransform.sizeDelta = new Vector2(-Mathf.Lerp(0f, maxRectValue * 2, 1 - value),
                    fill.rectTransform.sizeDelta.y);
                fill.rectTransform.anchoredPosition = new Vector2(-Mathf.Lerp(0f, maxRectValue, 1 - value),
                    fill.rectTransform.anchoredPosition.y);
            }
            else
            {
                fill.rectTransform.sizeDelta = new Vector2(-Mathf.Lerp(0f, maxRectValue * 2, 1 - _op.progress),
                    fill.rectTransform.sizeDelta.y);
                fill.rectTransform.anchoredPosition = new Vector2(-Mathf.Lerp(0f, maxRectValue, 1 - _op.progress),
                    fill.rectTransform.anchoredPosition.y);
                if (_op.isDone)
                {
                    _op = null;
                    Hide();
                }
            }
        }
    }

    public void SetValue(float val)
    {
        value = val;
    }
}
