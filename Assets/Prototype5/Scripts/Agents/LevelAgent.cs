using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class LevelAgent : Agent
{
    // References
    [Header("References")]
    public GameObject ball;
    public AgentConnector agentConnector;

    [Header("Settings")]
    public List<GameObject> levelObjectPrefabs;
    public GameObject goalPrefab;
    public float minDistance, maxDistance;
    public float minScale, maxScale;
    public float initialGoalDistance;
    public int objectCount;

    // Runtime variables
    Transform currentGoal;
    Vector3 currentSpawnPosition;
    GameObject currentObject;
    Collider currentTargetCollider;
    string currentObjectTag;
    float smallestDistanceToGoal;
    public bool isHeuristic;

    private void Awake()
    {

    }
    public override void Initialize()
    {
        currentSpawnPosition = Vector3.zero;
        agentConnector = GameObject.Find("AgentConnector").GetComponent<AgentConnector>();
        agentConnector.EndAllAgentEpisodes.AddListener(EndEpisode);
    }

    public override void OnEpisodeBegin()
    {
        // Reset spawn position, reset goal, reset distance to goal
        currentSpawnPosition = Vector3.zero;
        currentGoal = Instantiate(goalPrefab, transform).transform;
        Vector3 goalPosition = Random.insideUnitSphere.normalized * initialGoalDistance;
        currentGoal.transform.localPosition = goalPosition;
        smallestDistanceToGoal = initialGoalDistance;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add 6 observations
        sensor.AddObservation(currentGoal.transform.position); 
        sensor.AddObservation(currentSpawnPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Decide on the next object spawn position. Moving from the current spawn position.
        float nextX = Mathf.Lerp(minDistance, maxDistance, actions.ContinuousActions[0] +1f / 2f);
        float nextY = Mathf.Lerp(minDistance, maxDistance, actions.ContinuousActions[1] +1f / 2f);
        float nextZ = Mathf.Lerp(minDistance, maxDistance, actions.ContinuousActions[2] +1f / 2f);
        Vector3 nextPosition = new Vector3(currentSpawnPosition.x + nextX, currentSpawnPosition.y + nextY, currentSpawnPosition.z + nextZ);
        
        // Decide on the next object prefab to use and spawn it
        int nextLevelObjectIndex = actions.DiscreteActions[0];
        GameObject nextObject = Instantiate(levelObjectPrefabs[nextLevelObjectIndex], transform);
        nextObject.transform.localPosition = nextPosition;
        nextObject.GetComponent<LevelObject>().parentLevelAgent = this;

        // Set the new level object to be the balls next target
        currentObject.GetComponent<LevelObject>().isTarget = false;

        currentSpawnPosition = nextPosition;
        currentObject = nextObject;
        currentObjectTag = nextObject.tag;
        currentTargetCollider = nextObject.GetComponent<Collider>();



        // Evaluate actions
        float nextDistanceToGoal = Vector3.Distance(currentSpawnPosition, currentGoal.localPosition);
        if(nextDistanceToGoal < smallestDistanceToGoal){
            AddReward(0.05f);
            smallestDistanceToGoal = nextDistanceToGoal;
        }
        else{
            AddReward(-0.025f);
        }
        if(nextDistanceToGoal < initialGoalDistance){
            AddReward(1f);
            agentConnector.EndAllAgentEpisodes.Invoke();
        }

        if(isHeuristic && transform.childCount > 3){
            // Delete previous level object
            Destroy(transform.GetChild(1).gameObject);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        continuousActions[0] = Random.Range(0f, 1f);
        continuousActions[1] = Random.Range(0f, 1f);
        continuousActions[2] = Random.Range(0f, 1f);

        discreteActions[0] = Random.Range(0, levelObjectPrefabs.Count);
    }
    public void OnTargetTriggerEnter(Collider other){
        // Ball reached target trigger area, spawn next object
        RequestDecision();
    }
    public void OnTargetCollisionEnter(Collision other){
        // Ball collided with target
    }

}
