using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Skeleton")
        {
            Debug.Log("Trap damaged skeleton.");
            other.gameObject.GetComponent<Enemy>().TakeDamage(0, 0, gameObject);
        }
        if (other.gameObject.tag == "Player")
        {
            //disabled until there's a better damage animation for the player
            //other.gameObject.GetComponent<Player>().TakeDamage(0, 0, gameObject);
        }
    }
}
