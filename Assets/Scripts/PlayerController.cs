using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Boundary
{
    public float xMin, xMax, zMin, zMax;
}

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float tilt;
    public float freezeTilt;
    public Boundary boundary;

    public GameObject shot;
    public GameObject blastShot;
    public Transform shotSpawn;
    public float fireRate;

    private Rigidbody rb;
    private AudioSource audioSource;
    private float nextFire;
    private Vector3 desiredVelocity;
    private bool isDesiredVelocitySet = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetButton("Fire1") && Time.time > nextFire)
        {
            FireBolt();
        }
        else if (Input.GetButton("Fire2") && Time.time > nextFire)
        {
            FireBlast();
        }
    }

    public void FireBlast()
    {
        nextFire = Time.time + fireRate;
        Instantiate(blastShot, shotSpawn.position, shotSpawn.rotation);
        audioSource.Play();
    }

    public void FireBolt()
    {
        nextFire = Time.time + fireRate;
        Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
        audioSource.Play();
    }

    public void UnsetDesiredVelocity()
    {
        isDesiredVelocitySet = false;
    }

    public void SetDesiredVelocity(Vector3 pos)
    {
        const int velocityLimit = 10;
        Vector3 maxVec = new Vector3(-velocityLimit, -velocityLimit, -velocityLimit);
        Vector3 minVec = new Vector3(velocityLimit, velocityLimit, velocityLimit);

        desiredVelocity = Vector3.Max(maxVec, Vector3.Min(minVec, pos));
        isDesiredVelocitySet = true;
    }

    private float freezeStartTime = -100;
    public float freezeDuration = 0.5f;

    public void Freeze()
    {
        freezeStartTime = Time.time;
    }

    private void FixedUpdate()
    {
        bool isFreezing = Time.time - freezeStartTime < freezeDuration;
        if (!isFreezing)
        {
            Vector3 movement = GetMovementVector();

            rb.velocity = movement * speed;
        }

        rb.position = new Vector3
        (
            Mathf.Clamp(rb.position.x, boundary.xMin, boundary.xMax),
            0.0f,
            Mathf.Clamp(rb.position.z, boundary.zMin, boundary.zMax)
        );
        rb.rotation = Quaternion.Euler(0.0f, 0.0f, -rb.velocity.x * (isFreezing ? freezeTilt : tilt));
    }

    private Vector3 GetMovementVector()
    {
        if (isDesiredVelocitySet)
        {
            return desiredVelocity;
        }
        else
        {
            float moveHorizontal = Input.GetAxis("Horizontal");
            float moveVertical = Input.GetAxis("Vertical");
            return new Vector3(moveHorizontal, 0.0f, moveVertical);
        }
    }

}
