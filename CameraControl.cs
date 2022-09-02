using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour {

    //movement variables
    private float moveY;
    private float moveX;
    private float spd = 50;

    //original color of the unit
    private Color32 origColor = new Color32(94, 30, 63, 255);

    //holds the selected unit
    private GameObject selected;

    //used for moving the camera
    private Vector3 cameraMove;

    //reference the gamemanager
    private GameManager appControl;

    //raycasts used to select and set destination
    private RaycastHit select;
    private RaycastHit dest;

    void Start () {

        //get the game manager
        appControl = GameObject.Find("GameManager").GetComponent<GameManager>();
        
        //lock the cursor in the middle of the screen
        Cursor.lockState = CursorLockMode.Locked;
                
    }
	
	void FixedUpdate () {

        moveY = Input.GetAxis("Mouse Y");
        moveX = Input.GetAxis("Mouse X");

        //gets the mouse movement and holds it in a vector to apply to the transform
        cameraMove = transform.forward * moveY + transform.right * moveX;

        //takes the current transform and applies the movement and speed
        transform.position += cameraMove * Time.deltaTime * spd;

        if (Input.GetKeyDown(KeyCode.P))
        {
            //if the cursor is unlocked then lock it, otherwise lock the cursor
            if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
                appControl.bringUpUpgrades(false);
            }
            else if(Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                appControl.bringUpUpgrades(true);
            }
            
        }

        //left click to select a unit
        if (Input.GetMouseButtonDown(0))
        {
            select = new RaycastHit();
            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out select) && select.transform.gameObject.tag == "Player Unit")
            {
                Debug.DrawRay(select.point, Vector3.down, Color.red, 1f, false);

                //if the unit selected is not the current unit change the color back to the original for the current unit
                if (selected != null && select.transform.gameObject != selected)
                {
                    selected.GetComponent<Renderer>().material.color = origColor;
                }

                //store the selected gameobject
                selected = select.transform.gameObject;
                
                //change color to reflect selection
                selected.GetComponent<Renderer>().material.color = new Color(1, 0, 1, 1); //94,30,63
            }
            else
            {
                //deselect the unit
                selected.GetComponent<Renderer>().material.color = origColor;
                selected = null;
            }

        }
        //right click to choose a new position
        if (Input.GetMouseButtonDown(1) && selected != null)
        {
            
            dest = new RaycastHit();
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out dest) && dest.transform.gameObject.tag == "Floor")
            {
                //move to the target position
                Debug.DrawRay(dest.point, Vector3.down, Color.cyan, 1f, false);
                selected.GetComponent<LeadUnitController>().targetPosition(dest.point);
            }
            
        }


        //press r to tell the gamemanager to reset the level
        if (Input.GetKeyDown(KeyCode.R))
        {
            appControl.ResetLevel();
        }
        
    }
}
