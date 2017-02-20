using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyByBoundary : MonoBehaviour
{
    private GameController gameControllerScript;

    private void Start()
    {
        gameControllerScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

    }
    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            return;
        }

        if (other.gameObject.tag == "Asteroid")
        {
            gameControllerScript.DecreaseHealth(1);
            gameControllerScript.ShakeCamera();
        }
        Destroy(other.gameObject);
    }
}
