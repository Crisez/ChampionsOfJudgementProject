using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DefeatScript : MonoBehaviour {

    //buttons
    [SerializeField]
    private Button btnMenu;
    [SerializeField]
    private Button btnQuit;

    void Start()
    {
        //button listeners
        btnMenu.onClick.AddListener(ReturnToMenu);
        btnQuit.onClick.AddListener(QuitGame);
    }

    //return to the menu
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    //quit the game
    public void QuitGame()
    {
        Application.Quit();
    }
}
