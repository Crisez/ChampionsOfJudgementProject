using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyControl : MonoBehaviour {

    //Create states
    public enum State { Search, Engage };
    //current state
    private State stateC;

    //Raycast for enemy attacks
    private RaycastHit enemyAtk;

   //overlapSphere collider array
    private Collider[] searchRadius;

    //a vector to help adjust the enemy attacks so they can hit the minions
    private Vector3 heightAdjust = new Vector3(0f, -0.2f, 0f);

    //the navmeshagent that allows for movement
    private NavMeshAgent enemyNav;

    //used to reference the manager to give upgrade points
    private GameManager manager;

    //The enemies original position for the enemy to return to after winning
    private Vector3 originalPosition;

    //Health & Damage
    private int hp;
    private int damage = 5;
    
    //attack cooldown
    private float cooldown = 0;

    //the unit to be attacked
    private GameObject targetPlayer;

    void Start () {
        //get the gamemanager reference setup
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();

        //set starting hp
        hp = 15;

        //get the navmeshagent from the specific enemy
        enemyNav = GetComponent<NavMeshAgent>();

        //set the original position
        originalPosition = this.transform.position;

        //calls method to swap states for the AI
        stateC = State.Search;
        StateSwap(State.Search);
    }

    //lower the attack cooldown over time
    void Update()
    {
        cooldown -= 1f * Time.deltaTime;
    }
    
    //statemachine used to swap between states
    private void StateSwap(State swap)
    {
        StopAllCoroutines();
        stateC = swap;

        switch (stateC) {
            case State.Search:
                
                StartCoroutine(Look());
                break;
            case State.Engage:

                StartCoroutine(EnemyEngage());
                break;
        }
    }

    IEnumerator Look()
    {
        while (true)
        {

            //Overlapsphere detection that detects colliders that enter the sphere around the enemy
            searchRadius = Physics.OverlapSphere(transform.position, 10);
           
            for (int i = 0; i < searchRadius.Length; i++)
            {
               //if the unit detected is a minion then set them as the target and swap to the engage state. Otherwise if it is a leader target it and move to the engage state
                if (searchRadius[i].tag == "Player Minion")
                {
                    targetPlayer = searchRadius[i].gameObject;
                    StateSwap(State.Engage);
                    yield break;
                } 
                else if (searchRadius[i].tag == "Player Unit")
                {
                    targetPlayer = searchRadius[i].gameObject;
                    StateSwap(State.Engage);
                    yield break;
                }
            }

           yield return null;
        }
    }
    IEnumerator EnemyEngage()
    {
        while (true)
        {
            //if the target is still alive
            if (targetPlayer != null)
            {
                //go to the target
                enemyNav.SetDestination(targetPlayer.transform.position);

                //look at the target without looking up or down too far
                this.transform.LookAt(new Vector3(targetPlayer.transform.position.x, 0.74f, targetPlayer.transform.position.z));

                //if the cooldown is finished attack
                if (cooldown <= 0)
                {
                    //raycast that hits either leader or minion in front of the enemy with a height adjustment
                    enemyAtk = new RaycastHit();
                    if (Physics.Raycast(this.transform.position + heightAdjust, transform.forward, out enemyAtk, 1f) && (enemyAtk.transform.gameObject.tag == "Player Unit" || enemyAtk.transform.gameObject.tag == "Player Minion"))
                    {
                        //if the target is a leader send the damage to the leaderscript else if the target is a minion send the damage to the gameobject's minion script.
                        if (enemyAtk.transform.gameObject.tag == "Player Unit")
                        {

                            targetPlayer.GetComponent<LeadUnitController>().receiveDmg(damage);
                            Debug.DrawRay(enemyAtk.point, transform.forward, Color.green, 1f, false);

                        } else if (enemyAtk.transform.gameObject.tag == "Player Minion")
                        {

                            targetPlayer.GetComponent<MinionController>().calcDmg(damage);
                            Debug.DrawRay(enemyAtk.point, transform.forward, Color.green, 1f, false);

                        }
                        
                    }
                    //reset the cooldown
                    cooldown = 5;
                }
            }
            else
            {
                //if the target is dead return to original position and resume search
                enemyNav.SetDestination(originalPosition);
                StateSwap(State.Search);
                yield break;
            }
            
            yield return null;
        }
    }

    //method to receive damage from players
    public void takeDmg(int amount)
    {
        hp -= amount;
        if(hp <= 0)
        {
            //send upgrade points to the manager before dying
            manager.earnPoints();
            Destroy(this.gameObject);
        }
    }

    /*
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 10);
    }
    */
}
