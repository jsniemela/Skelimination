using UnityEngine;
using System.Collections;
using System;


public class Enemy : Character
{
    public enum EnemyPersonality { aggressive, defensive, jerk };

    //personality determines the odds of randomized command actions.
    private EnemyPersonality personality;

    //target is the enemy's attacking target (a GameObject with a tag "Player")
    private GameObject target;

    //score determines the amount of points that the player gets from defeating this enemy.
    public int Score { get; set; }

    //random is used for calculating random numbers that are used in AI randomizations and target selection.
    System.Random random;

    //The enemy selects a new command based on his personality when commandDecided is false.
    private bool commandDecided;

    //executingCommand is true when the enemy is currently executing a command.
    private bool executingCommand;

    

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
        random = new System.Random();

        SelectTargetRandomly();

        Personality = EnemyPersonality.aggressive;

        commandDecided = false;
        executingCommand = false;
        
    }

    //Selects the attack target randomly from the GameObjects tagged with "Player". Sets the target to null if there aren't
    //any player objects in the scene.
    private void SelectTargetRandomly() {

        if (GameObject.FindGameObjectsWithTag("Player").Length > 0)
        {
            GameObject[] targetArray = GameObject.FindGameObjectsWithTag("Player");
            int randomIndex = random.Next(0, targetArray.Length - 1);
            target = targetArray[randomIndex];
            Debug.Log("An enemy selected the player number " + randomIndex + " as its target.");
        }
        else
        {
            target = null;
            Debug.LogError("An enemy couldn't find a GameObject with a tag Player. Target set to null");
        }

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

        //Decide a new command if the previous one has been executed.
        if (commandDecided == false)
        {
            DecideCommand();
        }

        //Set the animation to "Run" and change the state to moving if the GameObject's rigidbody is moving.
        if (!GetComponent<Rigidbody>().IsSleeping())
        {
            CharacterAnimator.CrossFade("Run", 0.0f);
            State = CharacterState.moving;
            Moving = true;
        }
        else {
            Moving = false;
        }

        //Change the animation and state to idle if the 
        SetMovementAnimation();

    }



    public EnemyPersonality Personality
    {
        get
        {
            return personality;
        }

        set
        {
            personality = value;
            Debug.Log("Enemy's personality changed to "+ personality.ToString() + ".");
        }
    }

    //Calculates a randomized command based on his personality that the enemy will execute.
    public void DecideCommand()
    {

        int command = random.Next(1, 10);

        //Aggressive personality uses mostly attacks and taunts.
        if (Personality == EnemyPersonality.aggressive)
        {

            if (command <= 6)
            {
                StartCoroutine(AttackCommand((float)random.Next(5, 10)));
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

        //Defensive personality likes to keep his distance to the target and attack sometimes. Taunts are rare.
        else if (Personality == EnemyPersonality.defensive)
        {

            if (command <= 5)
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

        //Jerks usually taunt the player and attack sometimes.
        else if (Personality == EnemyPersonality.jerk)
        {

            if (command <= 5)
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

        else
        {
            Debug.LogError("An enemy hasn't got a personality.");
        }

        commandDecided = true;

    }

    //Moves towards the selected target.
    private void MoveTowardsTarget()
    {

        if (CanMove == true)
        {

            //move towards the target

        }

    }

    private void Taunt()
    {
        CanMove = false;
        CharacterAnimator.CrossFade("Taunt", 0.0f);
    }



    protected override void Attack()
    {
        //CanMove = false;
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

        Debug.Log("An enemy decided to use attack command for " + duration + " seconds.");
        StartCoroutine(CommandDuration(duration));
        //select the target

        while (executingCommand == true)
        {
            MoveTowardsTarget();
            //hit him if you're close enough (call target's TakeDamage if there's a collision)
            yield return null;
        }



    }

    //Move away from the target.
    private IEnumerator DefendCommand(float duration)
    {

        Debug.Log("An enemy decided to use defend command for " + duration + " seconds.");
        StartCoroutine(CommandDuration(duration));

        while (executingCommand == true)
        {
            yield return null;
        }

    }

    //Taunt the target.
    private IEnumerator TauntCommand(float duration)
    {

        Debug.Log("An enemy decided to use taunt command for " + duration + " seconds.");
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
