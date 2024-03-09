using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Netcode;
using UnityEngine;

public class WaveManager : NetworkBehaviour
{
    [SerializeField] private WeightedObjectList<GameObject> customers = new WeightedObjectList<GameObject>();
    public List<GameObject> seats = new List<GameObject>();
    private int _wave;

    private void Start()
    {
        _wave = customers.Count();
        if(NetworkManager.IsHost)
            InvokeRepeating(nameof(SpawnCustomer), 0f, 4f);
    }

    private void SpawnCustomer()
    {
        var seat = Random.Range(0, seats.Count);
        var customer = Instantiate(customers.GetRandomObject(), transform.position, Quaternion.identity);
        customer.GetComponent<NetworkObject>().Spawn(true);
        customer.GetComponent<Customer>().Initialize(seat, this);
    }
    
    public void AddSeat(GameObject seat)
    {
        seats.Add(seat);
    }

    public void RemoveSeat(GameObject seat)
    {
        seats.Remove(seat);
    }
}
