using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class Customer : NetworkBehaviour
{
    private WaveManager _waveManager;
    private GameObject _seat;
    private int _orderInQueue;
    private GameObject _queuePosition;
    private Tweener _moveToQueueTween;

    public void Initialize(int seat, WaveManager waveManager)
    {
        _waveManager = waveManager;
        _seat = seat != -1 ? waveManager.seats[seat] : null;
        WalkTowardsTable();
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
                transform.DOMove(Vector2.zero, 5f).SetEase(Ease.Linear).OnComplete(() => LeaveCafe());
            else
                _waveManager.AddToQueue(this);
                
        }
        else
        {
            transform.DOMove(_seat.transform.position, 5f).SetEase(Ease.Linear).OnComplete(() =>
            {
                Invoke(nameof(LeaveTable), 15f);
            });
        }
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
        if(_seat)
            _waveManager.AddSeat(_seat);
        transform.DOMove(_waveManager.transform.position, 5f).OnComplete(() => {
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

    public bool IsFirstInQueue()
    {
        return _orderInQueue == 0;
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
        transform.DOMove(_queuePosition.transform.position, isMovingToQueue ? 5f : 3f);
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
}
