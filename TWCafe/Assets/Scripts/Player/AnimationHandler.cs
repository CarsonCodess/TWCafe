using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
    [SerializeField] private Animator anim;
    private string _animationState;

    [Button]
    public void TryGetAnimator()
    {
        if(anim != null)
            return;
        anim = GetComponent<Animator>();
        if (anim == null)
            anim = GetComponentInChildren<Animator>();
    }

    public void Play(string animationName, float duration = 0.1f)
    {
        if(_animationState == animationName)
            return;
        anim.CrossFade(animationName, duration);
        _animationState = animationName;
    }
    
    public void Stop()
    {
        anim.StopPlayback();
    }
    
    public void SetParameter(string parameterName, float value, float duration = 0.1f)
    {
        anim.SetFloat(parameterName, value, duration, Time.deltaTime);
    }
}
