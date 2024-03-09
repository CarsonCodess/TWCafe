using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class WaveManager : MonoBehaviour
{

    public WeightedObjectList<GameObject> WOL = new WeightedObjectList<GameObject>();
    public int wave;

    void Start()
    {
        wave = WOL.Count();
        StartCoroutine(Spawn());
    }

    void Update()
    {
        
    }

    IEnumerator Spawn(){
        while(wave > 0){
            SpawnCustomer();
            yield return new WaitForSeconds(2f);
            wave--;
        }
    }

    private void SpawnCustomer(){
        Instantiate(WOL.GetRandomObject(), transform.position, Quaternion.identity);
    }
}
