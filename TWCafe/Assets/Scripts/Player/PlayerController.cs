using System;
using DG.Tweening;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float dashCooldown = 2f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashTime = 0.25f;
    [SerializeField] private ParticleSystem footsteps;
    [SerializeField] private Transform dropTarget;
    [SerializeField] private GameObject holdingItemModel;
    [SerializeField] private float throwForce = 10f;

    private PlayerControls _playerControls;
    private Vector2 _moveDirection = Vector2.zero;
    private InputAction _move;
    private bool _canDash = true;
    private float _dashTimer = 0;
    private bool _isDashing = false;
    private bool _isEmoting = false;
    private int _emote;
    private Rigidbody _rb;
    private AnimationHandler _animHandler;
    private NetworkVariable<int> _equippedItem = new NetworkVariable<int>();
    private NetworkVariable<bool> _interacting = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        _playerControls = new PlayerControls();
        _rb = GetComponent<Rigidbody>();
        _animHandler = GetComponent<AnimationHandler>();
    }
    
    private void OnEnable()
    {
        _playerControls.Enable(); 
        _move = _playerControls.Movement.Walk;
        _playerControls.Movement.Dash.performed += Dash;
        _playerControls.Movement.Drop.performed += DropAndSpawnItem;
        _playerControls.Movement.Throw.performed += Throw;
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }
    
    private void Update()
    {
        holdingItemModel.SetActive(_equippedItem.Value != 0);
        if(!IsOwner)
            return;

        if (!_isDashing)
        {
            UpdatePlayerMovement();
            _dashTimer += Time.deltaTime;
            if(_dashTimer >= dashCooldown)
                _canDash = true;
            _interacting.Value = Keyboard.current.eKey.wasPressedThisFrame;
            if(Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.digit2Key.wasPressedThisFrame && !_isEmoting)
                _isEmoting = true;
            else if (_moveDirection.x != 0f || _moveDirection.y != 0f) // Moving
                _isEmoting = false;
            _emote = Keyboard.current.digit1Key.wasPressedThisFrame ? 1 : 2;
        }
        UpdatePlayerAnimation();
    }

    private void UpdatePlayerMovement()
    {
        _moveDirection = _move.ReadValue<Vector2>();
        var acceleration = (_moveDirection * moveSpeed - new Vector2(_rb.velocity.x, _rb.velocity.z)) / 3f;
        _rb.velocity += new Vector3(acceleration.x * Time.deltaTime * 100, 0f, acceleration.y * Time.deltaTime * 100);
    }

    private void UpdatePlayerAnimation()
    {
        if (_isDashing)
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
        
        if (_equippedItem.Value == 0)
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

    private void Dash(InputAction.CallbackContext context)
    {
        if(!_canDash)
            return;
        if(_canDash)
        {
            _isDashing = true;
            _isEmoting = false;
            var dashTarget = new Vector3(transform.position.x + (_moveDirection.x * dashDistance), transform.position.y, transform.position.z + (_moveDirection.y * dashDistance));
            _rb.transform.DOMove(dashTarget, dashTime).SetEase(Ease.InOutQuint).OnComplete (() => {
                _isDashing = false;
            });
            _canDash = false;
            _dashTimer = 0;
        }
    }

    public void Pickup(int item)
    {
        if(!IsOwner)
            return;
        DOVirtual.Float(0f, 1f, 0.1f, _ => {}).OnComplete(() =>
        {
            SetEquippedItemServerRpc(item);
            holdingItemModel.SetActive(true);
            var itemSo = GameManager.Instance.GetItemObject(item);
            holdingItemModel.GetComponent<MeshFilter>().mesh = itemSo.prefab.GetComponentInChildren<MeshFilter>().sharedMesh;
            holdingItemModel.GetComponent<MeshRenderer>().material = itemSo.prefab.GetComponentInChildren<MeshRenderer>().sharedMaterial;
            holdingItemModel.transform.localPosition = itemSo.holdingOffset;
        });
        _isEmoting = false;
    }

    public void DropAndSpawnItem(InputAction.CallbackContext context)
    {
        if(_equippedItem.Value == 0 || !IsOwner)
            return;
        DropAndSpawnItemServerRpc();
    }

    [ServerRpc]
    private void DropAndSpawnItemServerRpc()
    {
        var itemObject = Instantiate(GameManager.Instance.GetItemObject(_equippedItem.Value).prefab, dropTarget.position, Quaternion.identity);
        itemObject.GetComponent<NetworkObject>().Spawn();
        SetEquippedItemServerRpc(0);
        holdingItemModel.SetActive(false);
        _isEmoting = false;
    }
    
    public void Throw(InputAction.CallbackContext context)
    {
        if(_equippedItem.Value == 0 || !IsOwner)
            return;
        ThrowServerRpc();
        _animHandler.Stop();
        _animHandler.Play("Throw", 0.15f);
    }

    [ServerRpc]
    private void ThrowServerRpc()
    {
        var itemObject = Instantiate(GameManager.Instance.GetItemObject(_equippedItem.Value).prefab, holdingItemModel.transform.position, Quaternion.identity);
        itemObject.GetComponent<NetworkObject>().Spawn();
        itemObject.GetComponent<Rigidbody>().AddForce(transform.forward * throwForce, ForceMode.Impulse);
        SetEquippedItemServerRpc(0);
        holdingItemModel.SetActive(false);
        _isEmoting = false;
    }
    
    public void Drop()
    {
        SetEquippedItemServerRpc(0);
        holdingItemModel.SetActive(false);
        _isEmoting = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetEquippedItemServerRpc(int item)
    {
        _equippedItem.Value = item;
    }
    
    public int GetItem()
    {
        return _equippedItem.Value;
    }

    public bool IsPressingInteract()
    {
        return _interacting.Value;
    }
}
