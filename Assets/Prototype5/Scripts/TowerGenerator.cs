using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TowerGenerator : MonoBehaviour
{
    // Settings
    public float towerHeight;
    public float maxGenerationRadius;
    public float minHeightIncrease;
    public float maxHeightIncrease;
    public float minDistanceIncrease;
    public float maxDistanceIncrease;
    public float fragileChance;

    public GameObject platformPrefab;
    public GameObject fragilePlatformPRefab;
    
    // Runtime
    Vector3 currentPosition;
    
    
    void Start()
    {

    }

    void GenerateTower(){
        currentPosition = transform.position;
        while(currentPosition.y < towerHeight){
                
            Vector3 nextXZDirection = new Vector3(Random.Range(-1f, 1f),0f, Random.Range(-1f, 1f)).normalized;
            float nextDistanceIncrease = Random.Range(minDistanceIncrease, maxDistanceIncrease);
            float nextHeightIncrease = Random.Range(minHeightIncrease, maxHeightIncrease);
            Vector3 nextPosition = new Vector3(
                currentPosition.x + nextXZDirection.x * nextDistanceIncrease,
                currentPosition.y + nextHeightIncrease,
                currentPosition.z + nextXZDirection.z * nextDistanceIncrease
            ); 
            
            
            while(Vector2.Distance(new Vector2(nextPosition.x, nextPosition.z), new Vector2(transform.position.x, transform.position.z)) > maxGenerationRadius){
                nextXZDirection = Quaternion.AngleAxis(Random.Range(10f, 310f), Vector3.up) * nextXZDirection;
                nextPosition = new Vector3(
                    currentPosition.x + nextXZDirection.x * nextDistanceIncrease,
                    currentPosition.y + nextHeightIncrease,
                    currentPosition.z + nextXZDirection.z * nextDistanceIncrease
                ); 
            }
            
            
            
            if(Random.Range(0, 1f) <= fragileChance){
                GameObject nextPlatform = GameObject.Instantiate(fragilePlatformPRefab, transform);
                nextPlatform.transform.position = nextPosition;
            }
            else{
                GameObject nextPlatform = GameObject.Instantiate(platformPrefab, transform);
                nextPlatform.transform.position = nextPosition;
            }
            currentPosition = nextPosition;
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T)){
            for(int i = 0; i < transform.childCount; i++){
                GameObject.Destroy(transform.GetChild(i).gameObject);
            }
            GenerateTower();
        }
    }
}
