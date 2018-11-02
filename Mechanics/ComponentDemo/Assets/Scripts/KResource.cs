using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KResource : MonoBehaviour {

    public GameObject[] prereqs;
    public int activeTime;
    public int passiveTime;

    public bool held;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        if (held)
        {

            //transform.position = transform.parent.forward;
            transform.position = transform.parent.position + transform.parent.forward * 1;

        }
		
	}
}
