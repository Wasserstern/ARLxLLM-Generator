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

    public Transform marbleTransform;
    public Transform goalTransform;
    public Transform allHoles;
    public Transform groundTransform;
    Vector3 initialMarblePosition;
    Quaternion initialBoardRotation;

    public Material penaltyMaterial;
    public Material rewardMaterial;

    public override void Initialize()
    {
        initialMarblePosition = marbleTransform.position;
        initialBoardRotation = transform.rotation;
        if(marbleTransform.GetComponent<Rigidbody>().IsSleeping()){
        }


    }
    public override void OnEpisodeBegin()
    {
        transform.rotation = initialBoardRotation;
        marbleTransform.position = initialMarblePosition;
        marbleTransform.GetComponent<Rigidbody>().velocity = Vector3.zero;
        Bounds groundBounds = groundTransform.GetComponent<BoxCollider>().bounds;
        Vector3 nextGoalPosition = new Vector3(Random.Range(groundBounds.min.x + 0.5f, groundBounds.max.x - 0.5f),goalTransform.position.y, Random.Range(groundBounds.min.z + 0.5f, groundBounds.max.z - 0.5f));
        goalTransform.position = nextGoalPosition;
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

        transform.Rotate(new Vector3(tiltX * tiltSpeed * Time.deltaTime, 0f, tiltZ * tiltSpeed * Time.deltaTime));

        if(Vector3.Distance(initialMarblePosition, marbleTransform.position) > maxDistance){
            AddReward(-1f);
            groundTransform.GetComponent<MeshRenderer>().material = penaltyMaterial;
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    public void MarbleCollided(Collision other){
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
    public void MarbleTriggered(Collider other){
        if(other.gameObject.layer == LayerMask.NameToLayer("Goal")){
            SetReward(1f);
            groundTransform.GetComponent<MeshRenderer>().material = rewardMaterial;
            EndEpisode();
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Penalty")){
            Debug.Log("Just collided with wall");
            SetReward(-1f);
            groundTransform.GetComponent<MeshRenderer>().material = penaltyMaterial;
            EndEpisode();
        }
    }

    
    
}
