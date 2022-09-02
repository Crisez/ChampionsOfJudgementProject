using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BaseScript : MonoBehaviour {

    //Health
    private int baseHealth;

   
    void Start () {
        baseHealth = 30;
	}
	
    //script for the base to take damage and be destroyed
    public void besieged(int incomingDmg)
    {
        baseHealth -= incomingDmg;
        if (baseHealth <= 0)
        {
            //unlock the cursor before going to the next scene
            Cursor.lockState = CursorLockMode.None;
            //go to the victory scene and destroy this object
            SceneManager.LoadScene("Victory");
            Destroy(this.gameObject);
        }
    }
}
