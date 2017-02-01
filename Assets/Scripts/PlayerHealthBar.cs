using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour {

    Image healthbar;
    GameObject player;

    // Use this for initialization
    void Start ()
    {
        healthbar = GetComponent<Image>();
        //        var player = GameObject.Find("Player");
        
    }
	
	// Update is called once per frame
	void Update ()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        healthbar.fillAmount = (float)player.GetComponent<Player>().Health / (float)player.GetComponent<Player>().MaxHealth;
	}
}
