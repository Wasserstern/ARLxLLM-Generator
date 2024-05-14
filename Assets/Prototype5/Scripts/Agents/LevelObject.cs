using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObject : MonoBehaviour 
{
    public LevelAgent parentLevelAgent;
    public bool isTarget = true;

    private void OnTriggerEnter(Collider other){
        if(isTarget && other.gameObject.layer == LayerMask.NameToLayer("Ball")){
            parentLevelAgent.OnTargetTriggerEnter(other);
        }
    }
    private void OnCollisionEnter(Collision collision){
        if(isTarget && collision.gameObject.layer == LayerMask.NameToLayer("Ball")){
            parentLevelAgent.OnTargetCollisionEnter(collision);
        }
    }
}