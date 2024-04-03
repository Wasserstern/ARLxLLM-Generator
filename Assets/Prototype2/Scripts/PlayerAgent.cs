using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using Unity.Barracuda;
using Unity.Jobs;
using Google.Protobuf.Reflection;

public class PlayerAgent : Agent
{
    public float maxVelocity;
    public float acceleration;
    public float deceleration;
    public float turnSpeed;
    public float jumpForce;
    public float gravityEnhance;
    public float groundCheckDistance;
    Rigidbody rgbd;
    Transform lookDirector;
    public Transform goalTransform;
    public Transform startTransform;
    public Transform observations;
    public Material rewardMaterial;
    public Material penaltyMaterial;
    Vector3 nextVelocity;
    float maxDistance;
    GameObject currentGroundObject;
    Quaternion originalRotation;
    public override void Initialize()
    {
        rgbd = GetComponent<Rigidbody>();
        lookDirector = transform.GetChild(0);
        nextVelocity = rgbd.velocity;
        originalRotation = transform.rotation;
    }
    public override void OnEpisodeBegin()
    {
        maxDistance = Vector3.Distance(startTransform.position, goalTransform.position);
        transform.position = startTransform.position;
        rgbd.velocity = Vector3.zero;
        nextVelocity = Vector3.zero;
        currentGroundObject = null;
        transform.rotation = originalRotation;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        for(int i = 0; i < observations.childCount; i++){
            sensor.AddObservation(observations.GetChild(i).localPosition);
        }
        sensor.AddObservation(startTransform.localPosition);
        sensor.AddObservation(goalTransform.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        bool isPressingJump = actions.DiscreteActions[0] == 1;
        switch(actions.DiscreteActions[1]){
            case 0: {
                transform.Rotate(new Vector3(0f, turnSpeed * Time.deltaTime, 0f));
                break;
            }
            case 1:{
                transform.Rotate(new Vector3(0f, -turnSpeed * Time.deltaTime, 0f));
                break;
            }
            case 2:{
                // dont rotate
                break;
            }
        }
        Vector3 lookDirection = (lookDirector.position - transform.position).normalized;
        switch(actions.DiscreteActions[2]){
            case 0:{
                nextVelocity = rgbd.velocity += lookDirection * acceleration * Time.deltaTime;

                break;
            }
            case 1:{
                nextVelocity = rgbd.velocity -= lookDirection * acceleration * Time.deltaTime;

                break;
            }
            case 2:{
                break;
            }
        }
        Vector3 rayDirection = (new Vector3(transform.position.x, transform.position.y -1f, transform.position.z) - transform.position).normalized;
        RaycastHit hit;
        if(Physics.Raycast(transform.position, rayDirection,out hit, groundCheckDistance, LayerMask.GetMask("Ground"))){
            // is grounded
            if(isPressingJump){
                rgbd.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            }
            else{
                rgbd.velocity = new Vector3(rgbd.velocity.x, 0f, rgbd.velocity.z);
            }
            
        }
        else{
            rgbd.AddForce(-transform.up * gravityEnhance * Time.deltaTime);
        }
        Debug.Log(maxDistance - Vector3.Distance(transform.position, goalTransform.position));
        SetReward(maxDistance - Vector3.Distance(transform.position, goalTransform.position));
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        switch((int)Input.GetAxisRaw("Horizontal")){
            case -1:{
                discreteActions[1] = 1;
                break;
            }
            case 1:{
                discreteActions[1] = 0;
                break;
            }
            case 0:{
                discreteActions[1] = 2;
                break;
            }
        }
        switch((int)Input.GetAxisRaw("Vertical")){
            case -1:{
                discreteActions[2] = 1;
                break;
            }
            case 1:{
                discreteActions[2] = 0;
                break;
            }
            case 0:{
                discreteActions[2] = 2;
                break;
            }
        }
        Debug.Log("IsPressingJump: " + discreteActions[0].ToString());
        Debug.Log("Turn: " + discreteActions[1].ToString());
        Debug.Log("Move: "  + discreteActions[2].ToString());
    }
    private void Update(){

    }
    private void OnCollisionEnter(Collision other){
        if(other.gameObject.tag == "Reward"){
            other.gameObject.GetComponent<MeshRenderer>().material = rewardMaterial;
        }
    }

    private void OnTriggerEnter(Collider other){
        if(other.gameObject.layer == LayerMask.NameToLayer("Penalty")){
            AddReward(-2f);
            for(int i = 0; i < observations.childCount; i++){
                observations.GetChild(i).GetComponent<MeshRenderer>().material = penaltyMaterial;
            }
            EndEpisode();
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Goal")){
            SetReward(maxDistance + 1f);
            EndEpisode();
        }
    }
}
