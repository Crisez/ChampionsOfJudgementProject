using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameManager : MonoBehaviour {

    //instance for game manager
    public static GameManager inst;

    //arrays for holding all units of a specific type
    private GameObject[] allMinions;
    private GameObject[] allLeaders;

    //keep track of the number of leaders to allow for a player loss
    private int leaders = 0;

    //upgrade points
    private int upPoints = 0;

    //upgrade buttons
    [SerializeField]
    private Button btnMinDmg;
    [SerializeField]
    private Button btnLeadDmg;
    [SerializeField]
    private Button btnMinHealth;
    [SerializeField]
    private Button btnLeadHealth;

    //point visuals and the background for the upgrade menu
    [SerializeField]
    private RawImage purpleBackground;
    [SerializeField]
    private Text totalPoints;

    private void Awake()
    {
        //singleton pattern
        if (inst == null)
        {
            //establish instance
            inst = this;
        }
        else if (inst != this)
        {
            //get rid of extra instances
            Destroy(gameObject);
        }

    }

    private void Start()
    {
        //button listeners
        btnMinDmg.onClick.AddListener(upMinDmg);
        btnLeadDmg.onClick.AddListener(upLeadDmg);
        btnMinHealth.onClick.AddListener(upMinHp);
        btnLeadHealth.onClick.AddListener(upLeadHp);

        //hide the buttons
        btnMinDmg.gameObject.SetActive(false);
        btnLeadDmg.gameObject.SetActive(false);
        btnMinHealth.gameObject.SetActive(false);
        btnLeadHealth.gameObject.SetActive(false);

        //hide the purple background and points
        purpleBackground.gameObject.SetActive(false);
        totalPoints.gameObject.SetActive(false);

        //Set up points display
        totalPoints.text = "Points: " + upPoints;
    }

    private void Update()
    {
        //once the player leader units are all destroyed, issue a signal for defeat
        if(leaders <= 0)
        {
            //unlock the cursor before moving on to the next scene
            Cursor.lockState = CursorLockMode.None;
            //load the defeat scene
            SceneManager.LoadScene("Defeat");
        }
    }

    //Method to have lead units report their existence and death
    public void rollcall(bool lead){
        //if the leader exists increase the pool of leaders before defeat
        if(lead == true)
        {
            leaders++;
        } //if the leader is now dead decrease the pool of leaders
        else if (lead == false)
        {
            leaders--;
        }
    }

    //Method to get and display points following an enemy's destruction
    public void earnPoints()
    {
        upPoints++;
        totalPoints.text = "Points: " + upPoints;
    }

    //Button methods
    //upgrade minion damage
    public void upMinDmg()
    {
        //if the player has enough points start the upgrade process
        if(upPoints >= 1)
        {
            //find all minions 
            allMinions = GameObject.FindGameObjectsWithTag("Player Minion");

            //go through each minion and tell them to upgrade
            foreach(GameObject minionUnit in allMinions)
            {
                
                minionUnit.GetComponent<MinionController>().incDmg();
                
            }
           
            //decrease the pool of upgrade points and redisplay the new number of points
            upPoints--;
            totalPoints.text = "Points: " + upPoints;
        }
    }

    //upgrade leader damage
    public void upLeadDmg()
    {
        //if the player has enough points start the upgrade process
        if (upPoints >= 1)
        {
            //find all leaders
            allLeaders = GameObject.FindGameObjectsWithTag("Player Unit");

            //go through each leader and tell them to upgrade
            foreach (GameObject knightUnit in allLeaders)
            {

                knightUnit.GetComponent<LeadUnitController>().increaseDamage();

            }

            //decrease the pool of upgrade points and redisplay the new number of points
            upPoints--;
            totalPoints.text = "Points: " + upPoints;
        }
    }

    //upgrade minion health
    public void upMinHp()
    {
        //if the player has enough points start the upgrade process
        if (upPoints >= 1)
        {
            //find all minions
            allMinions = GameObject.FindGameObjectsWithTag("Player Minion");

            //go through each minion and tell them to upgrade
            foreach (GameObject minionUnit in allMinions)
            {

                minionUnit.GetComponent<MinionController>().incHp();

            }

            //decrease the pool of upgrade points and redisplay the new number of points
            upPoints--;
            totalPoints.text = "Points: " + upPoints;
        }
    }

    //upgrade leader health
    public void upLeadHp()
    {
        //if the player has enough points start the upgrade process
        if (upPoints >= 1)
        {
            //find all leaders
            allLeaders = GameObject.FindGameObjectsWithTag("Player Unit");

            //go through each leader and tell them to upgrade
            foreach (GameObject knightUnit in allLeaders)
            {

                knightUnit.GetComponent<LeadUnitController>().increaseHealth();

            }

            //decrease the pool of upgrade points and redisplay the new number of points
            upPoints--;
            totalPoints.text = "Points: " + upPoints;
        }
    }

    //show or hide the upgrade menu
    public void bringUpUpgrades(bool yes)
    {
        if (yes == true)
        {
            //show the buttons
            btnMinDmg.gameObject.SetActive(true);
            btnLeadDmg.gameObject.SetActive(true);
            btnMinHealth.gameObject.SetActive(true);
            btnLeadHealth.gameObject.SetActive(true);

            //show the purple background and points
            purpleBackground.gameObject.SetActive(true);
            totalPoints.gameObject.SetActive(true);

        } else if (yes == false)
        {
            //hide the buttons
            btnMinDmg.gameObject.SetActive(false);
            btnLeadDmg.gameObject.SetActive(false);
            btnMinHealth.gameObject.SetActive(false);
            btnLeadHealth.gameObject.SetActive(false);

            //hide the purple background and points
            purpleBackground.gameObject.SetActive(false);
            totalPoints.gameObject.SetActive(false);
        }
    }

    //This resets the current level
    public void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //quit the game
    public void Quit()
    {
        Application.Quit();
    }
}
