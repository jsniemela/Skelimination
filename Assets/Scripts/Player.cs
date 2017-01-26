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
    }
    


    private void Awake()
    {
        InitializePlayer(10, 10, 1, 14.0f, CharacterState.idle, 0.2f, GetComponent<Animator>(), true, false, GameObject.FindGameObjectWithTag("ImageTarget"), 0, 0);
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

            if (knockback > 0)
            {
                //Move the character according to the knockback.
            }

            //Call die if the characte's health goes to zero.
            if (Health < 1)
            {
                Die(null);
            }
            else
            {
                CharacterAnimator.CrossFade("Knockback", 0.0f);
                State = CharacterState.knockback;
                StartCoroutine(StopCharacter(2.0f));
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

    /*
    private void OnCollisionStay(Collision collision)
    {

        if(State == CharacterState.attack && (collision.gameObject.tag == "Skeleton" || collision.gameObject.tag == "Player"))
        {



            if (!collisionGameObjects.Contains(collision.gameObject)) {
                collisionGameObjects.Add(collision.gameObject);
                //collisionGameObjects.GetHashCode();
            }
         
            if (collisionGameObjects.Count > 0) {
                StartCoroutine(attackDelay(0.3f));
            }
                
        }
    }
    */


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Skeleton" || collision.gameObject.tag == "Player"){

            if (!collisionGameObjects.Contains(collision.gameObject))
            {
                collisionGameObjects.Add(collision.gameObject);
                //Debug.LogError("Gameobject entered collision.");
                Debug.LogError("List size is now " + collisionGameObjects.Count +".");
            }

        }
        
        
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Skeleton" || collision.gameObject.tag == "Player")
        {

            if (collisionGameObjects.Contains(collision.gameObject))
            {
                collisionGameObjects.Remove(collision.gameObject);
                //Debug.LogError("Gameobject exited collision.");
                Debug.LogError("List size is now " + collisionGameObjects.Count + ".");
            }

        }
    }

    /*
    private void OnCollisionStay(Collision collision)
    {

        if (collision.gameObject.tag == "Skeleton" || collision.gameObject.tag == "Player")
        {

            if (!collisionGameObjects.Contains(collision.gameObject))
            {
                collisionGameObjects.Add(collision.gameObject);
                Debug.LogError("List size is now " + collisionGameObjects.Count + ".");
            }

        }
    }
    */

    protected IEnumerator attackDelay(float waitDuration)
    {

        //gameObject.GetComponent<Rigidbody>().isKinematic = true;
        yield return new WaitForSeconds(waitDuration);

        //Debug.LogError("List size is " +collisionGameObjects.Count +".");

        //If there is at least one gameobject in the collisionGameObjects, iterate them, call
        //their TakeDamage and move them accordingly. 
        if (collisionGameObjects.Count > 0) {

            foreach (GameObject g in collisionGameObjects)
            {
                try
                {
                    if (g.gameObject.tag == "Skeleton")
                    {           
                        g.GetComponent<Enemy>().TakeDamage(AttackPower, Knockback, gameObject);
                        g.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        g.GetComponent<Rigidbody>().velocity = transform.forward * Knockback;
                        //Debug.LogError("Attack collided with GameObject " +i + ".");
                    }
                    else if (g.gameObject.tag == "Player")
                    {
                        g.GetComponent<Player>().TakeDamage(AttackPower, Knockback, gameObject);
                        g.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        g.GetComponent<Rigidbody>().velocity = transform.forward * Knockback;
                        //Debug.LogError("Attack collided with GameObject " + i + ".");
                    }

                }
                catch (MissingReferenceException e)
                {
                    //Debug.LogError("");
                }
            }

        }

        yield return new WaitForSeconds(1f);


        //gameObject.GetComponent<Rigidbody>().isKinematic = false;
        //attackTarget.GetComponent<Rigidbody>().velocity = Vector3.zero;
        //attackTarget = null;

        //collisionGameObjects.Clear();

        yield return null;
    }

    protected override void Die(GameObject killer)
    {

        if (State != CharacterState.dead) {

            CharacterAnimator.CrossFade("Death", 0.0f);
            State = CharacterState.dead;
            Debug.Log("Player died.");
            StartCoroutine(WaitAndDisable(5.0f, gameObject));
            //Notify GameLogic
            //StartCoroutine(Wait(2.0f));

        }

    }

}
