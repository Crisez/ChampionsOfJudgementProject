using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;
using System;

public class ChampionMenuScript : MonoBehaviour {

    //buttons
    [SerializeField]
    private Button btnNew;
    [SerializeField]
    private Button btnLoad;
    [SerializeField]
    private Button btnQuit;

    //dropdown menu
    [SerializeField]
    private Dropdown load;

    //if the dropdown menu is loaded with savefiles
    private bool loaded = false;

    //lists to hold the data read in from the database
    private List<string> saveFiles;
    private List<string> levelList;

    //the savefile to load and its associated index
    private string saveFileToLoad;
    private int saveIndex;

    //the path to the database
    private string path;

    //connection to database
    private IDbConnection sqlconnect;

    //command to be given to the database
    private IDbCommand giveCommand;

    //reads from the database
    private IDataReader databaseRead;

    //selects all fields from the database
    private string selectQuery;

    void Start()
    {
        //button listeners
        btnNew.onClick.AddListener(NewGame);
        btnLoad.onClick.AddListener(LoadProgress);
        btnQuit.onClick.AddListener(QuitGame);

        //hide the dropdown
        load.gameObject.SetActive(false);

        //set up the path to the database, the connection, and the needed query
        path = "URI=file:" + Application.dataPath + "/ChampDatabase.db";
        sqlconnect = new SqliteConnection(path);
        selectQuery = "SELECT * FROM PLAYERDATA";
    }

    //loads the first level
    public void NewGame()
    {
        SceneManager.LoadScene("LevelOne");
    }

    //load a saved game
    public void LoadProgress()
    {
        //if the dropdown has not been loaded
        if(loaded == false)
        {
            saveFiles = new List<string>();
            levelList = new List<string>();

            sqlconnect.Open();

            //prep for new command
            giveCommand = sqlconnect.CreateCommand();

            //give command
            giveCommand.CommandText = selectQuery;

            databaseRead = giveCommand.ExecuteReader();

            //read the fields in and add the savefiles and levels to the lists
            while (databaseRead.Read())
            {

                saveFiles.Add(databaseRead[1].ToString());
                levelList.Add(databaseRead[2].ToString());
                
            }

            databaseRead.Close();

            giveCommand.Dispose();

            sqlconnect.Close();

            //clear previous options on the dropdown
            load.options.Clear();

            //add the savefiles as options
            load.AddOptions(saveFiles);

            //show the dropdown
            load.gameObject.SetActive(true);

            loaded = true;

        } else if(loaded == true) 
        {
            //if the dropdown is loaded get the selected savefile
            saveFileToLoad = load.options[load.value].text;
            Debug.Log("SaveFile: " + saveFileToLoad);

            //find the index of the savefile
            saveIndex = saveFiles.IndexOf(saveFileToLoad);
            Debug.Log("Index: " + saveIndex);
            Debug.Log("Level: " + levelList[saveIndex]);

            //apply this index to the level list and start the correct level
            SceneManager.LoadScene(int.Parse(levelList[saveIndex]));
        }
        
    }

    //quit the game
    public void QuitGame()
    {
        Application.Quit();
    }
}
