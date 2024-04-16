using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class BallAgent : Agent
{   
    // References
    Rigidbody rgbd;
    Collider col;
    // Settings


    // Bounce settings
    public float standardBounce;
    public float minBounce;
    public float maxBounce;
    public float bounceIncrease;
    public float bounceDecreaseStandard;
    public float bounceDecreaseFast;
    // Movement settings
    public float moveAcceleration;
    public float moveDecceleration;
    public float maxMoveSpeed;
    [Range(0f, 1f)]
    public float deadZoneMagnitude;
    public float minVelocityMagnitude;

    // Runtime stuff
    [SerializeField]
    float currentBounce;
    Vector2 inputDirection;
    bool isPressingBounce;
    bool isPressingStiff;
    public override void Initialize()
    {
        rgbd = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        currentBounce = standardBounce;
    }
    public override void OnEpisodeBegin()
    {
        
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        inputDirection = new Vector2(actions.ContinuousActions[0], actions.ContinuousActions[1]);

        isPressingBounce = actions.DiscreteActions[0] == 1;
        isPressingStiff = actions.DiscreteActions[1] == 1;
        float nextBounce = currentBounce;
        if(isPressingBounce){
            nextBounce += bounceIncrease + Time.deltaTime;
            if(nextBounce > maxBounce){
                nextBounce = maxBounce;
            }
        }
        else if(isPressingStiff){
            nextBounce -= bounceDecreaseFast * Time.deltaTime;
            if(nextBounce < minBounce){
                nextBounce = minBounce;
            }
        }
        else{
            nextBounce -= bounceDecreaseStandard * Time.deltaTime;
             if(nextBounce < minBounce){
                nextBounce = minBounce;
            }
        }
        currentBounce = nextBounce;
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");

        discreteActions[0] = Convert.ToInt32(Input.GetKey(KeyCode.J));
        discreteActions[1] = Convert.ToInt32(Input.GetKey(KeyCode.K));
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void FixedUpdate(){
        if(inputDirection.magnitude > deadZoneMagnitude){
            // Apply input
            rgbd.AddForce(inputDirection * moveAcceleration, ForceMode.Force);
        }
        else{
            //TODO: Slow down ball via force, not directly
        }

        if(rgbd.velocity.magnitude > maxMoveSpeed){
            // Slow down ball if its velocity exceeds the allowed maximum
            float magnitudeDifference = rgbd.velocity.magnitude - maxMoveSpeed;
            Vector2 oppositeDirection = -rgbd.velocity.normalized;
            rgbd.AddForce(oppositeDirection * magnitudeDifference, ForceMode.Force);
        }
    }

    private void OnCollisionEnter(Collision other){
        if(other.gameObject.layer == LayerMask.NameToLayer("Ground")){
            Vector2 bounceDirection = other.contacts[0].normal;
            rgbd.AddForce(bounceDirection * currentBounce, ForceMode.Impulse);
        }
    }
}
