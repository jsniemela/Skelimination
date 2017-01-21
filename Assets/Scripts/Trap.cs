﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Skeleton")
        {
            Debug.Log("Trap damaged skeleton.");
            other.gameObject.GetComponent<Enemy>().TakeDamage(0, 0, gameObject);
        }
    }
}
