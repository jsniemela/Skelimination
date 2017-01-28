using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class Enemy : Character
{
    public enum EnemyPersonality { aggressive, defensive, jerk };

    //personality determines the odds of randomized command actions.
    private EnemyPersonality personality;

    //target is the enemy's attacking target (a GameObject with a tag "Player")
    private GameObject target;

    //score determines the amount of points that the player gets from defeating this enemy.
    public int Score { get; set; } 

    //The enemy selects a new command based on his personality when commandDecided is false.
    private bool commandDecided;

    //executingCommand is true when the enemy is currently executing a command.
    private bool executingCommand;

    GameObject latestToucher;

    AudioSource audioSource;
    AudioClip attack1;
    AudioClip attack2;
    AudioClip taunt1;
    AudioClip taunt2;
    AudioClip death1;
    AudioClip death2;

    private void Awake()
    {
        InitializeEnemy();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        //Stop executing update methods if the enemy is dead.
        if (State == CharacterState.dead)
        {
            return;
        }

        //Kill the enemy if he drops below the imageTarget's y position.
        KillTheCharacterIfOutOfBounds();

        //Changes state and animation to idle and tries to select a new target (current Scene's GameObject with a tag "Player") 
        //randomly if there is no current target or if the target is dead.
        if (target == null || target.GetComponent<Player>().State == CharacterState.dead)
        {

            StopAllCoroutines();
            State = CharacterState.idle;
            Moving = false;
            CanMove = false;

            if (!CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                CharacterAnimator.CrossFade("Idle", 0.0f);
            }

            SelectTargetRandomly();

        }

        //Decide a new command if the previous one has been executed.
        if (commandDecided == false && executingCommand == false && target != null && State != CharacterState.dead)
        {
            CanMove = true;
            StopAllCoroutines();
            DecideCommand();
        }

        //Change the animation and state to idle or run according to the situation.
        SetMovementAnimation();

    }

    private void FixedUpdate()
    {

        //Lock rotation so that the character doesn't fall over.
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

    }

    //Selects a personality randomly and initializes the enemy's stats based on it.
    public void InitializeEnemy()
    {

        audioSource = GetComponent<AudioSource>();
        collisionGameObjects = new List<GameObject>();

        //Select a personality randomly.
        //Personality = (EnemyPersonality)random.Next(0, (int)Enum.GetNames(typeof(EnemyPersonality)).Length);
        Personality = (EnemyPersonality)RandomNumberGenerator.NextRandom(0, (int)Enum.GetNames(typeof(EnemyPersonality)).Length);
       

        //Initialize stats based on the personality.
        switch (Personality)
        {
            case EnemyPersonality.aggressive:
                SetStats(3, 3, 2, 6.0f, CharacterState.idle, 4.0f, GetComponent<Animator>(), true, false, 100, GameObject.FindGameObjectWithTag("ImageTarget"));

                //Assign voices based on the personality.
                try
                {
                    attack1 = (AudioClip)Resources.Load("Sounds/Voices/aggressive_attack1");
                    attack2 = (AudioClip)Resources.Load("Sounds/Voices/aggressive_attack2");
                    taunt1 = (AudioClip)Resources.Load("Sounds/Voices/aggressive_taunt1");
                    taunt2 = (AudioClip)Resources.Load("Sounds/Voices/aggressive_taunt2");
                    death1 = (AudioClip)Resources.Load("Sounds/Voices/aggressive_death1");
                    death2 = (AudioClip)Resources.Load("Sounds/Voices/aggressive_death2");
                }
                catch (Exception e) { }
                            
                break;
            case EnemyPersonality.defensive:

                try
                {
                    attack1 = (AudioClip)Resources.Load("Sounds/Voices/defensive_attack1");
                    attack2 = (AudioClip)Resources.Load("Sounds/Voices/defensive_attack2");
                    taunt1 = (AudioClip)Resources.Load("Sounds/Voices/defensive_taunt1");
                    taunt2 = (AudioClip)Resources.Load("Sounds/Voices/defensive_taunt2");
                    death1 = (AudioClip)Resources.Load("Sounds/Voices/defensive_death1");
                    death2 = (AudioClip)Resources.Load("Sounds/Voices/defensive_death2");
                }
                catch (Exception e) { }

                SetStats(4, 4, 1, 6.0f, CharacterState.idle, 3.0f, GetComponent<Animator>(), true, false, 100, GameObject.FindGameObjectWithTag("ImageTarget"));
                break;
            case EnemyPersonality.jerk:

                try
                {
                    attack1 = (AudioClip)Resources.Load("Sounds/Voices/jerk_attack1");
                    attack2 = (AudioClip)Resources.Load("Sounds/Voices/jerk_attack2");
                    taunt1 = (AudioClip)Resources.Load("Sounds/Voices/jerk_taunt1");
                    taunt2 = (AudioClip)Resources.Load("Sounds/Voices/jerk_taunt2");
                    death1 = (AudioClip)Resources.Load("Sounds/Voices/jerk_death1");
                    death2 = (AudioClip)Resources.Load("Sounds/Voices/jerk_death2");
                }
                catch (Exception e) { }

                SetStats(2, 2, 2, 6.0f, CharacterState.idle, 5.0f, GetComponent<Animator>(), true, false, 100, GameObject.FindGameObjectWithTag("ImageTarget"));
                break;
            default:

                try
                {
                    attack1 = (AudioClip)Resources.Load("Sounds/Voices/aggressive_attack1");
                    attack2 = (AudioClip)Resources.Load("Sounds/Voices/aggressive_attack2");
                    taunt1 = (AudioClip)Resources.Load("Sounds/Voices/aggressive_taunt1");
                    taunt2 = (AudioClip)Resources.Load("Sounds/Voices/aggressive_taunt2");
                    death1 = (AudioClip)Resources.Load("Sounds/Voices/aggressive_death1");
                    death2 = (AudioClip)Resources.Load("Sounds/Voices/aggressive_death2");
                }
                catch (Exception e) { }

                SetStats(2, 2, 2, 6.0f, CharacterState.idle, 3.5f, GetComponent<Animator>(), true, false, 100, GameObject.FindGameObjectWithTag("ImageTarget"));
                break;
        }

        try
        {
            //Set the superclass' audio settings.
            characterAudioSource = audioSource;
            destroyGameObject = (AudioClip)Resources.Load("Sounds/destroy_gameobject");
            swordAttack1 = (AudioClip)Resources.Load("Sounds/enemy_sword_attack");
            swordAttack2 = (AudioClip)Resources.Load("Sounds/player_sword_attack");
            swordSlash = (AudioClip)Resources.Load("Sounds/sword_slash");
        }
        catch (Exception e) {}
        

        Debug.Log("Enemy's personality set to "+ Personality.ToString() + ".");
        SelectTargetRandomly();
        commandDecided = false;
        executingCommand = false;

    }

    public void SetStats(int maxHealth, int health, int attackPower, float knockback, CharacterState state, float speed, Animator animator, bool canMove, bool moving, int score, GameObject ground)
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
        imageTarget = ground;
    }

    //Selects the attack target randomly from the GameObjects tagged with "Player". Sets the target to null if there aren't
    //any player objects in the scene.
    private void SelectTargetRandomly()
    {

        GameObject[] targetArray = GameObject.FindGameObjectsWithTag("Player");

        //Stop executing this method if there are no player objects in the scene.
        if (targetArray == null || targetArray.Length <= 0) {
            return;
        }

        //Shuffle the targetArray if there are multiple player objects.
        int n = targetArray.Length;
        while (n > 1)
        {
            //int k = random.Next(n--);
            int k = RandomNumberGenerator.NextRandom(n--);
            GameObject temp = targetArray[n];
            targetArray[n] = targetArray[k];
            targetArray[k] = temp;
        }

        //Iterate every randomized array index and try to find a player who is alive.
        foreach (GameObject player in targetArray)
        {

            if (player.GetComponent<Player>().State != CharacterState.dead)
            {
                target = player;
                RotateSmoothlyTowardsTarget(target.transform);
                //Debug.Log("An enemy selected a player as its target.");
                return;
            }
            else
            {
                target = null;
                //Debug.Log("An enemy couldn't find a valid player target.");
            }

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
            //Debug.Log("Enemy's personality changed to " + personality.ToString() + ".");
        }
    }

    //Calculates a randomized command based on his personality that the enemy will execute.
    //The commands are Coroutines that run for a certain period of time.
    public void DecideCommand()
    {

        //int command = random.Next(1, 10);
        int command = RandomNumberGenerator.NextRandom(1, 10);

        //Aggressive personality uses mostly attacks and taunts.
        if (Personality == EnemyPersonality.aggressive)
        {

            if (command <= 7)
            {
                StartCoroutine(AttackCommand((float)RandomNumberGenerator.NextRandom(4, 8)));
            }
            else if (command > 7 && command <= 9)
            {
                StartCoroutine(TauntCommand());
            }
            else
            {
                StartCoroutine(DefendCommand((float)RandomNumberGenerator.NextRandom(4, 7)));
            }

        }

        //Defensive personality likes to keep his distance to the target and attack sometimes. Taunts are rare.
        else if (Personality == EnemyPersonality.defensive)
        {

            if (command <= 4)
            {
                StartCoroutine(DefendCommand((float)RandomNumberGenerator.NextRandom(3, 7)));
            }
            else if (command > 4 && command <= 8)
            {
                StartCoroutine(AttackCommand((float)RandomNumberGenerator.NextRandom(3, 8)));
            }
            else
            {
                StartCoroutine(TauntCommand());
            }

        }

        //Jerks usually taunt the player and attack sometimes.
        else if (Personality == EnemyPersonality.jerk)
        {

            if (command <= 4)
            {
                StartCoroutine(TauntCommand());
            }
            else if (command > 4 && command <= 9)
            {
                StartCoroutine(AttackCommand((float)RandomNumberGenerator.NextRandom(3, 6)));
            }
            else
            {
                StartCoroutine(DefendCommand((float)RandomNumberGenerator.NextRandom(3, 4)));
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

        if (CanMove == true
        && State != CharacterState.knockback
        && State != CharacterState.dead
        && State != CharacterState.attack
        && target != null)
        {

            Moving = true;

            float step = Speed * Time.deltaTime;

            RotateSmoothlyTowardsTarget(targetPosition);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition.position, step);

        }

    }

    //Moves the enemy away from the target if he is within the avoidingDistance. If the target is outside the avoidinDistance,
    //the enemy will stay still and use "Defend" animation. 
    private void AvoidTarget(Transform targetPosition, float avoidingDistance)
    {

        if (CanMove == true
        && State != CharacterState.knockback
        && State != CharacterState.dead
        && State != CharacterState.knockback
        && target != null)
        {

            float step = -1 * Speed * Time.deltaTime;
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition.position);

            if (distanceToTarget < avoidingDistance)
            {

                Moving = true;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition.position, step);
                transform.LookAt(targetPosition);

                if (!CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                {
                    CharacterAnimator.CrossFade("Walk", 0.0f);
                }

            }
            else
            {
                Moving = false;
                State = CharacterState.defend;
                RotateSmoothlyTowardsTarget(targetPosition);
                //GetComponent<Rigidbody>().velocity = Vector3.zero;

                if (!CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Defend"))
                {
                    CharacterAnimator.CrossFade("Defend", 0.0f);
                }

            }

        }

    }

    private void RotateSmoothlyTowardsTarget(Transform targetPosition)
    {
        if (targetPosition != null) {

            Vector3 targetPoint = targetPosition.position;
            Quaternion targetRotation = Quaternion.LookRotation(targetPoint - transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 2.5f);

        }
        
    }

    //Enemy will attack the target if it is close enough (within the given minDistance).
    private void AttackCharacterIfCloseEnough(Transform targetPosition, float minDistance)
    {

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition.position);

        if (distanceToTarget < minDistance && State != CharacterState.knockback && State != CharacterState.dead && State != CharacterState.attack)
        {
            RpcAttack();
        }

    }

    //Deals damage, knocks the enemy back and selects the attacker as the new target.
    public override void TakeDamage(int damage, float knockback, GameObject attacker)
    {

        if (State != CharacterState.dead && State != CharacterState.knockback)
        {

            Health = Health - damage;

            int random = RandomNumberGenerator.NextRandom(1, 5);

            audioSource.Stop();

            if (random == 1) {
                audioSource.PlayOneShot(attack1);
            }
            else if (random == 2)
            {
                audioSource.PlayOneShot(attack2);
            }

            //Call die if the characte's health goes to zero.
            if (Health < 1)
            {
                Die(attacker);
            }
            //The enemy is still alive. Change to damage animation and stop him for a few seconds.
            else
            {
                
                if (!CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Damage"))
                {
                    CharacterAnimator.CrossFade("Damage", 0.0f);

                }

                //Select the attacker as a new target if he is still alive.
                if (attacker.tag.Equals("Player") && attacker.GetComponent<Player>().State != CharacterState.dead)
                {
                    target = attacker;
                }

                StartCoroutine(StopCharacter(2.0f));

                State = CharacterState.knockback;

            }

        }

    }
        
    //Lauches the attack animation and stops movement for a while.
    protected override void RpcAttack()
    {
        StartCoroutine(StopCharacter(3.0f));

        if (!CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            CharacterAnimator.CrossFade("Attack", 0.0f);
            State = CharacterState.attack;
            StartCoroutine(attackDelay(1.0f));
        }

        //Debug.Log("Enemy attacked.");
    }

    //Kills the enemy and gives the points to the killer.
    protected override void Die(GameObject killer)
    {

        if (State != CharacterState.dead) {

            GetComponent<Rigidbody>().mass = 0.5f;
            CharacterAnimator.CrossFade("Death", 0.0f);
            State = CharacterState.dead;
            CanMove = false;
            Moving = false;

            int random = RandomNumberGenerator.NextRandom(1, 4);

            audioSource.Stop();

            if (random == 1)
            {
                audioSource.PlayOneShot(death1);
            }
            else if (random == 2)
            {
                audioSource.PlayOneShot(death2);
            }

            //If a valid killer object is given (a gameobject with the Player script) 
            //and the player's state isn't dead.
            if (killer != null && killer.GetComponent<Player>().State != CharacterState.dead)
            {
                //Add this enemy's score and a kill to the player.
                killer.GetComponent<Player>().Score += Score;
                killer.GetComponent<Player>().Kills += 1;
                Debug.Log("A player killed an enemy and received " + Score + " points.");
            }

            //A valid killer argument wasn't given.
            else if (killer == null) {

                //Give the latest player who has touched this enemy the points.
                if (latestToucher != null && latestToucher.GetComponent<Player>().State != CharacterState.dead)
                {
                    latestToucher.GetComponent<Player>().Score += Score;
                    latestToucher.GetComponent<Player>().Kills += 1;
                    Debug.Log("A player convinced an enemy to kill himself or pushed him off the platform and received " + Score + " points.");
                }
                else
                {
                    Debug.Log("An enemy killed himself.");
                }

           }

           //Wait for five seconds before destroying this GameObject.
           StartCoroutine(WaitAndDestroy(10.0f, gameObject));

        }

    }

    //Stores the latest player who has touched this enemy to give him points
    //if this enemy decides to walk off the cliff.
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Player")
        {
            if (State != CharacterState.dead)
            {
                latestToucher = collision.gameObject;
            }
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {

            if (!collisionGameObjects.Contains(collision.gameObject))
            {
                collisionGameObjects.Add(collision.gameObject);
                //Debug.LogError("Gameobject entered collision.");
                //Debug.LogError("List size is now " + collisionGameObjects.Count +".");
            }

        }


    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {

            if (collisionGameObjects.Contains(collision.gameObject))
            {
                collisionGameObjects.Remove(collision.gameObject);
                //Debug.LogError("Gameobject exited collision.");
                //Debug.LogError("List size is now " + collisionGameObjects.Count + ".");
            }

        }
    }

    //Move towards the selected target and try to hit him after you get close enough.
    private IEnumerator AttackCommand(float duration)
    {

        Debug.Log("An enemy decided to use attack command for " + duration + " seconds.");
        StartCoroutine(CommandDuration(duration));
        float attackDistance = (float)RandomNumberGenerator.NextRandom(3, 5);

        while (executingCommand == true)
        {

            if (target != null && target.GetComponent<Player>().State != CharacterState.dead && State != CharacterState.dead)
            {
                MoveTowardsTarget(target.transform);
                AttackCharacterIfCloseEnough(target.transform, attackDistance);
            }

            yield return null;

        }

        yield return null;

    }

    //Keep distance to the target.
    private IEnumerator DefendCommand(float duration)
    {

        Debug.Log("An enemy decided to use defend command for " + duration + " seconds.");
        StartCoroutine(CommandDuration(duration));

        float avoidingDistance = (float)RandomNumberGenerator.NextRandom(7, 10);

        while (executingCommand == true)
        {

            if (target != null && target.GetComponent<Player>().State != CharacterState.dead && State != CharacterState.dead)
            {
                AvoidTarget(target.transform, avoidingDistance);
                //AttackCharacterIfCloseEnough(target.transform, attackDistance);
            }

            yield return null;

        }

        yield return null;

    }

    //Taunt the target.
    private IEnumerator TauntCommand()
    {

        Debug.Log("An enemy decided to use taunt command.");
        executingCommand = true;

        if (target != null && target.GetComponent<Player>().State != CharacterState.dead && State != CharacterState.dead)
        {
            CanMove = false;
            Moving = false;
            RotateSmoothlyTowardsTarget(target.transform);

            yield return new WaitForSeconds(1f);

            if (!CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Taunt")) {

                //Play either one of the taunt sound effect randomly.
                int random = RandomNumberGenerator.NextRandom(1, 6);

                audioSource.Stop();

                if (random == 1)
                {
                    audioSource.PlayOneShot(taunt1);
                }
                if (random == 2){
                    audioSource.PlayOneShot(taunt2);
                }
                
                CharacterAnimator.CrossFade("Taunt", 0.0f);
            }
            
            yield return new WaitForSeconds(1.5f);

            CanMove = true;

        }
     
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
