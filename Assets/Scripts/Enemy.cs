using UnityEngine;
using System.Collections;
using System;


public class Enemy : Character
{
    public enum EnemyPersonality { aggressive, defensive, jerk };

    public int Score { get; set; }
    private GameObject target;
    public EnemyPersonality Personality { get; set; }
    private bool commandDecided;
    private bool executingCommand;
    System.Random random = new System.Random();

    public void InitializeEnemy(int maxHealth, int health, int attackPower, float knockback, CharacterState state, float speed, Animator animator, bool canMove, bool moving)
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

        Score = 100;
        target = GameObject.FindGameObjectWithTag("Player");
        Personality = EnemyPersonality.aggressive;

        commandDecided = false;
        executingCommand = false;
    }



    private void Awake()
    {
        InitializeEnemy(3, 3, 1, 1.0f, CharacterState.idle, 2.0f, GetComponent<Animator>(), true, false);
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (commandDecided == false) {
            DecideCommand();
        }
        
        if (!GetComponent<Rigidbody>().IsSleeping())
        {
            CharacterAnimator.CrossFade("Run", 0.0f);
            State = CharacterState.moving;
        }

        SetMovementAnimation();

    }



    public void DecideCommand() {
 
        int command = random.Next(1, 10);

        if (Personality == EnemyPersonality.aggressive)
        {

            if (command <= 6)
            {
                StartCoroutine(AttackCommand((float) random.Next(5, 10)));
            }
            else if (command <= 9)
            {
                StartCoroutine(TauntCommand((float)random.Next(3, 5)));
            }
            else
            {
                StartCoroutine(DefendCommand((float)random.Next(4, 8)));
            }

        }

        else if (Personality == EnemyPersonality.defensive)
        {

            if (command <= 4)
            {
                StartCoroutine(DefendCommand((float)random.Next(4, 8)));
            }
            else if (command <= 8)
            {
                StartCoroutine(AttackCommand((float)random.Next(5, 10)));
            }
            else
            {
                StartCoroutine(TauntCommand((float)random.Next(3, 7)));
            }

        }

        else if (Personality == EnemyPersonality.jerk)
        {

            if (command <= 4)
            {
                StartCoroutine(TauntCommand((float)random.Next(5, 10)));
            }
            else if (command <= 8)
            {
                StartCoroutine(AttackCommand((float)random.Next(3, 7)));
            }
            else
            {
                StartCoroutine(DefendCommand((float)random.Next(3, 5)));
            }

        }

        else {
            Debug.LogError("An enemy hasn't got a personality.");
        }

        commandDecided = true;

    }


    private void MoveTowardsTarget() {

        if (CanMove == true) {

            //move towards the target

        }
        
    }

    private void Taunt()
    {
        //CanMove = false;
        //move towards the target

    }



    protected override void Attack()
    {
        CharacterAnimator.CrossFade("Attack", 0.0f);
        State = CharacterState.attack;
        Debug.Log("Enemy attacked.");
    }

    protected override void Die()
    {
        CharacterAnimator.CrossFade("Death", 0.0f);
        State = CharacterState.dead;
        Debug.Log("Enemy died.");
        //Notify GameLogic, give points to the player
        StartCoroutine(Wait(2.0f));
        Destroy(gameObject);
    }


    //Move towards the selected target and try to hit him after you get close enough.
    private IEnumerator AttackCommand(float duration)
    {

        Debug.Log("An enemy decided to use attack command for" + duration + " seconds.");
        StartCoroutine(CommandDuration(duration));
        //select the target

        while (executingCommand == true) {
            MoveTowardsTarget();
            //hit him if you're close enough (call target's TakeDamage if there's a collision)
            yield return null;
        }

        

    }

    //Move away from the target.
    private IEnumerator DefendCommand(float duration)
    {

        Debug.Log("An enemy decided to use defend command for" + duration + " seconds.");
        StartCoroutine(CommandDuration(duration));

        while (executingCommand == true)
        {
            yield return null;
        }
    
    }

    //Taunt the target.
    private IEnumerator TauntCommand(float duration)
    {

        Debug.Log("An enemy decided to use taunt command for" + duration + " seconds.");
        StartCoroutine(CommandDuration(duration));

        while (executingCommand == true)
        {
            yield return null;
        }
   
    }

    //Determines the length of selected commands.
    private IEnumerator CommandDuration(float duration)
    {
        
        executingCommand = true;
        yield return new WaitForSeconds(duration);
        executingCommand = false;
        commandDecided = false;

    }


}
