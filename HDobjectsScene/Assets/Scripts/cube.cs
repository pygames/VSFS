using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cube : MonoBehaviour {
    public float moveSpeed;
    private Rigidbody rbody;
	// Use this for initialization
	void Start () {
        moveSpeed = 5f;
        rbody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        transform.Translate(moveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime, moveSpeed * Input.GetAxis("Jump") * Time.deltaTime, moveSpeed * Input.GetAxis("Vertical") * Time.deltaTime);


    }
}
