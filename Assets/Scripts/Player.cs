using UnityEngine;
using System.Collections;
using CnControls;

public class Player : MonoBehaviour  {
	bool moving = false;
	bool canMove = true;
    float speed = 2.0f;

	void Start () {
	
	}

	// Update is called once per frame
	void Update () {
		if (canMove == true) 
		{
            //Lock rotation so that the character doesn't fall over
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

            Movement();
            

		}

		if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Attack") && moving == false)
		{
			GetComponent<Animator> ().CrossFade ("Idle", 0.0f);
			canMove = true;
		}

		if (CnInputManager.GetButton("Attack")){
			if (true) { 
				canMove = false;
				moving = false;
				GetComponent<Animator> ().CrossFade ("Attack", 0);
				//GetComponent<Rigidbody>().velocity = new Vector3(0 , 0, 5.0f);
			}
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
        movement *= speed / 10;
        //change moving state to true when moving
        moving = horizontalMovement != 0 || verticalMovement != 0;

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
        if (moving)
        {
            //move character
            GetComponent<Rigidbody>().MovePosition(characterMovement);
            //fade into running animation
            GetComponent<Animator>().CrossFade("Run", 0.0f);
        }
    }


	private void Attack() {

	}

	private int calculateDamage() {
		int damage = 1;
		return damage;
	}
}
