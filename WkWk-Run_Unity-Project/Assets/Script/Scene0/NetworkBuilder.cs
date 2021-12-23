using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkBuilder : MonoBehaviour
{
    [SerializeField] private GameObject NetworkClient;
    [SerializeField] private GameObject AudioPrefab;

    private void Awake()
    {
        if (!FindObjectOfType<Client>())
        {
            Instantiate(NetworkClient);
        }

        if (!FindObjectOfType<AudioManager>())
        {
            Instantiate(AudioPrefab);
        }
    }
}
