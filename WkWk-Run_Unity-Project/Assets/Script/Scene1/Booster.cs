using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Booster : MonoBehaviour
{
    [SerializeField] private string PlatformName;
    private Transform destroyPoint;

    private GameManager manager;
    private Client network;

    private int coinValue;

    // Start is called before the first frame update
    void Start()
    {
        manager = FindObjectOfType<GameManager>();
        network = FindObjectOfType<Client>();
        destroyPoint = FindObjectOfType<CameraFollow>().PlatformDestroyerPoint;
        
        // Set coin value
        if(transform.position.y < manager.LevelDistance)
        {
            coinValue = 10;
        }
        else if (transform.position.y < manager.LevelDistance * 2)
        {
            coinValue = 15;
        }
        else if (transform.position.y < manager.LevelDistance * 3)
        {
            coinValue = 20;
        }
        else if (transform.position.y < manager.LevelDistance * 4)
        {
            coinValue = 25;
        }
        else if (transform.position.y < manager.LevelDistance * 5)
        {
            coinValue = 30;
        }
        else
        {
            coinValue = 10;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (manager.GameIsStarted)
        {
            // Destroy this object if it wasn't needed
            if (transform.position.y < destroyPoint.position.y)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            if (PlatformName == "Coin")
            {
                // Add coin to player
                PlayerManager player = collision.GetComponent<PlayerManager>();
                if (player.playerName == network.MyName)
                {
                    player.GetCoin(coinValue);
                }
            }
            else if (PlatformName == "Fast")
            {
                // Faster movement player
                PlayerManager player = collision.GetComponent<PlayerManager>();
                if (player.playerName == network.MyName)
                {
                    player.FastMovement();
                }
            }

            // Destroy object
            Destroy(gameObject);
        }
        else if (collision.tag == "Obstacle")
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Obstacle Booster Collision : " + collision.tag);
        }
    }
}
