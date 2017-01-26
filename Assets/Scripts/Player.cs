using UnityEngine;
using System.Collections;
using CnControls;
using UnityEngine.Networking;
using System.Collections.Generic;
using System;

public class Player : Character  {

    private GameObject attackTarget;
    Vector3 movement;
    public int Score { get; set; }
    public int Kills { get; set; }

    AudioSource audioSource;
    AudioClip playerSpawn;
    AudioClip apologies;

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
        //GameObject imagetarget = GameObject.FindGameObjectWithTag("ImageTarget");
        transform.SetParent(imageTarget.transform, false);
    }

	// Update is called once per frame
	void Update () {

        KillTheCharacterIfOutOfBounds();

    }

    private void FixedUpdate()
    {

        if (!isLocalPlayer)
        {
            return;
        }

        if (State == CharacterState.dead) {
            return;
        }

        //Lock rotation so that the character doesn't fall over
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        if (CanMove == true)
        {
            Movement();
        }


        SetMovementAnimation();

        if (CnInputManager.GetButton("Attack"))
        {
                 
            Attack();
            
        }

    }

    private void Movement ()
    {
        //get horizontal and vertical input from controller
        float horizontalMovement = CnInputManager.GetAxis("Horizontal");
        float verticalMovement = CnInputManager.GetAxis("Vertical");

        //apply input to movement direction
        Vector3 movement = new Vector3(horizontalMovement, 0.0f, verticalMovement);
        movement = new Vector3(horizontalMovement, 0.0f, verticalMovement);

        //apply camera direction to movement
        movement = Camera.main.transform.TransformDirection(movement);
        movement.y = 0.0f;
        movement.Normalize();

        //apply speed to movement
        movement *= Speed;

        //change moving state to true when moving
        Moving = horizontalMovement != 0 || verticalMovement != 0;

        if (movement != Vector3.zero)
        {
            //var targetDirection = new Vector3 (Input.GetAxis ("Horizontal"), 0.0f, Input.GetAxis ("Vertical"));
            //targetDirection = Camera.main.transform.TransformDirection (targetDirection);
            //targetDirection.y = 0.0f;
            Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);
            float rotationSpeed = 60.0f;
            Quaternion newRotation = Quaternion.Lerp(GetComponent<Rigidbody>().rotation, targetRotation, rotationSpeed * Time.deltaTime);
            GetComponent<Rigidbody>().MoveRotation(newRotation);
        }
        var characterMovement = transform.position + movement;
        if (Moving)
        {
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

            Health = Health - damage;
            //Debug.LogError("Damage dealt to player. His current HP is " + Health+".");

            if (knockback > 0)
            {
                
            }

            //Call die if the characte's health goes to zero.
            if (Health < 1)
            {
                Die(null);
            }
            else
            {
                CharacterAnimator.CrossFade("Knockback", 0.0f);              
                StartCoroutine(StopCharacter(5.0f));
                State = CharacterState.knockback;
            }
            
        }

    }

    protected override void Attack() {

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
                collisionGameObjects.Add(collision.gameObject);
                //Debug.LogError("Gameobject entered collision.");
                //Debug.LogError("List size is now " + collisionGameObjects.Count +".");
            }

        }


    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag == "Skeleton" || collision.gameObject.tag == "Player")
        {

            if (collisionGameObjects.Contains(collision.gameObject))
            {
                collisionGameObjects.Remove(collision.gameObject);
                //Debug.LogError("Gameobject exited collision.");
                //Debug.LogError("List size is now " + collisionGameObjects.Count + ".");
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
