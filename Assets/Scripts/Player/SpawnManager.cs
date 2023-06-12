using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public Transform[] SpawnPoint;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        foreach(Transform spaw in SpawnPoint)
        {
            spaw.gameObject.SetActive(false);
        }
    }

    public Transform GetSpawnPoint()
    {
        return SpawnPoint[Random.Range(0, SpawnPoint.Length)];
    }
}
