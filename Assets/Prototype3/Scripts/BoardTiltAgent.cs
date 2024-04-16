using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;

public class BoardTiltAgent : Agent
{
    public float maxTiltAngle;
    public float tiltSpeed;
    public float maxDistance;
    
    public int obstacleCount;
    public int obstalceRows;

    public Transform marbleTransform;
    public Transform goalTransform;
    public Transform allHoles;
    public float gravityScale;
    public Transform groundTransform;
    Vector3 initialMarblePosition;
    Quaternion initialBoardRotation;
    Rigidbody rgbd;
    Vector3 eulerAngleVelocity;
    float startDistanceToGoal;
    float currentDistanceToGoal;

    public Material penaltyMaterial;
    public Material rewardMaterial;

    public override void Initialize()
    {
        initialMarblePosition = marbleTransform.position;
        initialBoardRotation = transform.rotation;
        rgbd = GetComponent<Rigidbody>();
        eulerAngleVelocity = Vector3.zero;



    }
    public override void OnEpisodeBegin()
    {
        transform.rotation = initialBoardRotation;
        marbleTransform.GetComponent<Rigidbody>().constraints =RigidbodyConstraints.FreezeAll;
        marbleTransform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        marbleTransform.position = initialMarblePosition;
        marbleTransform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Bounds groundBounds = groundTransform.GetComponent<BoxCollider>().bounds;
        Vector3 nextGoalPosition = new Vector3(Random.Range(groundBounds.min.x + 0.5f, groundBounds.max.x - 0.5f),goalTransform.position.y, Random.Range(groundBounds.min.z + 0.5f, groundBounds.max.z - 0.5f));
        goalTransform.position = nextGoalPosition;
        marbleTransform.GetComponent<Rigidbody>().constraints =RigidbodyConstraints.None;
        eulerAngleVelocity = Vector3.zero;

        startDistanceToGoal = Vector3.Distance(marbleTransform.position, goalTransform.position);
        currentDistanceToGoal = startDistanceToGoal;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((marbleTransform.localPosition - goalTransform.localPosition).normalized);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        
        float tiltX = actions.ContinuousActions[0];
        float tiltZ = actions.ContinuousActions[1];
        
        if(tiltX > 0 && transform.rotation.x > maxTiltAngle){
            tiltX = 0f;
        }
        else if(tiltX < 0 && transform.rotation.x < -maxTiltAngle){
            tiltX = 0f;
        }

        if(tiltZ > 0 && transform.rotation.z > maxTiltAngle){
            tiltZ = 0f;
        }
        else if(tiltZ < 0 && transform.rotation.z < -maxTiltAngle){
            tiltZ = 0f;
        }


        eulerAngleVelocity = new Vector3(tiltX * tiltSpeed , 0f, tiltZ * tiltSpeed);
        /*
        if(Vector3.Distance(initialMarblePosition, marbleTransform.position) > maxDistance){
            AddReward(-1f);
            groundTransform.GetComponent<MeshRenderer>().material = penaltyMaterial;
            EndEpisode();
        }
        */
        float distanceReward = 1f - currentDistanceToGoal / startDistanceToGoal;
        if(distanceReward < 0f){
            distanceReward = 0f;
        }
        SetReward(distanceReward);
        
    }
    private void FixedUpdate(){
        Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * Time.fixedDeltaTime);
        rgbd.MoveRotation(rgbd.rotation * deltaRotation);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    public void MarbleCollided(Collision other){
        
        if(other.gameObject.layer == LayerMask.NameToLayer("Goal")){
            Debug.Log("FArt");
            SetReward(1f);
            groundTransform.GetComponent<MeshRenderer>().material = rewardMaterial;
            EndEpisode();
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Penalty")){
            Debug.Log("FArt");
            SetReward(-1f);
            groundTransform.GetComponent<MeshRenderer>().material = penaltyMaterial;
            EndEpisode();
        }
    }
    public void MarbleTriggered(Collider other){
        if(other.gameObject.layer == LayerMask.NameToLayer("Goal")){
            SetReward(1f);
            groundTransform.GetComponent<MeshRenderer>().material = rewardMaterial;
            EndEpisode();
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Penalty")){
            SetReward(-1f);
            groundTransform.GetComponent<MeshRenderer>().material = penaltyMaterial;
            EndEpisode();
        }
    }

    
    
}
