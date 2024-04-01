using System;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public event Action OnDash;
    public event Action OnDrop;
    public event Action OnThrow;
    public event Action<Vector2> OnMove;

    private PlayerControls _playerControls;

    private void Awake()
    {
        _playerControls = new PlayerControls();
    }
    
    private void OnEnable()
    {
        _playerControls.Enable(); 

        _playerControls.Movement.Dash.performed += c => OnDash?.Invoke();
        _playerControls.Movement.Drop.performed += c => OnDrop?.Invoke();
        _playerControls.Movement.Throw.performed += c => OnThrow?.Invoke();
    }

    private void Update()
    {
        var move = _playerControls.Movement.Walk.ReadValue<Vector2>();
        OnMove?.Invoke(move);
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }
}
