using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MinionController : MonoBehaviour {

    //Create states
    public enum State { Search, Follow, Engage };
    //current state
    private State stateCM;

    //navmeshagent used for movement
    private NavMeshAgent minionNav;

    //Raycast for minion attacks
    private RaycastHit minionAtk;

    //overlapSphere collider array used for finding leaders
    private Collider[] searchRad;

    //overlapSphere collider array used for checking distance from other minions and finding enemies
    private Collider[] distCheck;

    //gameobject to hold the minion's current leader
    private GameObject leader;

    //target gameobject
    private GameObject targetEnemy;

    //playerbase
    private GameObject homebase;

    //set up alignment and separation vectors
    private Vector3 alignVector = new Vector3(0f, 0f, 0f);
    private Vector3 sepVector = new Vector3(0f, 0f, 0f);
    
    //vector to control minion movement in accordance with the flocking algorithm
    private Vector3 move;

    //weights for the algorithm
    private float alignment = 4f;
    private float separation = 1f;
    private float cohesion = 1f;

    //used to help minions determine the amount they should divide their separation distance by
    private int sepAmount = 0;

    //HP & Damage
    private int minHP;
    private int minDmg = 1;

    //attack cooldown
    private float minCooldown = 0;

    void Start()
    {
        //get the player base for the homebase
        homebase = GameObject.FindGameObjectWithTag("Player Base");

        //set minion hp
        minHP = 10;

        //calls method to swap states for the AI
        stateCM = State.Search;
        StateSwapper(State.Search);

        //gets the minion's navmeshagent
        minionNav = GetComponent<NavMeshAgent>();
    }

    //reduce cooldown over time
    void Update () {
        minCooldown -= 1f * Time.deltaTime;
    }

    //statemachine used to swap between states
    private void StateSwapper(State swap)
    {
        StopAllCoroutines();
        stateCM = swap;

        switch (stateCM)
        {
            case State.Search:

                StartCoroutine(FindLeader());
                break;
            case State.Follow:

                StartCoroutine(FollowUnit());
                break;
            case State.Engage:

                StartCoroutine(MinionEngage());
                break;
        }
    }

    IEnumerator FindLeader()
    {
        while (true)
        {

            //overlapsphere for detecting leaders
            searchRad = Physics.OverlapSphere(transform.position, 5);

            for (int i = 0; i < searchRad.Length; i++)
            {
                if (searchRad[i].tag == "Player Unit")
                {
                    //set the leader and tell them to add this minion as one of their minions before following them
                    leader = searchRad[i].gameObject;
                    leader.GetComponent<LeadUnitController>().addFriend(this.gameObject);
                    StateSwapper(State.Follow);
                    yield break;
                }
            }

            yield return null;
        }
    }
    IEnumerator FollowUnit()
    {
        while (true)
        {
            //if the leader is not dead
            if (leader != null)
            {
                //set the separation amount
                sepAmount = 2;
                
                //overlapsphere for detecting other minions and enemies
                distCheck = Physics.OverlapSphere(transform.position, 5);
                
                for (int i = 0; i < distCheck.Length; i++)
                {
                    //if it is a minion augment the separation vector by the difference in position between this minion and the other minion
                    if (distCheck[i].tag == "Player Minion")
                    {

                        sepVector += transform.position - distCheck[i].gameObject.transform.position;
                        
                    } else if (distCheck[i].tag == "Enemy" || distCheck[i].tag == "Enemy Base") //if it is an enemy or enemy base set it as a target and prepare to engage
                    {
                        targetEnemy = distCheck[i].gameObject;
                        StateSwapper(State.Engage);
                        yield break;
                    }
                }

                //prevent dividing by 0
                if (sepAmount >= 1)
                {
                    //divide the separation vector by the specified amount
                    sepVector = sepVector / sepAmount;
                }

                //calculate the final destination by taking the alignment vector, separation vector, and the average unit position - the minion's position as the cohesion vector and put them into the following equation multiplied by their weights
                //Destination = alignment weight * alignment + cohesion weight * cohesion +  separation weight * separation
                move = alignment * alignVector + cohesion * (leader.GetComponent<LeadUnitController>().getMinionDest(this.gameObject) - transform.position) + separation * sepVector;

                //move to the new position 
                Debug.DrawLine(transform.position, move, Color.yellow);
                minionNav.SetDestination(move);
            }
            else
            {
                //if the leader is destroyed, retreat to base and go back to searching for a new leader
                minionNav.SetDestination(homebase.transform.position);
                StateSwapper(State.Search);
                yield break;
            }

            yield return null;
        }
    }
    IEnumerator MinionEngage()
    {
        while (true)
        {
            //if the target has not been destroyed
            if (targetEnemy != null)
            {
                //go towards the target
                minionNav.SetDestination(targetEnemy.transform.position);

                //look at the target
                this.transform.LookAt(new Vector3(targetEnemy.transform.position.x, 0.5f, targetEnemy.transform.position.z));

                //when the cooldown is done
                if (minCooldown <= 0) {
                    //attack an enemy or enemy base in front of the minion
                    minionAtk = new RaycastHit();
                    if (Physics.Raycast(this.transform.position, transform.forward, out minionAtk, 1f) && (minionAtk.transform.gameObject.tag == "Enemy" || minionAtk.transform.gameObject.tag == "Enemy Base"))
                    {
                        //if it is an enemy send the damage to the enemy object's script otherwise send the damage to the base's script
                        if (minionAtk.transform.gameObject.tag == "Enemy")
                        {

                            targetEnemy.GetComponent<EnemyControl>().takeDmg(minDmg);
                            Debug.DrawRay(minionAtk.point, transform.forward, Color.magenta, 1f, false);

                        } else if (minionAtk.transform.gameObject.tag == "Enemy Base")
                        {
                            targetEnemy.GetComponent<BaseScript>().besieged(minDmg);
                            Debug.DrawRay(minionAtk.point, transform.forward, Color.magenta, 1f, false);
                        }
                        
                    }
                    //reset the cooldown
                    minCooldown = 3;
                }
            }
            else
            {
                //if the target is destroyed return to following the leader
                StateSwapper(State.Follow);
                yield break;
            }
                       
            yield return null;
        }
    }

    //upgrade methods
    //upgrade damage
    public void incDmg()
    {
        minDmg += 2;
        Debug.Log("Footmen Damage: " + minDmg);
    }
    //upgrade health
    public void incHp()
    {
        minHP += 5;
        Debug.Log("Footmen Health: " + minHP);
    }
    //calculate incoming damage
    public void calcDmg(int amount)
    {
        minHP -= amount;
        if (minHP <= 0)
        {
            if (leader != null)
            {
                //tell my leader I died
                leader.GetComponent<LeadUnitController>().recount(this.gameObject);
            }
            
            Destroy(this.gameObject);
        }
    }
    /*
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 5);
    }
    */
}
