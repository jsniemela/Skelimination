using UnityEngine;
using System.Collections;
using CnControls;
using UnityEngine.Networking;

public class Player : Character  {
    private GameObject attackTarget;

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
        GameObject imagetarget = GameObject.FindGameObjectWithTag("ImageTarget");
        transform.SetParent(imagetarget.transform, false);
    }

	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer)
        {
            return;
        }

		if (CanMove == true) 
		{

            //Lock rotation so that the character doesn't fall over
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

            Movement();
		}

        SetMovementAnimation();

        if (CnInputManager.GetButton("Attack")){

			CanMove = false;
			Moving = false;              
            Attack();
            
		}
	}

    private void Movement ()
    {
        //get horizontal and vertical input from controller
        float horizontalMovement = CnInputManager.GetAxis("Horizontal");
        float verticalMovement = CnInputManager.GetAxis("Vertical");

        //apply input to movement direction
        var movement = new Vector3(horizontalMovement, 0.0f, verticalMovement);

        //apply camera direction to movement
        movement = Camera.main.transform.TransformDirection(movement);
        movement.y = 0.0f;

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


	protected override void Attack() {
        CharacterAnimator.CrossFade("Attack", 0.0f);
        //Increase collider size during attack
        GetComponent<BoxCollider>().size = new Vector3(1.8f, 1.4f, 1.8f);
        State = CharacterState.attack;
        //Debug.Log("Player attacked.");
    }

    private void OnCollisionStay(Collision collision)
    {
        if(collision.transform.tag == "Skeleton")
        {
            if(State == CharacterState.attack) {
                Debug.Log("Attack collided with a skeleton");
                attackTarget = collision.gameObject;
                StartCoroutine(attackDelay(0.3f));
            }
        }
    }

    protected IEnumerator attackDelay(float waitDuration)
    {
        yield return new WaitForSeconds(waitDuration);
        Debug.Log("Knocked back");
        attackTarget.transform.forward = -this.transform.forward;
        attackTarget.GetComponent<Enemy>().TakeDamage(0, Knockback);
        //Revert collider size back to normal after attack
        GetComponent<BoxCollider>().size = new Vector3(1.4f, 1.4f, 1f);
        //attackTarget.GetComponent<Animator>().CrossFade("Knockback", 0.0f);
        //yield return new WaitForSeconds(10f);
        //attackTarget.GetComponent<Animator>().CrossFade("Idle", 0f);
        //attackTarget.transform.position = attackTarget.GetComponentInChildren<SphereCollider>().transform.position;
        //attackTarget.GetComponent<Rigidbody>().velocity = Vector3.zero;
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
