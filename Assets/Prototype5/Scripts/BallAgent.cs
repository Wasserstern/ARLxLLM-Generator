using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class BallAgent : Agent
{   
    // References
    public Transform cameraTransform;
    public TowerGenerator towerGenerator;
    Rigidbody rgbd;
    Collider col;
    
    // Settings
    public float camMoveSpeed;
    public float camTargetMaxDistance;

    // Bounce settings
    public float standardBounce;
    public float minBounce;
    public float maxBounce;
    public float bounceIncrease;
    public float bounceDecreaseStandard;
    public float bounceDecreaseFast;
    // Movement settings
    public float moveAcceleration;
    public float moveDeceleration;
    public float moveDecelerationFast;
    public float maxMoveSpeed;
    [Range(0f, 1f)]
    public float deadZoneMagnitude;
    public float minVelocityMagnitude;
    // ML Agent settings
    public float observationRadius;
    public int maxObservations;

    // Runtime variables
    [SerializeField]
    float currentBounce;
    Vector3 inputDirection;
    bool isPressingBounce;
    bool isPressingStiff;
    Vector3 cameraTargetPosition;
    Vector3 camOffsetPosition;
    Vector3 camVelocity;

    // AI variables
    Vector3 startPositionLocal;
    float initalGoalDistance;
    public override void Initialize()
    {
        rgbd = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        currentBounce = standardBounce;
        startPositionLocal = transform.localPosition;
        
    }
    public override void OnEpisodeBegin()
    {
        
        transform.localPosition = startPositionLocal;
        rgbd.velocity = Vector3.zero;
        currentBounce = standardBounce;
        isPressingBounce = false;
        isPressingStiff = false;
        inputDirection = Vector3.zero;
        towerGenerator.GenerateTower();
        initalGoalDistance = Vector3.Distance(towerGenerator.goalObject.transform.position, transform.position);
        
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        // Other
        sensor.AddObservation(currentBounce);
        sensor.AddObservation(transform.localPosition);

        // Add surroundings
        Collider[] obsverationColliders = Physics.OverlapSphere(transform.position, observationRadius, LayerMask.GetMask("Ground"));
        int observationIndex = 0;
        foreach(Collider oCol in obsverationColliders){
            if(oCol.gameObject.tag == "Platform"){
                Debug.Log("Found some platform");
                sensor.AddObservation(oCol.transform.localPosition);
                observationIndex++;
            }
            if(observationIndex >= maxObservations){
                break;
            }
        }
        for(int i = observationIndex; i < maxObservations; i++){
            sensor.AddObservation(Vector3.zero);
        }
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        inputDirection = new Vector3(actions.ContinuousActions[0],0f, actions.ContinuousActions[1]);
        isPressingBounce = actions.DiscreteActions[0] == 1;
        isPressingStiff = actions.DiscreteActions[1] == 1;
        float nextBounce = currentBounce;
        if(isPressingBounce){
            nextBounce += bounceIncrease * Time.deltaTime;
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
             if(nextBounce < standardBounce){
                nextBounce = standardBounce;
            }
        }
        currentBounce = nextBounce;
        
        float currentGoalDistance = Vector3.Distance(towerGenerator.goalObject.transform.position, transform.position);
        SetReward(initalGoalDistance / currentGoalDistance);
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
        cameraTargetPosition = cameraTransform.position;
        camOffsetPosition = cameraTransform.localPosition;
    }

    void Update()
    {
        
    }

    void FixedUpdate(){
        //inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        Vector3 velocityXZ = new Vector3(rgbd.velocity.x, 0f, rgbd.velocity.z);
        if(inputDirection.magnitude > deadZoneMagnitude){
            // Apply input
            rgbd.AddForce(inputDirection * moveAcceleration, ForceMode.Acceleration);
            /*
            Vector3 inputDirectionNormal = inputDirection.normalized;
            Vector3 velocityNormal = new Vector3(rgbd.velocity.x, 0f, rgbd.velocity.z).normalized;
            if(Vector3.Angle(inputDirectionNormal, velocityNormal) > 30f && rgbd.velocity.magnitude > minVelocityMagnitude){
                Vector3 oppositeDirection = new Vector3(-rgbd.velocity.x, 0f, -rgbd.velocity.z).normalized;
                rgbd.AddForce(oppositeDirection * moveDecelerationFast, ForceMode.Force);
            }
            */
        }
        else{
            if(velocityXZ.magnitude > minVelocityMagnitude){
                Vector3 oppositeDirection = new Vector3(-rgbd.velocity.x, 0f, -rgbd.velocity.z).normalized;
                rgbd.AddForce(oppositeDirection * moveDeceleration, ForceMode.Acceleration);
            }
        }

        if(velocityXZ.magnitude > maxMoveSpeed){
            // Slow down ball if its velocity exceeds the allowed maximum
            float magnitudeDifference = velocityXZ.magnitude - maxMoveSpeed;
            Vector3 oppositeDirection = -velocityXZ.normalized;
            rgbd.AddForce(oppositeDirection * magnitudeDifference * moveDecelerationFast, ForceMode.Acceleration);
        }

        // Camera udpates
        Vector3 nextCameraTargetPosition = transform.position + camOffsetPosition;
        if(Vector3.Distance(nextCameraTargetPosition, cameraTargetPosition) > camTargetMaxDistance){
            cameraTargetPosition = nextCameraTargetPosition;
        }
        
        if(cameraTransform.position != cameraTargetPosition){
            float camDistance = Vector3.Distance(cameraTransform.position, cameraTargetPosition);
            float currentCamSpeed = camMoveSpeed * Mathf.Clamp(camTargetMaxDistance / camDistance, 0f, 2f);
            cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, cameraTargetPosition, ref camVelocity, camMoveSpeed);
        }
    }

    void LateUpdate(){

    }

    private void OnCollisionEnter(Collision other){
        if(other.gameObject.layer == LayerMask.NameToLayer("Ground")){
            Vector2 bounceDirection = other.contacts[0].normal;
            rgbd.AddForce(bounceDirection * currentBounce, ForceMode.Impulse);
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Death")){
            SetReward(-1f);
           EndEpisode();
        }
    }
    
    private void OnTriggerEnter(Collider other){
        if(other.gameObject.layer == LayerMask.NameToLayer("Death")){
            SetReward(-1f);
            EndEpisode();
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Goal")){
            SetReward(GetCumulativeReward() + 1f);
            EndEpisode();
        }
    }
}
