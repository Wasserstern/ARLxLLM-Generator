using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Marble : MonoBehaviour
{
    public BoardTiltAgent boardTiltAgent;
    public float yVelocity;
    Rigidbody rgbd;
    void Start()
    {
        rgbd = GetComponent<Rigidbody>();   
    }

    // Update is called once per frame
    void Update()
    {
        rgbd.AddForce(new Vector3(0f, yVelocity * Time.deltaTime, 0f));
    }
    private void OnTriggerEnter(Collider other){
        boardTiltAgent.MarbleTriggered(other);
    }
    private void OnCollisionEnter(Collision other){
        boardTiltAgent.MarbleCollided(other);
    }
}
