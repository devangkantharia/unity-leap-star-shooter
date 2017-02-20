using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyByContact : MonoBehaviour
{
    public GameObject explosion;
    public int scoreValue;
    public int damage;
    private GameController gameController;

    private void Start()
    {
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
        if (other.tag == "Boundary")
        {
            return;
        }
        if (other.tag == "Blast")
        {
            Destroy(other.gameObject);
            return;
        }
        Instantiate(explosion, transform.position, transform.rotation);
        if (other.tag == "Player")
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            var rb = player.GetComponent<Rigidbody>();
            rb.AddExplosionForce(15f, transform.position, 3, 0, ForceMode.Impulse);

            gameController.ShakeCamera();

            player.GetComponent<PlayerController>().Freeze();

            Destroy(gameObject);
            gameController.DecreaseHealth(damage);

            return;
        }
        gameController.AddScore(scoreValue);
        Destroy(other.gameObject);
        Destroy(gameObject);
    }
}
