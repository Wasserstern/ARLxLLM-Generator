using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Marble : MonoBehaviour
{
    public BoardTiltAgent boardTiltAgent;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other){
        boardTiltAgent.MarbleTriggered(other);
    }
    private void OnCollisionEnter(Collision other){
        boardTiltAgent.MarbleCollided(other);
    }
}
