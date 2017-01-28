using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;

public enum CharacterState { idle, moving, knockback, taunt, frozen, dead, attack, defend };

abstract public class Character : NetworkBehaviour
{
    
    public int MaxHealth { get; set; }
    [SyncVar]
    private int health;
    public int AttackPower { get; set; }
    public float Knockback { get; set; }
    [SyncVar]
    private CharacterState state;
    public float Speed { get; set; }
    public Animator CharacterAnimator { get; set; }
    public bool CanMove { get; set; }
    public bool Moving { get; set; }
    public GameObject imageTarget;
    public List<GameObject> collisionGameObjects;

    protected AudioSource characterAudioSource;
    protected AudioClip destroyGameObject;
    protected AudioClip swordAttack1;
    protected AudioClip swordAttack2;
    protected AudioClip swordSlash;

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
                //Debug.Log("Health set to 0.");
                //Die();
            }
            else if (value > MaxHealth)
            {
                health = MaxHealth;
                //Debug.Log("New health value is greater than maxHealth. Health set to maxHealth.");
            }
            else
            {
                health = value;
                //Debug.Log("Health changed to "+value);
            }
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
                //Debug.Log("Character's state changed to " + state.ToString());
            }

        }
    }

    //Check if the character has fallen below the imageTarget and kill him if he has.
    public void KillTheCharacterIfOutOfBounds()
    {
        if (imageTarget != null && gameObject.transform.position.y < imageTarget.transform.position.y - 5f && State != CharacterState.dead) {

            Die(null);

        }
    }
  
    //Changes character's state to idle if the currently playing animation is  called "Idle". Also allows the 
    //character to move again by setting CanMove to true. All the animator's animations except "Death" crossfade to "Idle" automatically.   
    public void SetMovementAnimation()
    {

        if (Moving == false && CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && State != CharacterState.dead)
        {
                        
            State = CharacterState.idle;          
            CanMove = true;

        }
        else if (Moving == true && CanMove == true && State != CharacterState.knockback && State != CharacterState.dead) {

            if (State != CharacterState.moving)
            {
                State = CharacterState.moving;
            }

            if (!CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Run") && !CharacterAnimator.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
            {
                CharacterAnimator.CrossFade("Run", 0.0f);

            }
        }
    }

    public virtual void TakeDamage(int damage, float knockback, GameObject attacker) { }

    protected virtual void RpcAttack() { }

    protected virtual void Die(GameObject killer) { }

    protected IEnumerator StopCharacter(float waitDuration)
    {
        CanMove = false;
        Moving = false;
        yield return new WaitForSeconds(waitDuration);
        CanMove = true;
        yield return null;
    }

    //Wait, play a sound effect, disable the object, wait, destroy.
    protected IEnumerator WaitAndDestroy(float waitDuration, GameObject objectToBeDestroyed)
    {
        yield return new WaitForSeconds(waitDuration);
        characterAudioSource.PlayOneShot(destroyGameObject, 0.25f);
        //GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(1.3f);
        Destroy(objectToBeDestroyed);
    }

    protected IEnumerator WaitAndDisable(float waitDuration, GameObject objectToBeDisabled)
    {
        yield return new WaitForSeconds(waitDuration);
        characterAudioSource.PlayOneShot(destroyGameObject);
        objectToBeDisabled.SetActive(false);
    }

    //Waits for a given time (so that the attack animation can progress to the correct position) and
    //calls TakeDamage() method of every Enemy or Player who are currently stored in the collisionGameObjects list.
    protected IEnumerator attackDelay(float waitDuration)
    {
        
        yield return new WaitForSeconds(waitDuration);

        characterAudioSource.PlayOneShot(swordSlash, 0.15f);

        //If there is at least one gameobject in the collisionGameObjects, iterate them, call
        //their TakeDamage and move them accordingly. 
        if (collisionGameObjects.Count > 0)
        {          

            int random = RandomNumberGenerator.NextRandom(1, 3);

            //characterAudioSource.Stop();

            if (random == 1)
            {
                characterAudioSource.PlayOneShot(swordAttack2);
            }
            else
            {
                characterAudioSource.PlayOneShot(swordAttack1);
            }

            foreach (GameObject g in collisionGameObjects)
            {
                try
                {
                    if (g.gameObject.tag == "Skeleton" && g.gameObject.GetComponent<Enemy>().State != CharacterState.dead)
                    {
                        g.GetComponent<Enemy>().TakeDamage(AttackPower, Knockback, gameObject);
                        g.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        g.GetComponent<Rigidbody>().velocity = transform.forward * Knockback;
                        //Debug.LogError("Attack collided with GameObject " +i + ".");
                    }
                    else if (g.gameObject.tag == "Player" && g.gameObject.GetComponent<Player>().State != CharacterState.dead)
                    {
                        g.GetComponent<Player>().TakeDamage(AttackPower, Knockback, gameObject);
                        g.GetComponent<Rigidbody>().velocity = Vector3.zero;
                        g.GetComponent<Rigidbody>().velocity = transform.forward * Knockback;
                        //Debug.LogError("Attack collided with GameObject " + i + ".");
                    }

                }
                catch (MissingReferenceException e)
                {

                }
            }

        }

        yield return new WaitForSeconds(1f);
        yield return null;

    }

}
