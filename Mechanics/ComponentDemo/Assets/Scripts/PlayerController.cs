using UnityEngine;
using UnityEngine.AI;
// Include the namespace required to use Unity UI
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class PlayerController : MonoBehaviour
{

    public float GrabRange = Mathf.Infinity;

    // Create public variables for player speed, and for the Text UI game objects
    public float speed;
    public Text holdText;
    public Text winText;

    // Create private references to the rigidbody component on the player, and the count of pick up objects picked up so far
    private Rigidbody rb;
    private int count;

    public Camera cam;

    public NavMeshAgent agent;

    public GameObject goal;
    //public GameObject loc;
    public Vector3 loc;

    void TakeRelease()
    {

        //if no children, take. Otherwise release.
        if(transform.GetComponentsInChildren<Transform>()[1].childCount == 0)
        {

            print("Grabbing...");

            Transform tMin = null;
            float minDist = GrabRange;

            //Agent's position and list of resources
            Vector3 currentPos = transform.position;

            //This is currently not working because the function returns the children and their parent, which I need to remove from the list. 
            //I think this solves this but I can't check until later
            Transform[] resourceArray = transform.parent.parent.Find("Resources").Find("Mobile").GetComponentsInChildren<Transform>();
            //foreach(Transform t in resourceArray)
            for(int i  =  1; i < resourceArray.Length; i++)
            {

                Transform t = resourceArray[i];

                float dist = Vector3.Distance(t.position, currentPos);
                if (dist < minDist)
                {
                    tMin = t;
                    minDist = dist;
                }

            }

            //the code to reassign the parent, with an if guard for in case there was no object within range.
            if (tMin)
            {

                tMin.GetComponent<Rigidbody>().useGravity = false;
                float xHold = tMin.transform.position.x;
                float zHold = tMin.transform.position.z;
                tMin.parent = transform.GetComponentsInChildren<Transform>()[1];
                tMin.GetComponent<KResource>().held = true;

            }

            

        }
        //...otherwise release
        else
        {

            print("Letting go...");

            //references an object in hand
            Transform inHand = transform.GetComponentsInChildren<Transform>()[1].GetComponentsInChildren<Transform>()[1];

            //restores gravity and moves object to mobile objects collection.
            inHand.GetComponent<Rigidbody>().useGravity = true;
            inHand.parent = transform.parent.parent.Find("Resources").Find("Mobile");
            print(inHand);
            inHand.GetComponent<KResource>().held = false;

        }

        

    }

    // At the start of the game..
    void Start()
    {
        // Assign the Rigidbody component to our private rb variable
        rb = GetComponent<Rigidbody>();

        // Set the count to zero 
        count = 0;

        // Run the SetCountText function to update the UI (see below)
        SetCountText();

        // Set the text property of our Win Text UI to an empty string, making the 'You Win' (game over message) blank
        //winText.text = "";
    }

    // Before rendering each frame..
    void Update()
    {

        //Grabs/drops an object when the spacebar is hit.
        if (Input.GetKeyUp("space"))
            TakeRelease();

        //sends the cook to where was clicked.
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
                agent.SetDestination(hit.point);

        }

        //if goal
        //gather objects
        //else smoke


        if (Input.GetKeyUp("return"))
            Achieve(goal);

    }

    void Achieve(GameObject objective)
    {
        //if fixed ingredient
            //set fixed ingredient as location
        //else
            //location selector
                //arbitrary for now. Create invisible GO as default
        //order things to gather. I'm not going to build this yet
        //for ingredients
            //if ingredient doesn't exists
                //gatherobjects(ingredient)
            //while(ingredient not in location)
            //goto ingredient
            //while(not holding ingredient)
                //grab ingredient
            //goto location
            //drop ingredient

        
        StartCoroutine(GetClose(objective));

        

    }

    IEnumerator GetClose(GameObject objective)
    {

        GameObject[] weNeed;
        Transform[] temp = transform.parent.parent.Find("Resources").Find("Mobile").GetComponentsInChildren<Transform>();
        Transform[] weHave = new Transform[temp.Length - 1];
        for (int i = 1; i < temp.Length; i++)
        {
            print("we have " + temp[i]);
            weHave[i - 1] = temp[i];
        }
        weNeed = objective.GetComponent<KResource>().prereqs;

        bool gathered = false;
        bool ingredientMissing = true;
        bool locSet = false;

        //I wanna make this into a while controlled by gathered which will check if there are any ingredients outside the location
        while (!gathered && ingredientMissing)
        {

            print("starting while");

            for (int i = 0; i < weNeed.Length; i++)
            {

                print("starting for");

                Transform goHere = null;
                ingredientMissing = true;
                gathered = true;

                //cycles through eveything in the kitchen to compare to the current item in question
                //if item matches thing we need, sets that item to be used as destination
                foreach (Transform t in weHave)
                {

                    print("we need " + weNeed[i].transform);
                    print("...and we have " + t);
                    print(Vector3.Distance(t.position, gameObject.GetComponent<PlayerController>().loc));
                    if (weNeed[i].transform.name == t.name)
                    {

                        ingredientMissing = false;
                        if (Vector3.Distance(t.position, gameObject.GetComponent<PlayerController>().loc) > GrabRange)
                        {

                            print("Gohere set to " + t);
                            goHere = t;

                            if (!locSet)
                                gameObject.GetComponent<PlayerController>().loc = goHere.position;
                            locSet = true;
                            gathered = false;

                        }
                    }
                }

                //if the object is in the kitchen, go to it, get it, and bring it to prep area.
                if (goHere)
                {

                    print("Getting " + goHere + " Get within " + GrabRange);
                    agent.SetDestination(goHere.position);

                    print("yield 1 " + Vector3.Distance(goHere.position, transform.position));
                    yield return new WaitUntil(() => Vector3.Distance(goHere.position, transform.position) < GrabRange);

                    print("while 1");
                    while (transform.GetComponentsInChildren<Transform>()[1].childCount == 0)
                        TakeRelease();

                    print("going to loc");
                    agent.SetDestination(gameObject.GetComponent<PlayerController>().loc);

                    print("yield 2 " + Vector3.Distance(gameObject.GetComponent<PlayerController>().loc, transform.position));
                    yield return new WaitUntil(() => Vector3.Distance(gameObject.GetComponent<PlayerController>().loc, transform.position) < GrabRange / 2);

                    print("while 2");
                    while (transform.GetComponentsInChildren<Transform>()[1].childCount != 0)
                        TakeRelease();

                }
                if(ingredientMissing)
                {

                    print("Must make " + weNeed[i]);
                    Achieve(weNeed[i]);

                }

            }

            yield return null;

        }
        
        print("giggity");

        Transform tMin = null;
        float minDist = float.PositiveInfinity;
        Vector3 currentPos = gameObject.GetComponent<PlayerController>().loc;
        Transform[] resourceArray = transform.parent.parent.Find("Resources").Find("Mobile").GetComponentsInChildren<Transform>();
        //foreach(Transform t in resourceArray)
        for (int i = 1; i < weNeed.Length; i++)
        {
            
            foreach (Transform t in weHave)
            {

                if (weNeed[i].transform.name == t.name)
                {

                    float dist = Vector3.Distance(t.position, currentPos);
                    if (dist < minDist)
                    {
                        tMin = t;
                        minDist = dist;
                    }
                }
            }
            
            //Destroy(tMin.gameObject);

        }
        
        GameObject justMade = Instantiate(objective, currentPos, Quaternion.Euler(0, 0, 0));
        justMade.transform.parent = transform.parent.parent.Find("Resources").Find("Mobile");

    }

    // Each physics step..
    void FixedUpdate()
    {
        // Set some local float variables equal to the value of our Horizontal and Vertical Inputs
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");


        // Create a Vector3 variable, and assign X and Z to feature our horizontal and vertical float variables above
        Vector3 movement = transform.forward * moveVertical;//new Vector3(0.0f, 0.0f, moveVertical);

        // Add a physical force to our Player rigidbody using our 'movement' Vector3 above, 
        // multiplying it by 'speed' - our public player speed that appears in the inspector
        rb.AddForce(movement * speed);
        transform.Rotate(0.0f, -moveHorizontal * speed / 9, 0.0f);

        //rb.velocity += transform.forward * speed;
    }

    // When this game object intersects a collider with 'is trigger' checked, 
    // store a reference to that collider in a variable named 'other'..
    void OnTriggerEnter(Collider other)
    {
        // ..and if the game object we intersect has the tag 'Pick Up' assigned to it..
        if (other.gameObject.CompareTag("Pick Up"))
        {
            // Make the other game object (the pick up) inactive, to make it disappear
            other.gameObject.SetActive(false);

            // Add one to the score variable 'count'
            count = count + 1;

            // Run the 'SetCountText()' function (see below)
            SetCountText();
        }
    }

    // Create a standalone function that can update the 'countText' UI and check if the required amount to win has been achieved
    void SetCountText()
    {
        // Update the text field of our 'countText' variable
        

        // Check if our 'count' is equal to or exceeded 12
        if (count >= 12)
        {
            // Set the text value of our 'winText'
            winText.text = "You Win!";
        }

        //holdText.text = "Holding: " + tMin.ToString();
    }
}
