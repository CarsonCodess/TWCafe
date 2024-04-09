using System;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[HideMonoScript]
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

        _playerControls.Movement.Dash.performed += Dash;
        _playerControls.Movement.Drop.performed += Drop;
        _playerControls.Movement.Throw.performed += Throw;
    }

    private void Drop(InputAction.CallbackContext c)
    {
        if(!IsOwner)
            return;
        OnDrop?.Invoke();
    }
    
    private void Dash(InputAction.CallbackContext c)
    {
        if(!IsOwner)
            return;
        OnDash?.Invoke();
    }
    
    private void Throw(InputAction.CallbackContext c)
    {
        if(!IsOwner)
            return;
        OnThrow?.Invoke();
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
