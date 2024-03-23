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
    private WaveManager _waveManager;
    private GameObject _seat;
    private int _orderInQueue;
    private GameObject _queuePosition;
    private Tweener _moveToQueueTween;
    private NetworkVariable<bool> _isReady = new NetworkVariable<bool>();
    private NetworkVariable<bool> _hasTakenOrder = new NetworkVariable<bool>();
    private NetworkVariable<bool> _isAngry = new NetworkVariable<bool>();
    private float _angryProgress;

    public void Initialize(int seat, WaveManager waveManager)
    {
        _waveManager = waveManager;
        _seat = seat != -1 ? waveManager.seats[seat] : null;
        WalkTowardsTable();
        UpdateUI();
    }
    
    private void WalkTowardsTable()
    {
        if(!IsHost)
            return;
        if(_seat)
            _waveManager.RemoveSeat(_seat);
        if (!_seat)
        {
            if (_waveManager.IsQueueFull())
                transform.DOMove(new Vector3(0f, transform.position.y, 0f), 5f).SetEase(Ease.Linear).OnComplete(() => LeaveCafe());
            else
                _waveManager.AddToQueue(this);
        }
        else
            transform.DOMove(new Vector3(_seat.transform.position.x, transform.position.y, _seat.transform.position.z), 5f).SetEase(Ease.Linear).OnComplete(() =>
            {
                ReadyServerRpc();
                Invoke(nameof(StartAngryTimerServerRpc), timeBeforeAngry);
            });
    }

    private void LeaveTable()
    {
        LeaveCafe(true);
        _waveManager.MoveQueue();
    }

    private void LeaveCafe(bool leavingFromTable = false)
    {
        if(!IsHost)
            return;
        if (_seat)
            _waveManager.AddSeat(_seat);

        transform.DOMove(new Vector3(_waveManager.transform.position.x, transform.position.y, _waveManager.transform.position.z), 5f).OnComplete(() => {
            GetComponent<NetworkObject>().Despawn();
        }).OnUpdate(() =>
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
        var isMovingToQueue = _moveToQueueTween != null;
        if (_moveToQueueTween == null)
        {
            _moveToQueueTween.Kill();
            _moveToQueueTween = null;
        }

        _orderInQueue--;
        _queuePosition = _waveManager.waitingQueuePositions[_orderInQueue];
        transform.DOMove(new Vector3(_queuePosition.transform.position.x, transform.position.y, _queuePosition.transform.position.z), isMovingToQueue ? 5f : 3f);
    }

    public void AddToQueue(int order, GameObject position)
    {
        _orderInQueue = order;
        _queuePosition = position;
        _moveToQueueTween = transform.DOMove(_queuePosition.transform.position, 5f).OnComplete(() =>
        {
            _moveToQueueTween.Kill();
            _moveToQueueTween = null;
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

    protected override void OnUpdate(PlayerController player)
    {
        if(player.IsPressingInteract() && !_hasTakenOrder.Value && _isReady.Value)
            TakeOrderServerRpc();
        else if (player.IsPressingInteract() && _hasTakenOrder.Value && _isReady.Value && player.GetItem() != 0)
        {
            //Check if correct item
            player.Drop();
            StopAngryTimerServerRpc();
            ReadyServerRpc(false);
            Invoke(nameof(LeaveTable), timeFinishEating);
        }
    }

    protected override void Update()
    {
        if(!IsHost)
            return;
        base.Update();
        if (_isAngry.Value)
        {
            _angryProgress += Time.deltaTime / timeUntilLeave;
            angryBarFill.rectTransform.sizeDelta = new Vector2(-Mathf.Lerp(0, 2, 1 - _angryProgress), angryBarFill.rectTransform.sizeDelta.y);
            angryBarFill.rectTransform.anchoredPosition = new Vector2(-Mathf.Lerp(0, 2, 1 - _angryProgress) / 2, angryBarFill.rectTransform.anchoredPosition.y);
            if (_angryProgress >= 1f)
            {
                StopAngryTimerServerRpc();
                ReadyServerRpc(false);
                LeaveTable();
            }
        }
        UpdateUI();
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
        _isAngry.Value = false;
        _angryProgress = 0f;
    }
}
