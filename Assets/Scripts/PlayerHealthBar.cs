using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour {

    Image healthbar;
    Player player;

    // Use this for initialization
    void Start ()
    {
        healthbar = GetComponent<Image>();
//        var player = GameObject.Find("Player");
    }
	
	// Update is called once per frame
	void Update ()
    {
        healthbar.fillAmount = (float)player.Health / (float)player.MaxHealth;
	}
}
