using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class MoveToGoalAgent : Agent
{
    public float moveSpeed;
    public Transform goalTransform;
    public float minGoalDistance;
    public BoxCollider groundCollider;
    public BoxCollider agentCollider;
    public MeshRenderer groundRenderer;
    public Material rewardMaterial;
    public Material penaltyMaterial;
    public override void OnEpisodeBegin()
    {
        float minX = groundCollider.bounds.min.x + agentCollider.transform.localScale.x;
        float maxX = groundCollider.bounds.max.x - agentCollider.transform.localScale.x;
        float minZ = groundCollider.bounds.min.z + agentCollider.transform.localScale.z;
        float maxZ = groundCollider.bounds.max.z - agentCollider.transform.localScale.z;

        Vector3 randomGoalPosition = new Vector3(Random.Range(minX, maxX), goalTransform.position.y, Random.Range(minZ, maxZ));
        Vector3 randomAgentPositin = new Vector3(Random.Range(minX, maxX), goalTransform.position.y, Random.Range(minZ, maxZ));

        while(Vector3.Distance(randomGoalPosition, randomAgentPositin) < minGoalDistance){
            randomGoalPosition = new Vector3(Random.Range(minX, maxX), goalTransform.position.y, Random.Range(minZ, maxZ));
        }
        transform.position = randomAgentPositin;
        goalTransform.position = randomGoalPosition;
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(goalTransform.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        Debug.Log(actions.ContinuousActions[0]);
        Debug.Log(actions.ContinuousActions[1]);
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        transform.position += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");   
    }


    private void OnTriggerEnter(Collider other){
        if(other.gameObject.layer == LayerMask.NameToLayer("Goal")){
            SetReward(1f);
            groundRenderer.material = rewardMaterial;
            EndEpisode();

        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Border")){
            SetReward(-1f);
            groundRenderer.material = penaltyMaterial;
            EndEpisode();
        }
    }


}
