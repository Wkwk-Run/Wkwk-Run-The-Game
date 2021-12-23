using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawnPoint : MonoBehaviour
{
    private GameManager manager;

    private float speed = 8f;

    void Start()
    {
        manager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        // Only move when game is on
        if(manager.PlatformSpawningIsStarted && !manager.GameIsFinished && transform.position.y < manager.FinishPoint.position.y + 22)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y + (speed * Time.deltaTime));     
        }
    }
}
