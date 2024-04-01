using System;
using DG.Tweening;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    public bool IsDashing { get; private set; }
    
    [SerializeField] private float dashCooldown = 2f;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashTime = 0.4f;
    
    private InputHandler _inputHandler;
    private Rigidbody _rb;
    
    private bool _canDash = true;
    private float _dashTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (TryGetComponent(out _inputHandler))
            _inputHandler.OnDash += Dash;
    }
    
    public void OnDestroy()
    {
        if (_inputHandler)
            _inputHandler.OnDash -= Dash;
    }

    private void Update()
    {
        _dashTimer += Time.deltaTime;
        if(_dashTimer >= dashCooldown)
            _canDash = true;
    }
    
    private void Dash()
    {
        if(!_canDash)
            return;
        if(_canDash)
        {
            IsDashing = true;
            var dashTarget = new Vector3(transform.position.x + _inputHandler.MovementDirection.x * dashDistance, transform.position.y, transform.position.z + _inputHandler.MovementDirection.y * dashDistance);
            _rb.transform.DOMove(dashTarget, dashTime).SetEase(Ease.InOutQuint).OnComplete (() => {
                IsDashing = false;
            });
            _canDash = false;
            _dashTimer = 0;
        }
    }
}
