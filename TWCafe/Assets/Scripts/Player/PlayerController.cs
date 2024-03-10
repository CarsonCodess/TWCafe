using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.IO.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashTime = 0.25f;
    
    private PlayerControls _playerControls;
    private Vector2 _moveDirection = Vector2.zero;
    private InputAction _move;
    private InputAction _dash;
    private bool _canDash = true;
    private float _dashTimer = 0;
    private bool _isDashing = false;
    private Rigidbody2D _rb;
    private FoodItem _equippedItem;

    private void Awake()
    {
        _playerControls = new PlayerControls();
        _rb = GetComponent<Rigidbody2D>();
    }
    

    private void OnEnable()
    {
        _playerControls.Enable(); 
        _move = _playerControls.Movement.Walk;
        _dash = _playerControls.Movement.Dash;
        _dash.performed += Dash;
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }
    
    private void Update()
    {
        if(!IsOwner)
            return;
        _moveDirection = _move.ReadValue<Vector2>();
        _dashTimer += Time.deltaTime;
        if(_dashTimer >= 4){
            _canDash = true;
        }
    }

    private void FixedUpdate()
    {
        if(_isDashing)
            return;
        var acceleration = (_moveDirection * moveSpeed - _rb.velocity) / 3f;
        _rb.velocity += acceleration * Time.deltaTime * 100;
    }

    private void Dash(InputAction.CallbackContext context)
    {
        if(!_canDash)
            return;
        if(_canDash && _dashTimer >= 4){
            _isDashing = true;
            var dashTarget = _rb.position + _moveDirection * dashDistance;
            _rb.DOMove(dashTarget, dashTime).SetEase(Ease.InOutQuint).OnComplete (() => {
                _isDashing = false;
            });
            _canDash = false;
            _dashTimer = 0;
        }
    }

    public void Pickup(FoodItem item)
    {
        _equippedItem = item;
    }
    
    public void Drop()
    {
        _equippedItem = null;
    }

    public FoodItem GetItem()
    {
        return _equippedItem;
    }
}
