using DG.Tweening;
using UnityEngine;

public class Customer : MonoBehaviour
{
    private Rigidbody2D _rb;
    private WaveManager _waveManager;
    private GameObject _seat;

    public void Initialize(GameObject seat, WaveManager waveManager)
    {
        _waveManager = waveManager;
        _seat = seat;
        _rb = GetComponent<Rigidbody2D>();
        Invoke(nameof(LeaveTable), 12f);
        WalkTowardsTable();
    }
    
    private void WalkTowardsTable()
    {
        if(_seat)
            _waveManager.RemoveSeat(_seat);
        transform.DOMove(_seat != null ? _seat.transform.position : Vector3.zero, 5f).SetEase(Ease.Linear).OnComplete(() => 
        {
            if(!_seat)
                LeaveTable();
        });
    }

    private void LeaveTable()
    {
        if(_seat)
            _waveManager.AddSeat(_seat);
        transform.DOMove(_waveManager.transform.position, 5f).OnComplete(() => {
            Destroy(gameObject);
        });
    }
}
