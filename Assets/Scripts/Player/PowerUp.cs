using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum Type
    {
        Health,
        Reload,
        Granade
    }

    [SerializeField] Type _powerUpType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().DropManager(_powerUpType);
            Destroy(gameObject);
        }
    }
}
