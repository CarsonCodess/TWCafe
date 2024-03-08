using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashTime = 0.25f;
    
    private PlayerControls _playerControls;
    private Vector2 _moveDirection = Vector2.zero;
    private InputAction _move;
    private InputAction _dash;
    private bool _canDash = true;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _playerControls = new PlayerControls();
        _rb = GetComponent<Rigidbody2D>();
    }
    

    private void OnEnable()
    {
        _move = _playerControls.Movement.Walk;
        _move.Enable();

        _dash = _playerControls.Movement.Dash;
        _dash.Enable();
        _dash.performed += Dash;
    }

    private void OnDisable()
    {
        _move.Disable();

        _dash.Disable();
    }
    
    private void Update()
    {
        _moveDirection = _move.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if(!_canDash)
            return;
        var acceleration = (_moveDirection * moveSpeed - _rb.velocity) / 3f;
        _rb.velocity += acceleration * Time.deltaTime * 100;
    }

    private void Dash(InputAction.CallbackContext context)
    {
        if(!_canDash)
            return;
        _canDash = false;
        var dashTarget = _rb.position + _moveDirection * dashDistance;
        _rb.DOMove(dashTarget, dashTime).SetEase(Ease.InOutQuint);
        _canDash = true;
    }
}
