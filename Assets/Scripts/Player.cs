using UnityEngine;
using System.Collections;
using CnControls;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public class Player : Character  {
    [SyncVar]
    private GameObject attackTarget;
    [SyncVar]
    Vector3 movement;
    [SyncVar]
    Quaternion targetRotation;
    [SyncVar]
    Quaternion newRotation;
    [SyncVar]
    bool attacking;

    public int Score { get; set; }
    public int Kills { get; set; }

    AudioSource audioSource;
    AudioClip playerSpawn;
    AudioClip apologies;

    [SyncVar]
    float horizontalMovement;
    [SyncVar]
    float verticalMovement;

    public void InitializePlayer(int maxHealth, int health, int attackPower, float knockback, CharacterState state, 
        float speed, Animator animator, bool canMove, bool moving, GameObject ground, int score, int kills)
    { 
        MaxHealth = maxHealth;
        Health = health;
        AttackPower = attackPower;
        Knockback = knockback;
        State = state;
        Speed = speed;
        CharacterAnimator = animator;
        CanMove = canMove;
        Moving = moving;
        imageTarget = ground;
        Score = score;
        Kills = kills;
        collisionGameObjects = new List<GameObject>();

        audioSource = GetComponent<AudioSource>();

        try
        {
            //Initialize own audio sources.
            playerSpawn = (AudioClip)Resources.Load("Sounds/player_spawns");
            apologies = (AudioClip)Resources.Load("Sounds/Voices/friendlyfire1");

            //Set the superclass' audio settings.
            characterAudioSource = audioSource;
            destroyGameObject = (AudioClip)Resources.Load("Sounds/destroy_gameobject");
            swordAttack1 = (AudioClip)Resources.Load("Sounds/enemy_sword_attack");
            swordAttack2 = (AudioClip)Resources.Load("Sounds/player_sword_attack");
            swordSlash = (AudioClip)Resources.Load("Sounds/sword_slash");
        }
        catch (Exception e) { }

    }
    


    private void Awake()
    {
        InitializePlayer(10, 10, 1, 14.0f, CharacterState.idle, 0.2f, GetComponent<Animator>(), true, false, GameObject.FindGameObjectWithTag("ImageTarget"), 0, 0);
        audioSource.PlayOneShot(playerSpawn);
    }


	void Start () {
        transform.SetParent(imageTarget.transform, false);
    }

	// Update is called once per frame
	void Update () {
        KillTheCharacterIfOutOfBounds();
        SetMovementAnimation();
        if (State == CharacterState.dead)
        {
            return;
        }

        //Lock rotation so that the character doesn't fall over
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        if (!isLocalPlayer)
        {
            return;
        }

        

        if (CanMove == true)
        {
            //get horizontal and vertical input from controller
            horizontalMovement = CnInputManager.GetAxis("Horizontal");
            verticalMovement = CnInputManager.GetAxis("Vertical");
            CmdMove(horizontalMovement, verticalMovement);
        }


        

        if (CnInputManager.GetButton("Attack"))
        {
            //attacking variable isn't really used for anything, but attack doesn't seem to synchronize unless they have at least one parameter. 
            attacking = true;
            CmdAttack(attacking);
        }
    }

    [Command]
    void CmdMove(float horizontalMovement, float verticalMovement)
    {
        //passes the movement command to client as a remote procedure call
        RpcMove(horizontalMovement, verticalMovement);
    }
    

    [ClientRpc]
    private void RpcMove (float horizontalMovement, float verticalMovement)
    {
        //apply input to movement direction
        movement = new Vector3(horizontalMovement, 0.0f, verticalMovement);

        //apply camera direction to movement
        movement = Camera.main.transform.TransformDirection(movement);
        movement.y = 0.0f;
        movement.Normalize();

        //apply speed to movement
        movement *= Speed;

        //change moving state to true when moving
        Moving = horizontalMovement != 0 || verticalMovement != 0;

        if (Moving == true)
        {
            var characterMovement = transform.position + movement;
            targetRotation = Quaternion.LookRotation(movement, Vector3.up);
            float rotationSpeed = 60.0f;
            newRotation = Quaternion.Lerp(GetComponent<Rigidbody>().rotation, targetRotation, rotationSpeed * Time.deltaTime);
            //rotate character
            GetComponent<Rigidbody>().MoveRotation(newRotation);

            //move character
            GetComponent<Rigidbody>().MovePosition(characterMovement);
            //fade into running animation
            CharacterAnimator.CrossFade("Run", 0.0f);
            //change character state to moving
            State = CharacterState.moving;
        }
    }

    public override void TakeDamage(int damage, float knockback, GameObject attacker)
    {
        if (State != CharacterState.dead && State != CharacterState.knockback)
        {
            //Debug.Log("Damage dealt to player. His current HP is " + Health+".");
            Health = Health - damage;

            //Call die if the characte's health goes to zero.
            if (Health < 1)
            {
                Die(null);
            }
            else
            {
                //Animation taken from skeleton so it looks wrong on the knight. 
                CharacterAnimator.CrossFade("Knockback", 0.0f);              
                StartCoroutine(StopCharacter(5.0f));
                State = CharacterState.knockback;
            }
        }
    }

    [Command]
    void CmdAttack(bool attacking)
    {
        RpcAttack(attacking);
    }

    [ClientRpc]
    protected void RpcAttack(bool attacking) {
        if (!CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack") && State != CharacterState.attack && State != CharacterState.knockback && State != CharacterState.dead)
        {
            CanMove = false;
            Moving = false;           
            State = CharacterState.attack;
            StartCoroutine(attackDelay(0.2f));
            CharacterAnimator.CrossFade("Attack", 0.0f);
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Skeleton" || collision.gameObject.tag == "Player")
        {
            if (!collisionGameObjects.Contains(collision.gameObject))
            {
                //Collision with a skeleton or player is added to a list for the knockback.
                collisionGameObjects.Add(collision.gameObject);
            }
        }
    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag == "Skeleton" || collision.gameObject.tag == "Player")
        {

            if (collisionGameObjects.Contains(collision.gameObject))
            {
                //Player or skeleton collision is removed from the list after they leave the collision zone.
                collisionGameObjects.Remove(collision.gameObject);
            }

        }
    }

    protected override void Die(GameObject killer)
    {

        if (State != CharacterState.dead) {
            audioSource.PlayOneShot(apologies);
            CharacterAnimator.CrossFade("Death", 0.0f);
            State = CharacterState.dead;
            CanMove = false;
            Moving = false;
            Debug.Log("Player died.");
            StartCoroutine(WaitAndDisable(5.0f, gameObject));
            //Notify GameLogic

        }

    }

}
