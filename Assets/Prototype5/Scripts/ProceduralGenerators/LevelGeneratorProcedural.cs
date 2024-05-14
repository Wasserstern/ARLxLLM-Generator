using System.Collections.Generic;
using UnityEngine;

public class LevelGeneratorProcedural : MonoBehaviour{
    public float initialGoalDistance;
    public float targetGoalDistacne;
    public float minDistance, maxDistance;
    public float minAngle, maxAngle;
    public float minY, maxY;
    [Range(0f, 1f)]
    public float maxSteepness;
    [Range(-1f, 0f)]
    public float minSteepness;
    public int maxObjects;
    public List<GameObject> levelObjectPrefabs;
    public GameObject goalPrefab;
    Vector3 goalPosition;
    Vector3 currentGoalDirection;
    Vector3 currentSpawnPosition;
    float currentDistanceToGoal;
    int currentObjects;
    
    private void Start(){
    }

    private void Update(){
        if(Input.GetKeyDown(KeyCode.L)){
            Generate();
        }
    }
    public  void Generate(){
        for(int i = 0; i < transform.childCount; i++){
            Destroy(transform.GetChild(i).gameObject);
        }
        Debug.Log("Generating procedural level.");
        Vector3 randomSpherePoint = Random.insideUnitSphere.normalized;
        float goalX = Random.Range(-1f, 1f);
        float goalZ = Random.Range(-1f, 1f);
        goalPosition = (new Vector3(goalX, 0f, goalZ) - Vector3.zero).normalized * initialGoalDistance;
        currentSpawnPosition = Vector3.zero;
        currentDistanceToGoal = Vector3.Distance(currentSpawnPosition, goalPosition);

        GameObject initialObject = Instantiate(levelObjectPrefabs[0], transform);
        initialObject.transform.localPosition = Vector3.zero;

        while(currentDistanceToGoal > targetGoalDistacne && currentObjects < maxObjects)
        {
            // Spawn next level object
            currentGoalDirection = (goalPosition - new Vector3(currentSpawnPosition.x, 0f, currentSpawnPosition.z)).normalized;
            float nextAngleY = Random.Range(minAngle, maxAngle);
            float nextAngleZ = Random.Range(minAngle, maxAngle);
            Vector3 nextDirection = Quaternion.AngleAxis(nextAngleY, Vector3.up) * currentGoalDirection;
            //nextDirection = Quaternion.AngleAxis(nextAngleZ, Vector3.forward) * nextDirection;
            float randomY = Random.Range(minY, maxY);
            Vector3 nextSpawnPosition = currentSpawnPosition + nextDirection * Random.Range(minDistance, maxDistance);

            GameObject nextLevelObject = Instantiate(levelObjectPrefabs[Random.Range(0, levelObjectPrefabs.Count)], transform);
            nextSpawnPosition = new Vector3(nextSpawnPosition.x, randomY, nextSpawnPosition.z);
            nextLevelObject.transform.localPosition = nextSpawnPosition;
            currentSpawnPosition = nextSpawnPosition;

            currentDistanceToGoal = Vector3.Distance(currentSpawnPosition, goalPosition);
            currentObjects++;


        }
        currentObjects = 0;
        
    }
}