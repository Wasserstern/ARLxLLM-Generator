using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{

    public float springForce;
    Transform forceDirector;
    void Start()
    {
        forceDirector = transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other){
        if(other.gameObject.layer == LayerMask.NameToLayer("Ball")){
            Debug.Log("Should spring.");
            Vector3 forceDirection = (forceDirector.transform.position - transform.position).normalized;
            Rigidbody ballRgbd = other.gameObject.GetComponent<Rigidbody>();
            ballRgbd.velocity = Vector3.zero;
            ballRgbd.AddForce(forceDirection * springForce, ForceMode.Impulse);
            
        }
    }
}
