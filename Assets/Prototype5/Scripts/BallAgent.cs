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
    public BridgeGenerator bridgeGenerator;
    public Material rewardAttainedMaterial;
    public Material rewardUnattainedMaterial;
    Rigidbody rgbd;
    Collider col;
    
    // Settings
    public float camMoveSpeed;
    public float camTargetMaxDistance;
    public float springMaxTime;

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
    float initialYPosition;
    float currentSpringTime;
    bool isSpringing;

    // AI variables
    Vector3 localStartPosition;
    float rewardFraction;
    [SerializeField]
    GameObject currentTargetPlatform;
    int currentTargetPlatformIndex;
    

    public override void Initialize()
    {
        rgbd = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        currentBounce = standardBounce;
        localStartPosition = transform.localPosition;
        
    }
    public override void OnEpisodeBegin()
    {
        
        transform.localPosition = localStartPosition;
        rgbd.velocity = Vector3.zero;
        currentBounce = standardBounce;
        isPressingBounce = false;
        isPressingStiff = false;
        inputDirection = Vector3.zero;
        currentTargetPlatform = null;
        bridgeGenerator.GenerateBridge();
        initialYPosition = transform.localPosition.y;
        rewardFraction = 1f / bridgeGenerator.transform.childCount;
        currentTargetPlatform = bridgeGenerator.transform.GetChild(0).gameObject;
        currentTargetPlatformIndex = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        currentTargetPlatform = bridgeGenerator.transform.GetChild(currentTargetPlatformIndex).gameObject;
        if(currentTargetPlatform != null){
            Vector2 localPositionXZ = new Vector2(transform.localPosition.x, transform.localPosition.z);
            Vector2 localTargetPositionXZ = new Vector2(currentTargetPlatform.transform.localPosition.x, currentTargetPlatform.transform.localPosition.z);
            float currentTargetDistance = Vector2.Distance(localPositionXZ, localTargetPositionXZ);
            sensor.AddObservation(currentTargetDistance); // Observe Distance to target
            sensor.AddObservation((localTargetPositionXZ - localPositionXZ).normalized); // Observe direction to target
            sensor.AddObservation(rgbd.velocity); // Observe rgbd velocity
        }
        else{
            sensor.AddObservation(0f);
            sensor.AddObservation(Vector2.zero);
            sensor.AddObservation(Vector3.zero);
        }
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        inputDirection = new Vector3(actions.ContinuousActions[0],0f, actions.ContinuousActions[1]);
        if(useBounceMeter){
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
        }
        else{
            currentBounce = maxBounce;
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
        if(isSpringing){
            currentSpringTime += Time.deltaTime;
            if(currentSpringTime >= springMaxTime){
                currentSpringTime = 0f;
                isSpringing = false;
            }
        }
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
            if(velocityXZ.magnitude > minVelocityMagnitude && !isSpringing){
                // Standard slow down when no input is detected to reduce floaty movement
                Vector3 oppositeDirection = new Vector3(-rgbd.velocity.x, 0f, -rgbd.velocity.z).normalized;
                rgbd.AddForce(oppositeDirection * moveDeceleration, ForceMode.Acceleration);
            }
        }

        if(velocityXZ.magnitude > maxMoveSpeed && !isSpringing){
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


        if(transform.localPosition.y < initialYPosition - deathDistance){
            SetReward(-1f);
            EndEpisode();
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
        else if(other.gameObject.layer == LayerMask.NameToLayer("Goal")){
            SetReward(1f);
            EndEpisode();
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Reward")){
            Platform rewardPlatform = other.transform.parent.GetComponent<Platform>();
            if(!rewardPlatform.hasBeenHit && rewardPlatform.gameObject == currentTargetPlatform){
                rewardPlatform.hasBeenHit = true;
                other.gameObject.GetComponent<MeshRenderer>().material = rewardAttainedMaterial;
                AddReward(rewardFraction);
                currentTargetPlatformIndex++;
                currentTargetPlatform = bridgeGenerator.transform.GetChild(currentTargetPlatformIndex).gameObject;
            }
        }
    }

    public void Spring(Vector3 force, Vector3 springPosition){
        isSpringing = true;
        currentSpringTime = 0f;
        transform.position = springPosition;
        rgbd.velocity = Vector3.zero;
        rgbd.AddForce(force, ForceMode.Impulse);
    }
}
