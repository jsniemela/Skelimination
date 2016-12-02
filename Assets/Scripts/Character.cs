using UnityEngine;
using System.Collections;


abstract public class Character : MonoBehaviour {
	int health;
	int attack;
	int knockback;
	protected float speed;

    public int Health
    {
        get
        {
            return health;
        }

        set
        {
            health = value;
        }
    }

    public int Attack
    {
        get
        {
            return attack;
        }

        set
        {
            attack = value;
        }
    }

    public int Knockback
    {
        get
        {
            return knockback;
        }

        set
        {
            knockback = value;
        }
    }

    public float Speed
    {
        get
        {
            return speed;
        }

        set
        {
            speed = value;
        }
    }

    enum status { frozen };

	public Character() {

        }
}
