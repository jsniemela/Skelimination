using UnityEngine;
using System.Collections;

public class Player : Character  {
	bool moving = false;
	bool canMove = true;

    public Player() {
        speed = 2.0f;
    }

	void Start () {
	
	}

	// Update is called once per frame
	void Update () {
        if (canMove == true) 
		{
			var horizontalMovement = Input.GetAxis ("Horizontal");
			var verticalMovement = Input.GetAxis ("Vertical");
			var movement = new Vector3 (horizontalMovement, 0.0f, verticalMovement);
			movement *= speed/10;
			moving = horizontalMovement != 0 || verticalMovement != 0;

			//Vector3 relative = transform.rotation.eulerAngles;
			//int relative.y = Camera.main.transform.localEulerAngles; 

			if (movement != Vector3.zero) {
				//var targetDirection = new Vector3 (Input.GetAxis ("Horizontal"), 0.0f, Input.GetAxis ("Vertical"));
				//targetDirection = Camera.main.transform.TransformDirection (targetDirection);
				//targetDirection.y = 0.0f;
				Quaternion targetRotation = Quaternion.LookRotation (movement, Vector3.up);
				float rotationSpeed = 60.0f;
				Quaternion newRotation = Quaternion.Lerp (GetComponent<Rigidbody> ().rotation, targetRotation, rotationSpeed * Time.deltaTime);
				GetComponent<Rigidbody> ().MoveRotation (newRotation);
			}
			var characterMovement = transform.position + movement;
			if (moving) {
				GetComponent<Rigidbody> ().MovePosition (characterMovement);
				GetComponent<Animator> ().CrossFade ("Run", 0.0f);
			}

		}

		if (!GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Attack") && moving == false)
		{
			GetComponent<Animator> ().CrossFade ("Idle", 0.0f);
			canMove = true;
		}

		if (Input.GetButton("Attack")){
				canMove = false;
				moving = false;
				GetComponent<Animator> ().CrossFade ("Attack", 0);
				//GetComponent<Rigidbody>().velocity = new Vector3(0 , 0, 5.0f);
		}
	}


	private void Attack() {

	}

	private int calculateDamage() {
		int damage = 1;
		return damage;
	}
}
