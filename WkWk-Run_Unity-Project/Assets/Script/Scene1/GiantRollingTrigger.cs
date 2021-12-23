using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiantRollingTrigger : MonoBehaviour
{
    [SerializeField] GameObject rolling;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            rolling.GetComponent<Obstacle>().StartRolling();
        }
    }
}
