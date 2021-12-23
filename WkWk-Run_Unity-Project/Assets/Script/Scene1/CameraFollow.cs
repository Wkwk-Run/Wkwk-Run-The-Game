using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private GameManager manager;
    public Transform PlatformDestroyerPoint;

    public Transform playerPos { get; set; }
    private float offsetY = 2f;
    private float smoothFactor = 8f;

    // Start is called before the first frame update
    void Start()
    {
        manager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (manager.GameIsStarted)
        {
            // Follow player
            Vector3 targetPos = new Vector3(transform.position.x, playerPos.position.y + offsetY, transform.position.z);
            Vector3 smoothPos = Vector3.Lerp(transform.position, targetPos, smoothFactor * Time.fixedDeltaTime);
            transform.position = smoothPos;
        }
    }
}
