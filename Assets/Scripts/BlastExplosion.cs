using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastExplosion : MonoBehaviour {

    public LayerMask m_AsteroidMask;
    public float m_ExplosionForce = 1000f;
    public float m_ExplosionRadius = 5f;
    public float m_MaxLifeTime = 2f;

    private GameController gameController;

    private void Start()
    {
        Destroy(gameObject, m_MaxLifeTime);
  
        var gameControllerObject = GameObject.FindWithTag("GameController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject.GetComponent<GameController>();
        }
        if (gameController == null)
        {
            Debug.Log("Connot find 'GameController' script.");
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        // Find all the tanks in an area around the shell and damage them.
        if (other.tag == "Boundary")
        {
            return;
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_AsteroidMask);

        foreach (var collider in colliders)
        {
            var targetRigidbody = collider.GetComponent<Rigidbody>();

            if (!targetRigidbody)
                continue;

            DestroyByContact destroyByContactScript = collider.GetComponent<DestroyByContact>();
            int scoreValue = destroyByContactScript.scoreValue;
            gameController.AddScore(scoreValue);
            Instantiate(destroyByContactScript.explosion, collider.transform.position, collider.transform.rotation);
            Destroy(collider.gameObject);
        }

        gameController.ShakeCamera();

        Destroy(gameObject);
    }


}
