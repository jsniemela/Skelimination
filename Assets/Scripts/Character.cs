using UnityEngine;
using System.Collections;

public enum CharacterState { idle, moving, knockback, taunt, frozen, dead, attack };

abstract public class Character : MonoBehaviour
{

    private int maxHealth;
    private int health;
    private int attackPower;
    private float knockback;
    private CharacterState state;
    private float speed;
    protected Animator animator;
    public bool canMove;



    public Character(int maxHealth, int health, int attackPower, int knockback, CharacterState state, int speed, Animator animator, bool canMove)
    {
        MaxHealth = maxHealth;
        Health = health;
        AttackPower = attackPower;
        Knockback = knockback;
        State = state;
        Speed = speed;
        this.animator = animator;
        this.canMove = canMove;
    }



    public int MaxHealth
    {
        get
        {
            return maxHealth;
        }

        set
        {
            maxHealth = value;
        }
    }

    public int Health
    {
        get
        {
            return health;
        }

        set
        {
            if (value < 0)
            {
                health = 0;
            }
            else if (value > maxHealth)
            {
                health = maxHealth;
            }
            else
            {
                health = value;
            }
        }
    }

    public int AttackPower
    {
        get
        {
            return attackPower;
        }

        set
        {
            attackPower = value;
        }
    }

    public float Knockback
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

    public CharacterState State
    {
        get
        {
            return state;
        }

        set
        {
            //State can't be changed after the character dies.
            if (State != CharacterState.dead)
            {

                state = value;

            }

        }
    }



    public void TakeDamage(int damage, float knockback)
    {

        Health = Health - damage;
        
        if (Health > 0)
        {
            animator.SetTrigger("Knockback");
            State = CharacterState.knockback;
            //move character a certain amount
        }
        else {
            //call Die()
        }

    }

    public void OutOfBounds()
    {
        
        //if gameObject is out of bounds, call Die()

    }

    /*
    public void CalculateDamage() {}
    */

    protected virtual void Attack() { }

    protected virtual void Die() {
        //animator.SetTrigger("Death");
        //State = CharacterState.dead;
    }

    

    IEnumerator StopCharacter(float waitDuration)
    {
        canMove = false;
        yield return new WaitForSeconds(waitDuration);
        canMove = true;
    }

}
