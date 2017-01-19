﻿using UnityEngine;
using System.Collections;

public enum CharacterState { idle, moving, knockback, taunt, frozen, dead, attack };

abstract public class Character : MonoBehaviour
{

    public int maxHealth;
    private int health;
    private int attackPower; 
    private float knockback;
    private CharacterState state;
    private float speed;
    public Animator CharacterAnimator { get; set; }
    public bool CanMove { get; set; }
    public bool Moving { get; set; }





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
            if (value <= 0)
            {
                health = 0;
                Debug.Log("Health set to 0.");
                Die();
            }
            else if (value > MaxHealth)
            {
                health = MaxHealth;
                Debug.Log("New health value is greater than maxHealth. Health set to maxHealth.");
            }
            else
            {
                health = value;
                Debug.Log("Health changed to "+value);
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
            if (State != CharacterState.dead && value != state)
            {
                state = value;
                Debug.Log("Character's state changed to " + state.ToString());
            }

        }
    }




    public void TakeDamage(int damage, float knockback)
    {

        if (State != CharacterState.dead && State != CharacterState.knockback) {

            Health = Health - damage;
            CharacterAnimator.CrossFade("Knockback", 0.0f);
            State = CharacterState.knockback;

            if (knockback > 0)
            {
                //Move the character according to the knockback.
            }

            StartCoroutine(StopCharacter(2.0f));

        }

    }

    //The character 
    public void OutOfBounds()
    {
        //Out of bounds notification to GameManager.
        Die();
    }

    //Change character's state and animation to idle if he isn't moving and animations "Attack", 
    //"Death", "Taunt" or knockback aren't playing. Also allows the character to move again by setting CanMove to true.
    //All the animator's animations except "Death" lead to "Idle" automatically.        
    public void SetMovementAnimation()
    {

        if (Moving == false
            && !CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack")        
            && !CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Death")
            && !CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Taunt")
            && !CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Knockback"))
            {
                State = CharacterState.idle;
                CharacterAnimator.CrossFade("Idle", 0.0f);
                CanMove = true;
            }
    }
    

    protected virtual void Attack() { }

    protected virtual void Die() { }

    

    protected IEnumerator StopCharacter(float waitDuration)
    {
        CanMove = false;
        yield return new WaitForSeconds(waitDuration);
        CanMove = true;
    }

    protected IEnumerator Wait(float waitDuration)
    {
        yield return new WaitForSeconds(waitDuration);
    }

}
