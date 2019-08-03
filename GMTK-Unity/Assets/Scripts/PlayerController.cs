﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    //Variables
    public float WalkSpeed;
    public float JumpSpeed;
    public float MaxSpeed;
    public bool JumpResetsVelocity = false;
    public GameObject BulletPrefab;
    public GameObject BulletContainer;
    public float FireRate;
    float fireRateCooldownTimer;

    List<GameObject> collidedPlatforms;
    List<GameObject> droppedPlatforms;

    bool hasJump = true;
    Rigidbody2D rigidbody;


    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        collidedPlatforms = new List<GameObject>();
        droppedPlatforms = new List<GameObject>();
    }

    void Update()
    {
        fireRateCooldownTimer += Time.deltaTime;

        Vector2 inputVelocity = new Vector2();
        //Jumping
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && hasJump)
        {
            hasJump = false;
            inputVelocity.y += JumpSpeed;

            if (JumpResetsVelocity && rigidbody.velocity.y < 0)
            {
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0);
            }
        }
        
        if (Input.GetKey(KeyCode.S))
        {
            foreach (var gameObject in collidedPlatforms)
            {
                gameObject.GetComponent<EdgeCollider2D>().enabled = false;
                droppedPlatforms.Add(gameObject);
            }
        }
        else
        {
            foreach (var gameObject in droppedPlatforms)
            {
                gameObject.GetComponent<EdgeCollider2D>().enabled = true;
            }
            droppedPlatforms.Clear();
        }

        //Left Right Movement
        if (Input.GetKey(KeyCode.A))
        {
            inputVelocity.x -= WalkSpeed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputVelocity.x += WalkSpeed;
        }

        rigidbody.velocity += inputVelocity;

        // Cap our horizontal momentum to avoid going faster and faster the longer we hold down A and D
        if (rigidbody.velocity.x > MaxSpeed)
        {
            rigidbody.velocity = new Vector2(MaxSpeed, rigidbody.velocity.y);
        }
        if (rigidbody.velocity.x < -MaxSpeed)
        {
            rigidbody.velocity = new Vector2(-MaxSpeed, rigidbody.velocity.y);
        }

        // Flip the sprite depending on our velocity.
        if (rigidbody.velocity.x < 0)
        {
            this.GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            this.GetComponent<SpriteRenderer>().flipX = true;
        }

        // Logic for spawning bullets
        if (Input.GetMouseButton(0))
        {
            if (fireRateCooldownTimer >= FireRate)
            {
                fireRateCooldownTimer = 0;

                GameObject instancedBullet = Instantiate(BulletPrefab, BulletContainer.transform, false);
                instancedBullet.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, instancedBullet.transform.position.z);

                // We use this to make sure bullets don't damage us
                // see Bullet.CS for more info
                instancedBullet.AddComponent<Team>();
                instancedBullet.GetComponent<Team>().TeamID = TeamIDs.Player;

                //Rotates bullet to mouse
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                instancedBullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, mousePos - instancedBullet.transform.position);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If we collide with something that has the GivesJump component then allow ourselves to jump again
        // I am currently using these on the floor and the platforms to detect whether we have landed after a jump
        if (collision.gameObject.GetComponent<GivesJump>() != null)
        {
            // If the wall we platform we hit when raycasting downwards
            if (rigidbody.velocity.y <= 0 + float.Epsilon)
            {
                hasJump = true;
            }
        }

        // Add platforms we are in contact with to list
        if (collision.gameObject.layer == 10)
        {
            collision.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            collidedPlatforms.Add(collision.gameObject);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 10)
        {
            collision.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
}
