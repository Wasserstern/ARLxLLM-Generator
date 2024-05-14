using System;
using System.Collections;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class BallAgentProceduralSingular : Agent
{
    [Header("References")]
    public Transform cameraTransform;
    public SinglePlatformGenerator platformGenerator;
    public MeshRenderer startPlatformRenderer;
    public Material rewardMaterial;
    public Material penaltyMaterial;
    public Material currentMaterial;
    Rigidbody rgbd;
    Collider col;
    
    // Settings
    // TODO: Put camera logic somewhere else
    public float camMoveSpeed;
    public float camRotateSpeed;
    public float camTargetMaxDistance;

    [Header("Bounce Settings")]
    public float standardBounce;
    public float minBounce;
    public float maxBounce;
    public float bounceIncrease;
    public float bounceDecreaseStandard;
    public float bounceDecreaseFast;
    [Header("Movement Settings")]
    public float moveAcceleration;
    public float moveDeceleration;
    public float moveDecelerationFast;
    public float maxMoveSpeed;
    [Range(0f, 1f)]
    public float deadZoneMagnitude;
    public float minVelocityMagnitude;
    // ML Agent settings
    [Header("AI Settings")]
    public float deathDistance;
    public bool useBounceMeter;
    // Runtime variables
    [SerializeField]
    float currentBounce;
    Vector3 inputDirection;
    bool isPressingBounce;
    bool isPressingStiff;
    Vector3 cameraTargetPosition;
    Vector3 camOffsetPosition;
    Vector3 camVelocity;
    Vector3 localStartPosition;

    Transform targetPlatform;
    

    public override void Initialize()
    {
        rgbd = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        currentBounce = standardBounce;
        localStartPosition = transform.localPosition;

    }
    
    public override void OnEpisodeBegin()
    {
        // This runs on every new Ml Agent Episode when learning.
        transform.localPosition = localStartPosition;
        rgbd.velocity = Vector3.zero;
        currentBounce = standardBounce;
        isPressingBounce = false;
        isPressingStiff = false;
        inputDirection = Vector3.zero;

        platformGenerator.Generate();
        targetPlatform = platformGenerator.currentPlatform.transform;

        // Reset ball settings

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if(targetPlatform != null){
            sensor.AddObservation(targetPlatform.transform.localPosition);
            sensor.AddObservation(transform.localPosition);
            sensor.AddObservation(rgbd.velocity);
            if(useBounceMeter){
                sensor.AddObservation(currentBounce);
            }
            
        }
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
  
            inputDirection = new Vector3(actions.ContinuousActions[0],0f, actions.ContinuousActions[1]);
        if(useBounceMeter){
            isPressingBounce = actions.DiscreteActions[0] == 1;
            isPressingStiff = actions.DiscreteActions[1] == 1;
            Debug.Log(isPressingBounce + " " + isPressingStiff);
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
        }
        else{
            currentBounce = maxBounce;
        }
        
        
        if(transform.localPosition.y < -deathDistance){
            SetReward(-1f);
            startPlatformRenderer.material = penaltyMaterial;
            EndEpisode();
        }

        
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");

        if(useBounceMeter){
            ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

            discreteActions[0] = Convert.ToInt32(Input.GetKey(KeyCode.J));
            discreteActions[1] = Convert.ToInt32(Input.GetKey(KeyCode.K));
        }
    }

    private Vector3 NormalizeObservationVector(Vector3 vector, float minValue,  float maxValue){
        float x = (vector.x - minValue)/(maxValue - minValue);
        float y = (vector.x - minValue)/(maxValue - minValue);
        float z = (vector.x - minValue)/(maxValue - minValue);
        return new Vector3(x, y, z);
    }

    void Start()
    {
        cameraTargetPosition = cameraTransform.position;
        camOffsetPosition = cameraTransform.localPosition;
    }

    void Update()
    {
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

    void FixedUpdate(){
        Vector3 velocityXZ = new Vector3(rgbd.velocity.x, 0f, rgbd.velocity.z);
        if(inputDirection.magnitude > deadZoneMagnitude){
            // Apply input
            rgbd.AddForce(inputDirection * moveAcceleration, ForceMode.Acceleration);
        }
        else{
            if(velocityXZ.magnitude > minVelocityMagnitude){
                // Standard slow down when no input is detected to reduce floaty movement
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
        else if(other.gameObject.layer == LayerMask.NameToLayer("Ground")){
            if(other.gameObject.tag == "Platform" && other.gameObject.transform == targetPlatform){
                SetReward(1f);
                startPlatformRenderer.material = rewardMaterial;
                EndEpisode();
            }
        }

    }
}
