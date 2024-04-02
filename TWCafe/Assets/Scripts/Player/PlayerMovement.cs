using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private ParticleSystem footsteps;

    private bool _isEmoting = false;
    private int _emote;
    private Vector2 _moveDirection;
    private Rigidbody _rb;
    private AnimationHandler _animHandler;
    private InputHandler _inputHandler;
    private PlayerDash _playerDash;
    private Player _player;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animHandler = GetComponent<AnimationHandler>();
        _player = GetComponent<Player>();
        TryGetComponent(out _playerDash);
        if (TryGetComponent(out _inputHandler))
            _inputHandler.OnMove += OnMove;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (_inputHandler)
            _inputHandler.OnMove -= OnMove;
    }
    
    public void OnMove(Vector2 moveDir)
    {
        _moveDirection = moveDir;
        var acceleration = (_moveDirection * moveSpeed - new Vector2(_rb.velocity.x, _rb.velocity.z)) / 3f;
        _rb.velocity += new Vector3(acceleration.x * Time.deltaTime * 100, 0f, acceleration.y * Time.deltaTime * 100);
    }
    
    private void Update()
    {
        if(!IsOwner)
            return;

        if (!IsDashing())
        {
            if(Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.digit2Key.wasPressedThisFrame && !_isEmoting)
                _isEmoting = true;
            else if (_moveDirection.x != 0f || _moveDirection.y != 0f) // Moving
                _isEmoting = false;
            _emote = Keyboard.current.digit1Key.wasPressedThisFrame ? 1 : 2;
        }
        UpdatePlayerAnimation();
    }

    private void UpdatePlayerAnimation()
    {
        if (IsDashing())
        {
            _animHandler.SetParameter("Action", -1f, 0.15f);
            _animHandler.SetParameter("Holding", 0f, 0.15f);
            return;
        }

        if (_isEmoting)
        {
            _animHandler.SetParameter("Holding", -1f, 0.15f);
            _animHandler.SetParameter("Action", _emote == 1 ? 0f : -1f, 0.15f);
            return;
        }
        
        if (_player.GetBaseItem() == 0)
            _animHandler.SetParameter("Holding", 0f, 0.15f);
        else
            _animHandler.SetParameter("Holding", 1f, 0.15f);
        
        if (_moveDirection.x != 0f || _moveDirection.y != 0f) // Moving
        {
            _animHandler.SetParameter("Action", 1f, 0.15f);
            if(!footsteps.isPlaying)
                footsteps.Play();
            if(new Vector3(_rb.velocity.x, 0f, _rb.velocity.z) != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(new Vector3(_rb.velocity.x, 0f, _rb.velocity.z)), Time.deltaTime * 15f);
        }
        else
        {
            _animHandler.SetParameter("Action", 0f, 0.15f);
            if(footsteps.isPlaying)
                footsteps.Stop();
        }
    }

    private bool IsDashing()
    {
        return _playerDash != null && _playerDash.IsDashing;
    }
}
