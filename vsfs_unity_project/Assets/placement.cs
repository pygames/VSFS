using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    DEBUG TIPS:
    -PROBLEM:    is an object not picking up when the player tries to grab it from the counter?
    -SOLUTION 1: there are two likely problems to address.  1) make sure the item is parented to the correct
                 tile box.  2) make sure the tile box's corresponding is_location_full[] bool value is set to
                 the correct value when the game begins (true for holding, false for empty).  Search "SC738465"
                 to find location in code to set these initial values.
    -SOLUTION 2: make sure that the item is always the first child (of player if held.  of tile box if on counter) 
                 as the code changes the parenting of the object at index 0 of the child list.  Search "SC857254" 
                 to find the location in the code where this code is used
    -PROBLEM:    I added more tile boxes, but I get an IndexOutOfRangeException when I try to access the new tile boxes.
    -SOLUTION:   Make sure the size of is_location_full[] is set to the total number of tiles boxes (number of tiles 
                 boxes plus one if tile boxes are 1-indexed instead of 0-indexed like arrays).  Note: current scene in dev
                 uses 1-indexed tile boxes.


    NOTES:
    -Objects that can be picked up/put down must be placed
    on counter or in the player's hand at start of game
    -bools must be properly set to true or false depending 
    on if a particular tile contains an object (true) or 
    not (false)
    -object is currently parented to player cube when picked up.
    this can be changed to the player's hand by going down the player
    cube's hierarchy to find the hand
*/

public class placement : MonoBehaviour
{

    public float speed;

    //how far away the player must be to pick up/place an object on the counter
    public float range;

    private int once = 1;

    public GameObject carriable_object;
    public GameObject target;
    public GameObject player;
    private bool[] is_location_full;
    private bool is_holding;

    // Use this for initialization
    void Start()
    {
        speed = 5.0f;
        range = 1.5f;
        
    }

    // Update is called once per frame
    void Update()
    {
        //initializes values of bool array.  only runs onces.
        //was not working in Start() function so it was put here.
        //checking this if statement is not a significant use
        //of system resources, especially in the context of this
        //game's overall demand on target devices.
        if (once == 1) {
            once++;
            is_holding = false;
            is_location_full = new bool[39];
            for (int i = 0; i < 39; i++)
            {
                is_location_full[i] = false;
            }
            //SEARCH CODE: SC738465
            //locations that hold objects at the start of the game should be set to true here
            is_location_full[20] = true;
            is_location_full[21] = true;
            is_location_full[22] = true;
        }

        //movement
        float moveVertical = Input.GetAxis("Vertical") * Time.deltaTime;

        Vector3 movement = new Vector3(0.0f, 0.0f, moveVertical);

        transform.Translate(movement * speed, Space.Self);
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Rotate(-Vector3.up * 16 * speed * Time.deltaTime);
        }
        else if(Input.GetKey(KeyCode.RightArrow))
        {
            transform.Rotate(Vector3.up * 16 * speed * Time.deltaTime);
        }

        //carrying handling
        if (Input.GetKeyDown("space"))
        {
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, range))
            {
                int result;
                //check if hit's name contains a number, stores number in result
                //note: objects should only be given a number in their name anywhere if they are tiles
                if (int.TryParse(hit.collider.gameObject.name, out result))
                {
                    //is user holding item and is location empty
                    Debug.Log(result);
                    if (is_holding && !is_location_full[result])
                    {
                        Debug.Log("hello there");

                        //update array and holding bool
                        is_holding = false;
                        is_location_full[result] = true;

                        //swap parenting
                        //SEARCH CODE: SC857254
                        GameObject temp = player.gameObject.transform.GetChild(0).gameObject;
                        temp.transform.SetParent(hit.collider.gameObject.transform);
                        //place object
                        temp.transform.position = hit.collider.gameObject.transform.position;
                    }
                    //is user not holding and object and is there an object at location
                    else if (!is_holding && is_location_full[result])
                    {
                        Debug.Log("General Kenobi");

                        //update array and holding bool
                        is_holding = true;
                        is_location_full[result] = false;

                        //swap parenting
                        //SEARCH CODE: SC857254
                        GameObject temp = hit.collider.gameObject.transform.GetChild(0).gameObject;
                        temp.transform.SetParent(player.transform);
                    }


                    Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                    Debug.Log("Did Hit");

                }
            
                
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white);
                Debug.Log("Did not Hit");
            }
        }

    }
}