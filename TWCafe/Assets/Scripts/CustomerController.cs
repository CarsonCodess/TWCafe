using System.Collections;
using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using UnityEngine;

public class CustomerController : MonoBehaviour
{
    [SerializeField] List<GameObject> seats = new List<GameObject>();
    List<GameObject> seatsBackup = new List<GameObject>();
    Rigidbody2D rb;

    void Start()
    {
        foreach(var seat in seats){
            seatsBackup.Add(seat);
        }
        rb = GetComponent<Rigidbody2D>();
        Invoke(nameof(LeaveTable), 10);
    }


    void Update()
    {
        foreach (var seat in seats){
            if(seatsBackup.Contains(seat)){
                ToTable(seat);
            }
        }
    }
                                    //NOT SURE IF TWO CUSTOMERS WORK AT ONCE BUT THIS IS AN ISSUE FOR FUTURE ME :)
    private void ToTable(GameObject seat){
        transform.DOMove(seat.transform.position, 5f).OnComplete(() => {
            seats.Remove(seat);
        });
    }

    private void LeaveTable()
    {
        Debug.Log("Running LeaveTable Method");
        for(var i = 0; i < seatsBackup.Count; i++){
            if(seats.Count == 0){
                seats.Add(seatsBackup[0]);
                Destroy(gameObject);
            }
            if(seats[i] != seatsBackup[i]){
                seats.Insert(i, seatsBackup[i]);
                Destroy(gameObject);
            }
        }
    }
}
