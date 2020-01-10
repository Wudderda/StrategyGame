using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Unit : MonoBehaviour
{
    [Header("Attributes")]
    public int health;
    public float moveSpeed;
    public int level;
    public int exp;
    public string unitName;

    [Header("Control Variables")]
    public State aiState;
    public Idle_State idleState;
    public Attribute[] attributes;

    public enum Idle_State { AGGRESSIVE, STAND_GROUND };
    public enum State { IDLE, ATTACK, MOVE };
    public enum Attribute { ATTACKER, SWIMMER, AIR_FORCE };


    private Attacker attacker;
    Animator animator;
    public Camera main_camera;
    public NavMeshAgent agent;
    public GameObject Controller;
    private Vector3 pos;
    public GameObject vfx;
    bool is_moving;
    bool is_dead;
    bool chase_target = false;
    float chase_timer = 0f;
    void Start()
    {
        is_dead = false;
        this.gameObject.tag = "Unit";
        attacker = this.gameObject.GetComponent<Attacker>();
        vfx.GetComponent<ParticleSystem>().Pause();
        animator = GetComponent<Animator>();
        Controller.GetComponent<Game_Controller>().AddUnit(gameObject);
    }
    void Update()
    {
        if (chase_timer <= 0.3f)
        {
            chase_timer += Time.deltaTime;
        }
        if (chase_target)
        {
            Chase_Target(attacker.target);
        }
        if (agent.hasPath == false) // when the movement is finished go back to idle animation.
        {
            is_moving = false;
            animator.SetBool("is_walking", false);
        }
        switch (aiState)
        {
            case State.ATTACK:
                attacker.PerformAction();
                Debug.Log("Attack state performing action");
                break;
            case State.MOVE:
                break;
        }
    }
    private void Chase_Target(Unit target){
        if (chase_timer >= 0.3f && target) // chasing the target until he dies or told otherwise.
        {
            if (Vector3.Distance(gameObject.transform.position, target.gameObject.transform.position) > 10000f) // stop chasing condition 1
            {
                Set_Chase(false);
                attacker.target = null;
            }
            chase_timer = 0;
            Move_to(target.gameObject.transform.position);
            Debug.Log("HEADing to " + target.gameObject.transform.position);
        }
    }
    public void Set_Chase(bool state)
    {
        Debug.Log("chasing");
        chase_target = state;
    }
    public bool is_chasing()
    {
        return chase_target;
    }
    public void SetState(State newState)
    {
        State currentState = aiState;
        aiState = newState;
        if (newState == State.IDLE)
            idleState = Idle_State.STAND_GROUND;

        if (currentState == State.MOVE)
        {
            // TODO
            // attacker SelectTarget
        }


    }

    public bool TakeDamage(int damage)
    {
        Debug.Log("Taken damage health drops from: " + this.health + " to: " + (this.health - damage));
        this.health -= damage;
        if (this.health <= 0)
        {
            Debug.Log("Dead");
            return true;
        }

        return false;
    }






    public bool hasPath()
    {// since agent.hasPath() is not set immediately we needed to set is moving variable.
        return is_moving;
    }
    public void Move_to(Vector3 point)
    {
        agent.isStopped = false;
        //chase_target = false;
        SetState(State.MOVE);
        is_moving = true;
        // agent.FindClosestEdge function could be used to make a character take cover as close to the wall as possible .
        if (animator.GetBool("attack"))
        {
            set_attack(false);
        }
        if (!animator.GetBool("is_walking"))
        {
            animator.SetBool("is_walking", true);
        }
            agent.SetDestination(point);
    }
    public void select()
    {
        vfx.GetComponent<ParticleSystem>().Play();
    }
    public void deselect()
    {
        vfx.GetComponent<ParticleSystem>().Stop();
    }
    public void removeYourself()
    {
        Controller.GetComponent<Game_Controller>().DeleteUnit(gameObject);
    }
    public void set_attack(bool state)
    {
        if (!state)
        {
            animator.StopPlayback();
        }
        animator.SetBool("attack",state);
    }
    public void die()
    {
        Stop_moving();
        is_dead = true;
        animator.SetBool("die",true);
        Invoke("destroy" ,3);
    }
    public void destroy()
    {
        Destroy(gameObject);
    }
    public bool is_Dead()
    {
        return is_dead;
    }
    public void lookAt(Transform target)
    {
        gameObject.transform.LookAt(target, Vector3.up);
    }
    public void Stop_moving()
    {
        agent.isStopped = true;
        is_moving = false;
        animator.SetBool("is_walking", false);
    } 
}
