using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static Extensions;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float dashCooldown = 2f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashTime = 0.25f;
    [SerializeField] private ParticleSystem footsteps;
    [SerializeField] private Transform dropTarget;
    [SerializeField] private GameObject holdingItemModel;
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private GameObject baseItemPrefab;

    private bool _canDash = true;
    private float _dashTimer = 0;
    private bool _isDashing = false;
    private bool _isEmoting = false;
    private int _emote;
    private Vector2 _moveDirection;
    private Rigidbody _rb;
    private AnimationHandler _animHandler;
    private InputHandler _inputHandler;
    private NetworkList<int> _equippedItem = new NetworkList<int>(DefaultEmptyList(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> _interacting = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animHandler = GetComponent<AnimationHandler>();
        
    }

    private void OnEnable()
    {
        if(!_inputHandler)
            _inputHandler = GetComponent<InputHandler>();
        _inputHandler.OnMove += OnMove;
        _inputHandler.OnDrop += DropAndSpawnItem;
        _inputHandler.OnThrow += Throw;
        _inputHandler.OnDash += Dash;
    }

    private void OnDisable()
    {
        _inputHandler.OnMove -= OnMove;
        _inputHandler.OnDrop -= DropAndSpawnItem;
        _inputHandler.OnThrow -= Throw;
        _inputHandler.OnDash -= Dash;
    }

    public void OnMove(Vector2 moveDir)
    {
        _moveDirection = moveDir;
        var acceleration = (_moveDirection * moveSpeed - new Vector2(_rb.velocity.x, _rb.velocity.z)) / 3f;
        _rb.velocity += new Vector3(acceleration.x * Time.deltaTime * 100, 0f, acceleration.y * Time.deltaTime * 100);
    }
    
    private void Update()
    {
        holdingItemModel.SetActive(_equippedItem[0] != 0);
        if(!IsOwner)
            return;

        if (!_isDashing)
        {
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
        
        if (_equippedItem[0] == 0)
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

    private void Dash()
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

    public void Pickup(List<int> item)
    {
        if(!IsOwner)
            return;
        DOVirtual.Float(0f, 1f, 0.1f, _ => {}).OnComplete(() => { PickupItem(item); });
        _isEmoting = false;
    }

    private void PickupItem(List<int> item)
    {
        _equippedItem.Clear();
        foreach (var id in item)
            _equippedItem.Add(id);
        holdingItemModel.SetActive(true);
        var itemSo = GameManager.Instance.GetItemObject(item[0]);
        holdingItemModel.GetComponent<MeshFilter>().mesh = itemSo.mesh;
        holdingItemModel.GetComponent<MeshRenderer>().material = itemSo.material;
    }

    public void DropAndSpawnItem()
    {
        if(_equippedItem[0] == 0 || !IsOwner)
            return;
        DropAndSpawnItemServerRpc();
    }

    [ServerRpc]
    private void DropAndSpawnItemServerRpc()
    {
        SpawnItem(dropTarget.position);
    }

    private GameObject SpawnItem(Vector3 pos)
    {
        var itemObject = Instantiate(baseItemPrefab, pos, Quaternion.identity);
        itemObject.GetComponent<NetworkObject>().Spawn();
        itemObject.GetComponent<Pickup>().Initialize(_equippedItem.ToList());
        Drop();
        return itemObject;
    }

    public void Throw()
    {
        if(_equippedItem[0] == 0 || !IsOwner)
            return;
        ThrowServerRpc();
        _animHandler.Stop();
        _animHandler.Play("Throw", 0.15f);
    }

    [ServerRpc]
    private void ThrowServerRpc()
    {
        var itemObject = SpawnItem(holdingItemModel.transform.position);
        itemObject.GetComponent<Rigidbody>().AddForce(transform.forward * throwForce, ForceMode.Impulse);
    }
    
    public void Drop()
    {
        _equippedItem.Clear();
        _equippedItem.Add(0);
        holdingItemModel.SetActive(false);
        _isEmoting = false;
    }

    public int GetBaseItem()
    {
        return _equippedItem[0];
    }
    
    public List<int> GetEntireItem()
    {
        return _equippedItem.ToList();
    }

    public bool IsPressingInteract()
    {
        return _interacting.Value;
    }
}
