using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WaveManager : NetworkBehaviour
{
    [SerializeField] private WeightedObjectList<GameObject> customers = new WeightedObjectList<GameObject>();
    public List<GameObject> seats = new List<GameObject>();
    public List<GameObject> waitingQueuePositions = new List<GameObject>();
    private List<Customer> _waitingCustomers = new List<Customer>();
    private int _lastWaitingPosition;

    private void Start()
    {
        if(IsHost)
            InvokeRepeating(nameof(SpawnCustomer), 0f, 4f);
    }

    private void SpawnCustomer()
    {
        var seat = !IsQueueFull() && seats.Count > 0 ? Random.Range(0, seats.Count) : -1;
        var customer = Instantiate(customers.GetRandomObject(), transform.position, Quaternion.identity);
        customer.GetComponent<NetworkObject>().Spawn(true);
        customer.GetComponent<Customer>().Initialize(seat, this);
    }
    
    public void OpenUpSeat(GameObject seat)
    {
        seats.Add(seat);
    }

    public void ReserveSeat(GameObject seat)
    {
        seats.Remove(seat);
    }

    public void MoveQueue()
    {
        if(!IsHost)
            return;
        _lastWaitingPosition--;
        foreach (var customer in _waitingCustomers)
        {
            if (customer.IsFirstInQueue())
            {
                var seat = Random.Range(0, seats.Count);
                customer.Initialize(seat, this);
            }

            customer.MoveQueue();
        }

        _waitingCustomers.Remove(_waitingCustomers[0]);
    }

    public void AddToQueue(Customer customer)
    {
        if(!IsHost)
            return;
        _waitingCustomers.Add(customer);
        customer.AddToQueue(_lastWaitingPosition, waitingQueuePositions[_lastWaitingPosition]);
        _lastWaitingPosition++;
    }

    public bool IsQueueFull()
    {
        return _lastWaitingPosition == waitingQueuePositions.Count;
    }
}
