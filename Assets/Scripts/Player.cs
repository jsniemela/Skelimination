using UnityEngine;
using System.Collections;

public class Player : Character  {

 
    public void InitializePlayer(int maxHealth, int health, int attackPower, float knockback, CharacterState state, float speed, Animator animator, bool canMove, bool moving)
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
    }
    


    private void Awake()
    {
        InitializePlayer(10, 10, 1, 1.0f, CharacterState.idle, 0.2f, GetComponent<Animator>(), true, false);
    }

	void Start () {
        

    }

	// Update is called once per frame
	void Update () {
		if (CanMove == true) 
		{
			var horizontalMovement = Input.GetAxis ("Horizontal");
			var verticalMovement = Input.GetAxis ("Vertical");
			var movement = new Vector3 (horizontalMovement, 0.0f, verticalMovement);
			movement *= Speed;
			Moving = horizontalMovement != 0 || verticalMovement != 0;

			if (movement != Vector3.zero) {
				Quaternion targetRotation = Quaternion.LookRotation (movement, Vector3.up);
				float rotationSpeed = 60.0f;
				Quaternion newRotation = Quaternion.Lerp (GetComponent<Rigidbody> ().rotation, targetRotation, rotationSpeed * Time.deltaTime);
				GetComponent<Rigidbody> ().MoveRotation (newRotation);
			}

			var characterMovement = transform.position + movement;

            

            if (Moving) {
				GetComponent<Rigidbody> ().MovePosition (characterMovement);
                CharacterAnimator.CrossFade("Run", 0.0f);
                State = CharacterState.moving;
            }

		}

        SetMovementAnimation();

        if (Input.GetButton("Attack")){

			CanMove = false;
			Moving = false;              
            Attack();
            
		}
	}


	protected override void Attack() {
        CharacterAnimator.CrossFade("Attack", 0.0f);
        State = CharacterState.attack;
        //Debug.Log("Player attacked.");
    }

    protected override void Die()
    {
        CharacterAnimator.CrossFade("Death", 0.0f);
        State = CharacterState.dead;
        Debug.Log("Player died.");
        //Notify GameLogic
        StartCoroutine(Wait(2.0f));
        //Game over
    }


}
