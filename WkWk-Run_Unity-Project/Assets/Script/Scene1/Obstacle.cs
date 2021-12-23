using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private string ObstacleName;
    private Transform destroyPoint;

    private GameManager manager;
    private Client network;

    private bool isStartRolling;
    private float rollingSpeed = 3.5f;

    // Start is called before the first frame update
    void Start()
    {
        manager = FindObjectOfType<GameManager>();
        network = FindObjectOfType<Client>();
        destroyPoint = FindObjectOfType<CameraFollow>().PlatformDestroyerPoint;
    }

    // Update is called once per frame
    void Update()
    {
        if (manager.GameIsStarted)
        {
            // Destroy this object if it wasn't needed
            if(transform.position.y < destroyPoint.position.y)
            {
                Destroy(gameObject);
            }

            if (isStartRolling)
            {
                transform.position = new Vector2(transform.position.x, (transform.position.y - rollingSpeed * Time.deltaTime));
                try
                {
                    GetComponent<Animator>().SetFloat("Speed", rollingSpeed);
                }
                catch
                {

                }
            }
        }
    }

    // Method for moving obstacle
    public void StartRolling()
    {
        isStartRolling = true;   
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
        {
            PlayerManager player = collision.gameObject.GetComponent<PlayerManager>();
            if(player.playerName == network.MyName)
            {
                player.Dead(ObstacleName);
            }
        }
        else
        {
            Debug.Log("Collision Obstacle : " + collision.collider.tag);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            if(ObstacleName == "Water")
            {
                collision.GetComponent<PlayerManager>().IsSwimming(true);
            }
            else if(ObstacleName == "Water End" || ObstacleName == "Bridge")
            {
                collision.GetComponent<PlayerManager>().IsSwimming(false);
            }
            else if(ObstacleName == "Ball" || ObstacleName == "Log" || ObstacleName == "Lava")
            {
                PlayerManager player = collision.gameObject.GetComponent<PlayerManager>();
                if (player.playerName == network.MyName)
                {
                    player.Dead(ObstacleName);
                }
            }
        }
    }
}
