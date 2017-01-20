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
            Debug.Log("An enemy couldn't find a GameObject with a tag Player. Target set to null");
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

    }

    private void FixedUpdate()
    {
        //Decide a new command if the previous one has been executed.
        if (commandDecided == false)
        {
            StopAllCoroutines();
            DecideCommand();
        }

        //Change the animation and state to idle if needed.
        SetMovementAnimation();

        if (target == null && GameObject.FindGameObjectsWithTag("Player").Length > 0) {
            SelectTargetRandomly();
        }
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
    //The commands are Coroutines that run for a certain period of time.
    public void DecideCommand()
    {

        int command = random.Next(1, 10);

        //Aggressive personality uses mostly attacks and taunts.
        if (Personality == EnemyPersonality.aggressive)
        {

            if (command <= 5)
            {
                StartCoroutine(AttackCommand((float)random.Next(5, 10)));
            }
            else if (command <= 8)
            {
                StartCoroutine(TauntCommand());
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
                StartCoroutine(TauntCommand());
            }

        }

        //Jerks usually taunt the player and attack sometimes.
        else if (Personality == EnemyPersonality.jerk)
        {

            if (command <= 5)
            {
                StartCoroutine(TauntCommand());
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

    //Moves the enemy towards the selected target.
    private void MoveTowardsTarget(Transform targetPosition)
    {

        if (CanMove == true && State != CharacterState.knockback && State != CharacterState.dead && target != null)
        {

            float step = Speed * Time.deltaTime;
            
            transform.position = Vector3.MoveTowards(transform.position, targetPosition.position, step);
            transform.LookAt(targetPosition);
            SetEnemyToRun();

        }

    }

    //Moves the enemy away from the target if he is within the avoidingDistance. If the target is outside the avoidinDistance,
    //the enemy will stay still and use "Defend" animation. 
    private void AvoidTarget(Transform targetPosition, float avoidingDistance)
    {

        if (CanMove == true && State != CharacterState.knockback && State != CharacterState.dead && target != null)
        {

            float step = -1 * Speed * Time.deltaTime;
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition.position);

            if (distanceToTarget < avoidingDistance && State != CharacterState.knockback && State != CharacterState.dead)
            {

                transform.position = Vector3.MoveTowards(transform.position, targetPosition.position, step);
                //Vector3 dir = Vector3.zero;
                //transform.rotation = Quaternion.LookRotation(dir);
                SetEnemyToRun();

            }
            else {
                transform.LookAt(targetPosition);
                Moving = false;
                CharacterAnimator.CrossFade("Defend", 0.0f);
            }
            
        }

    }

    //A method for initializing run state and animation quickly.
    private void SetEnemyToRun() {

        State = CharacterState.moving;
        Moving = true;

        if (!CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            CharacterAnimator.CrossFade("Run", 0.0f);
        }

    }

    //Enemy will attack the target if it is close enough (within the given minDistance).
    private void AttackCharacterIfCloseEnough(Transform targetPosition, float minDistance) {

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition.position);

        if (distanceToTarget < minDistance && State != CharacterState.knockback && State != CharacterState.dead)
        {
            Attack();
        }

    }

    //Deals damage, knocks the enemy back and selects the attacker as the new target.
    public override void TakeDamage(int damage, float knockback, GameObject attacker)
    {

        if (State != CharacterState.dead && State != CharacterState.knockback)
        {

            Health = Health - damage;
            CharacterAnimator.CrossFade("Knockback", 0.0f);
            State = CharacterState.knockback;
            target = attacker;
            StartCoroutine(StopCharacter(2.0f));

            //executingCommand = false;
            //commandDecided = false;

        }

    }

    //Lauches the attack animation and stops movement for a while.
    protected override void Attack()
    {
        CanMove = false;
        Moving = false;
        CharacterAnimator.CrossFade("Attack", 0.0f);
        State = CharacterState.attack;
        //Debug.Log("Enemy attacked.");
    }

    //Kills the enemy, gives the player points and notifies the GameManager.
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
        float attackDistance = (float)random.Next(2, 5);
        //select the target

        while (executingCommand == true)
        {
            if (target != null) {
                MoveTowardsTarget(target.transform);
                AttackCharacterIfCloseEnough(target.transform, attackDistance);
            }

            yield return null;

        }

        SelectTargetRandomly();
        yield return null;

    }

    //Keep distance to the target.
    private IEnumerator DefendCommand(float duration)
    {

        Debug.Log("An enemy decided to use defend command for " + duration + " seconds.");
        StartCoroutine(CommandDuration(duration));
        float avoidingDistance = (float)random.Next(7, 10);
        //float attackDistance = (float)random.Next(1, 3);

        while (executingCommand == true)
        {

            if (target != null)
            {
                AvoidTarget(target.transform, avoidingDistance);
                //AttackCharacterIfCloseEnough(target.transform, attackDistance);
            }
            
            yield return null;

        }

        SelectTargetRandomly();
        yield return null;

    }

    //Taunt the target.
    private IEnumerator TauntCommand()
    {

        Debug.Log("An enemy decided to use taunt command.");

        if (target != null)
        {
            transform.LookAt(target.transform);
        }
        else {
            SelectTargetRandomly();
        }
        
        executingCommand = true;
        CanMove = false;
        Moving = false;
        CharacterAnimator.CrossFade("Taunt", 0.0f);
        yield return new WaitForSeconds(3f);

        executingCommand = false;
        commandDecided = false;

        yield return null;

    }

    //Determines the length of selected commands.
    private IEnumerator CommandDuration(float duration)
    {

        executingCommand = true;
        yield return new WaitForSeconds(duration);
        executingCommand = false;
        commandDecided = false;
        yield return null;

    }


}
