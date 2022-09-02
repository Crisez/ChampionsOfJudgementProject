using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using System;

public class VictoryScript : MonoBehaviour {

    //buttons
    [SerializeField]
    private Button btnContinue;
    [SerializeField]
    private Button btnSave;
    [SerializeField]
    private Button btnQuit;

    //the path to the database
    private string path;

    //ensure that the user id is unique
    private bool unique;

    //ensure that once it is confirmed to not be unique that the user id is not considered unique
    private bool confirmedUnique;

    //list to hold all user ids
    private List<string> users;

    //database connection
    private IDbConnection sqlconnect;

    //commands to be given to the database
    private IDbCommand giveInsertCommand;
    private IDbCommand giveSelectCommand;

    //used to read the database
    private IDataReader databaseRead;

    //the queries to be run
    private string selectQuery;
    private string insertQuery;

    //holds the userID and the highest save number
    private int userID;
    private int saveNum = 0;

    void Start() {

        //button listeners
        btnContinue.onClick.AddListener(NextLevel);
        btnSave.onClick.AddListener(SaveProgress);
        btnQuit.onClick.AddListener(QuitGame);

        //set unique to false so we can enter the while loop
        unique = false;

        //set up the path, connection, and selection query
        path = "URI=file:" + Application.dataPath + "/ChampDatabase.db";
        sqlconnect = new SqliteConnection(path);
        selectQuery = "SELECT * FROM PLAYERDATA";
       
    }

    //load the nextlevel
    public void NextLevel()
    {
        //Below is the code to load the menu. This would be expanded upon to actually load other levels if they existed
        SceneManager.LoadScene(0);
        
    }

    //save the progress to a save file
    public void SaveProgress()
    {
        //ensure unique is false everytime this is called
        unique = false;
        users = new List<string>();

        sqlconnect.Open();

        //prepare for a command
        giveSelectCommand = sqlconnect.CreateCommand();
        
        //give the select command
        giveSelectCommand.CommandText = selectQuery;


        databaseRead = giveSelectCommand.ExecuteReader();

        //read all the fields in the database so we can store the user's ids in a list so the new id can be unique, and so we can get the save with the highest savefile number
        while (databaseRead.Read())
        {
            users.Add(databaseRead[0].ToString());
            
            //check for the highest save number
            if (saveNum < int.Parse(databaseRead[1].ToString()))
            {
                saveNum = int.Parse(databaseRead[1].ToString());
            }
            
        }

        databaseRead.Close();

        giveSelectCommand.Dispose();

        //Insert new information

        //increase to next savefile number
        saveNum++;

        //prep for new command
        giveInsertCommand = sqlconnect.CreateCommand();

        //get a random unique user id
        randomUser();

        //insert the id, savefile number, and level into the database
        insertQuery = "INSERT INTO PLAYERDATA (PLAYER_ID, PLAYER_SAVEFILE, PLAYER_LEVEL) VALUES (" + userID + ", " + saveNum + ", " + SceneManager.GetActiveScene().buildIndex + ")";

        //give command
        giveInsertCommand.CommandText = insertQuery;

        giveInsertCommand.ExecuteNonQuery();

        giveInsertCommand.Dispose();

        sqlconnect.Close();

        //Confirm save
        btnSave.GetComponentInChildren<Text>().text = "Saved to SaveFile " + saveNum;

        //reset saveNum
        saveNum = 0;

    }

    //quit the game
    public void QuitGame()
    {
        Application.Quit();
    }

    //make a random unique user id
    private void randomUser(){

        while (unique == false)
        {
            //make a new number with the intention that it is unique
            userID = UnityEngine.Random.Range(1000, 5000);
            confirmedUnique = true;

            foreach (String id in users)
            {
                if (userID == int.Parse(id))
                {
                    unique = false;
                    //prevent unique from becoming true through the rest of this for loop
                    confirmedUnique = false;
                    Debug.Log("It is no longer unique");
                } // if it is not the same and we do not know of any that match it, make unique true
                else if(confirmedUnique == true && userID != int.Parse(id))
                {
                    unique = true;
                    Debug.Log("It is unique right now");
                }
            }
        }

    }
}
