using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LeadUnitController : MonoBehaviour {

    //navmeshagent for movement
    private NavMeshAgent navi;

    //list to hold all minions following this leader
    private List<GameObject> minionsList;
    
    //the number of minions the leader has
    private int minNum = 0;

    //vectors for calculating the average position and the final position of minions
    private Vector3 avgPos;
    private Vector3 finalPos;

    //Create states
    public enum State { Search, Fight};
    //current state
    private State current;

    //Destination to give minions
    private Vector3 currentDest;

    //overlapSphere collider array for finding enemies
    private Collider[] searchForEnemy;

    //target
    private GameObject enemy;

    //reference the manager
    private GameManager theManager;

    //Raycast for attacking
    private RaycastHit leadAtk;

    //vector to adjust height in case the enemies ever get minions
    private Vector3 alterHeight = new Vector3(0f, -0.2f, 0f);

    //Health & Damage
    private int unitHealth;
    private int dmg = 5;
    
    //attack cooldown
    private float cool = 0;

    void Start () {

        //get the manager
        theManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        //tell the GameManager I exist
        theManager.rollcall(true);

        //set health
        unitHealth = 15;

        //set the current state
        current = State.Search;
        changeState(State.Search);

        //get the navmeshagent from the leader gameobject
        navi = GetComponent<NavMeshAgent>();

        //make the list to hold the minions
        minionsList = new List<GameObject>();
    }
	
    //Lead Unit recieves orders to go to a location
	public void targetPosition(Vector3 destination)
    {
        currentDest = destination;
        navi.SetDestination(destination);
    }

    //reduce cooldown over time
    void Update()
    {
        cool -= 1f * Time.deltaTime;
    }

    //statemachine used to swap between states
    private void changeState(State swap)
    {
        StopAllCoroutines();
        current = swap;

        switch (current)
        {
            case State.Search:

                StartCoroutine(FindEnemy());
                break;
            case State.Fight:

                StartCoroutine(FightEnemy());
                break;
        }
    }

    IEnumerator FindEnemy()
    {
        while (true)
        {
            //overlapsphere detection for enemies or the enemy base
            searchForEnemy = Physics.OverlapSphere(transform.position, 5);

            for (int i = 0; i < searchForEnemy.Length; i++)
            {
                //if we find an enemy or enemy base, make them the target
                if (searchForEnemy[i].tag == "Enemy" || searchForEnemy[i].tag == "Enemy Base")
                {
                    enemy = searchForEnemy[i].gameObject;
                    changeState(State.Fight);
                    yield break;
                }
            }


            yield return null;
        }
    }

    IEnumerator FightEnemy()
    {
        while (true)
        {
            //if the target has not been destroyed
            if (enemy != null)
            {
                //go to the target
                navi.SetDestination(enemy.transform.position);

                //look at the enemy
                this.transform.LookAt(new Vector3(enemy.transform.position.x, 0.5f, enemy.transform.position.z));

                //if the cooldown is over, attack
                if (cool <= 0)
                {
                    //attack enemies or enemy bases in front of the lead unit
                    leadAtk = new RaycastHit();
                    if (Physics.Raycast(this.transform.position + alterHeight, transform.forward, out leadAtk, 2f) && (leadAtk.transform.gameObject.tag == "Enemy" || leadAtk.transform.gameObject.tag == "Enemy Base"))
                    {
                        //if it is an enemy send the damage to the enemy gameobject's script otherwise send it to the base's script
                        if (leadAtk.transform.gameObject.tag == "Enemy")
                        {
                            enemy.GetComponent<EnemyControl>().takeDmg(dmg);
                            Debug.DrawRay(leadAtk.point, transform.forward, Color.cyan, 2f, false);

                        } else if (leadAtk.transform.gameObject.tag == "Enemy Base")
                        {
                            enemy.GetComponent<BaseScript>().besieged(dmg);
                            Debug.DrawRay(leadAtk.point, transform.forward, Color.cyan, 2f, false);
                        }
                       
                    }
                    //reset the cooldown
                    cool = 5;
                }
            }
            else
            {
                //if the enemy is destroyed, go back to looking for more enemies
                changeState(State.Search);
                yield break;
            }


            yield return null;
        }
    }

//Lead Unit gives the minion their destination
public Vector3 getMinionDest(GameObject currentMinion)
    {
        avgPos = new Vector3(0f, 0.5f, 0f);
       
        //for every minion, take their position and begin to calculate the average position
        foreach (GameObject min in minionsList)
        {
            
                avgPos.x += min.transform.position.x;
                avgPos.z += min.transform.position.z;
          
        }

        
        //calculate the final position by taking the average x and z positions divided by the total minion numbers + the leader units position
        finalPos = new Vector3((avgPos.x / minNum) + currentDest.x, avgPos.y, (avgPos.z / minNum) + currentDest.z);

        //if the leader only has one minion, give him a predefined position
        if (minNum == 1)
        {
            finalPos = finalPos + new Vector3(3, 0, 3);
        }

        return finalPos;

    }

    //lead unit adds a minion to their group
    public void addFriend(GameObject minion)
    {
        minionsList.Add(minion);
        minNum++;
    }

    //upgrade methods
    //increase leader damage
    public void increaseDamage()
    {
        dmg += 5;
        Debug.Log("Knight Damage: " + dmg);
    }
    //increase leader health
    public void increaseHealth()
    {
        unitHealth += 10;
        Debug.Log("Knight Health: " + unitHealth);
    }

    //calculate incoming damage
    public void receiveDmg(int amount)
    {
        unitHealth -= amount;
        if (unitHealth <= 0)
        {
            //tell the game manager I died
            theManager.rollcall(false);
            Destroy(this.gameObject);
        }
    }

    //when a minion dies remove him from the leader's group of minions
    public void recount(GameObject minion)
    {
        minionsList.Remove(minion);
        minNum--;
    }
}
