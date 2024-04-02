using System;
using DG.Tweening;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Customer : Interactable
{
    [SerializeField] private GameObject angryBar;
    [SerializeField] private Image angryBarFill;
    [SerializeField] private GameObject orderPopup;
    [SerializeField] private float timeBeforeAngry = 25f;
    [SerializeField] private float timeUntilLeave = 10f;
    [SerializeField] private float timeFinishEating = 5f;
    [SerializeField] private float baseSpeed = 5f;
    private WaveManager _waveManager;
    private AnimationHandler _animHandler;
    private GameObject _seat;
    private bool _hasAvailableSeat;
    private int _orderInQueue;
    private GameObject _queuePosition;
    private Tweener _moveTween;
    private NetworkVariable<bool> _isReady = new NetworkVariable<bool>();
    private NetworkVariable<bool> _hasTakenOrder = new NetworkVariable<bool>();
    private NetworkVariable<bool> _isAngry = new NetworkVariable<bool>();
    private float _angryProgress;

    public void Initialize(int seat, WaveManager waveManager)
    {
        _waveManager = waveManager;
        _animHandler = GetComponent<AnimationHandler>();
        if (seat == -1)
        {
            _seat = null;
            _hasAvailableSeat = false;
        }
        else
        {
            _seat = waveManager.seats[seat];
            _hasAvailableSeat = true;
        }

        WalkTowardsTable();
        UpdateUI();
    }
    
    
    private void WalkTowardsTable()
    {
        if(!IsHost)
            return;
        if (!_hasAvailableSeat)
        {
            if (_waveManager.IsQueueFull())
                Move(Vector3.zero, () => LeaveCafe());
            else
                _waveManager.AddToQueue(this);
        }
        else
        {
            _waveManager.ReserveSeat(_seat);
            Move(_seat.transform.position, () =>
            {
                ReadyServerRpc();
                Invoke(nameof(StartAngryTimerServerRpc), timeBeforeAngry);
            });
        }
    }

    //[ServerRpc(RequireOwnership = false)]
    private void LeaveTable()
    {
        LeaveCafe(true);
        _waveManager.MoveQueue();
    }

    private void LeaveCafe(bool leavingFromTable = false)
    {
        if(!IsHost)
            return;
        if (_hasAvailableSeat)
            _waveManager.OpenUpSeat(_seat);
        Move(_waveManager.transform.position,() =>
        {
            transform.DOKill();
            GetComponent<NetworkObject>().Despawn();
        }, () =>
        {
            if (!_waveManager.IsQueueFull() && !leavingFromTable)
            {
                transform.DOKill();
                _waveManager.AddToQueue(this);
            }
        });
    }

    public void MoveQueue()
    {
        if (_orderInQueue <= 0)
            return;
        
        _orderInQueue--;
        _queuePosition = _waveManager.waitingQueuePositions[_orderInQueue];
        transform.DOKill();
        Move(_queuePosition.transform.position, () =>
        {
            LookAt(transform.position + Vector3.left);
        });
    }

    public void AddToQueue(int order, GameObject position)
    {
        _orderInQueue = order;
        _queuePosition = position;
        Move(_queuePosition.transform.position, () =>
        {
            LookAt(transform.position + Vector3.left);
        });
    }

    [ServerRpc(RequireOwnership = false)]
    private void ReadyServerRpc(bool ready = true)
    {
        _isReady.Value = ready;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void TakeOrderServerRpc()
    {
        _hasTakenOrder.Value = true;
        StopAngryTimerServerRpc();
        Invoke(nameof(StartAngryTimerServerRpc), timeBeforeAngry);
    }

    protected override void OnUpdate(Player player)
    {
        if(player.IsPressingInteract() && !_hasTakenOrder.Value && _isReady.Value)
            TakeOrderServerRpc();
        else if (player.IsPressingInteract() && _hasTakenOrder.Value && _isReady.Value && player.GetBaseItem() != 0)
        {
            //Check if correct item
            player.Drop();
            ServeServerRpc();
        }
    }

    
    [ServerRpc(RequireOwnership = false)]
    private void ServeServerRpc()
    {
        StopAngryTimerServerRpc();
        ReadyServerRpc(false);
        Invoke(nameof(LeaveTable), timeFinishEating);
    }

    protected override void Update()
    {
        base.Update();
        if (_isAngry.Value)
        {
            _angryProgress += Time.deltaTime / timeUntilLeave;
            angryBarFill.rectTransform.sizeDelta = new Vector2(-Mathf.Lerp(0, 2, 1 - _angryProgress), angryBarFill.rectTransform.sizeDelta.y);
            angryBarFill.rectTransform.anchoredPosition = new Vector2(-Mathf.Lerp(0, 2, 1 - _angryProgress) / 2, angryBarFill.rectTransform.anchoredPosition.y);
            if (_angryProgress >= 1f)
            {
                if(!IsHost)
                    return;
                StopAngryTimerServerRpc();
                ReadyServerRpc(false);
                LeaveTable();
            }
        }
        UpdateUI();
        
        if(!IsHost)
            return;
        _animHandler.SetParameter("Move", _moveTween != null ? 1f : 0f, 0.25f);
    }

    private void UpdateUI()
    {
        orderPopup.SetActive(_hasTakenOrder.Value && _isReady.Value);
        angryBar.SetActive(_isAngry.Value);
        orderPopup.transform.LookAt(Camera.main.transform);
        angryBar.transform.LookAt(Camera.main.transform);
    }
    
    public bool IsFirstInQueue()
    {
        return _orderInQueue == 0;
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartAngryTimerServerRpc()
    {
        _isAngry.Value = true;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void StopAngryTimerServerRpc()
    {
        CancelInvoke();
        _isAngry.Value = false;
        _angryProgress = 0f;
    }

    private void Move(Vector3 targetPos, TweenCallback onComplete = null, TweenCallback onUpdate = null)
    {
        LookAt(targetPos);
        var duration = Vector3.Distance(transform.position, targetPos) / baseSpeed;
        _moveTween = transform.DOMove(targetPos.ReplaceY(transform.position.y), duration).SetEase(Ease.Linear).OnComplete(() =>
        {
            _moveTween = null;
            onComplete?.Invoke();
        }).OnUpdate(() =>
        {
            if (_moveTween.target == null)
            {
                _moveTween.Kill();
                _moveTween = null;
            }
            onUpdate?.Invoke();
            LookAt(targetPos);
        }).SetAutoKill(true);
    }

    private void LookAt(Vector3 target)
    {
        if(transform != null && target - transform.position != Vector3.zero)
            transform.DORotateQuaternion(Quaternion.LookRotation(target - transform.position), 1f);
    }
}
