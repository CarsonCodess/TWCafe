using System;
using Unity.Netcode;
using UnityEngine;

public class InputHandler : NetworkBehaviour
{
    public Vector2 MovementDirection { get; private set; }
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
        if(!IsOwner)
            return;
        MovementDirection = _playerControls.Movement.Walk.ReadValue<Vector2>();
        OnMove?.Invoke(MovementDirection);
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }
}
