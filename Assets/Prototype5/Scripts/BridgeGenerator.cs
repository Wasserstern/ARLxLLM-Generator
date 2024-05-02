using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeGenerator : MonoBehaviour
{
    public float bridgeMaxLength;
    public float minNextDistance;
    public float maxNextDistance;
    public float minNextAngle;
    public float maxNextAngle;
    public float fragilePlatformChance;
    public BridgeMode bridgeMode;
    
    public GameObject standardPlatformPrefab;
    public GameObject fragilePlatformPrefab;
    public GameObject goalPrefab;

    Vector3 initialBridgeDirection;
    Vector3 currentPlatformPosition;
    float currentBridgeLength;

    public enum BridgeMode {undirected, directed}
    public GameObject goalObject;
    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.B)){
            GenerateBridge();
        }
    }

    public void GenerateBridge(){
        for(int i = 0; i < transform.childCount; i++){
            Destroy(transform.GetChild(i).gameObject);
        }
        currentBridgeLength = 0f;
        currentPlatformPosition = transform.position;

        switch(bridgeMode){
            case BridgeMode.undirected:{
                initialBridgeDirection = new Vector3(Random.Range(-1, 1f),Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                int counter = 0;
                while(currentBridgeLength < bridgeMaxLength)
                {
                    float nextDistance = 0;
                    if(counter == 0){
                        nextDistance = 25f;
                    }
                    else{
                        nextDistance = Random.Range(minNextDistance, maxNextDistance);
                    }
                    counter++;
                    float nextAngleY = Random.Range(minNextAngle, maxNextAngle);
                    float nextAngleZ = Random.Range(minNextAngle, maxNextAngle);
                    Vector3 nextDirection = Quaternion.AngleAxis(nextAngleY, Vector3.up) * initialBridgeDirection;
                    nextDirection = Quaternion.AngleAxis(nextAngleZ, Vector3.forward) * initialBridgeDirection;
                    Vector3 nextPlatformPosition = currentPlatformPosition + nextDirection * nextDistance;
                    if(Random.Range(0f, 1f) < fragilePlatformChance)
                    {
                        GameObject nextPlatform = GameObject.Instantiate(fragilePlatformPrefab, transform);
                        nextPlatform.transform.position =  nextPlatformPosition;
                    }
                    else
                    {
                        GameObject nextPlatform = GameObject.Instantiate(standardPlatformPrefab, transform);
                        nextPlatform.transform.position =  nextPlatformPosition;
                    }
                    currentPlatformPosition = nextPlatformPosition;
                    currentBridgeLength += nextDistance;
                }
                break;
            }
            case BridgeMode.directed:{
                initialBridgeDirection = new Vector3(Random.Range(-1, 1f),0f, Random.Range(-1f, 1f)).normalized;
                float nextDistance; 
                float nextAngle;
                Vector3 nextDirection;
                Vector3 nextPlatformPosition;
                int counter = 0;
                while(currentBridgeLength < bridgeMaxLength)
                {
                    if(counter == 0){
                        nextDistance = 25f;
                    }
                    else{
                        nextDistance = Random.Range(minNextDistance, maxNextDistance);
                    }
                    counter++;
                    nextAngle = Random.Range(minNextAngle, maxNextAngle);
                    nextDirection = Quaternion.AngleAxis(nextAngle, Vector3.up) * initialBridgeDirection;
                    nextPlatformPosition = currentPlatformPosition + nextDirection * nextDistance;

                    if(Random.Range(0f, 1f) < fragilePlatformChance)
                    {
                        GameObject nextPlatform = GameObject.Instantiate(fragilePlatformPrefab, transform);
                        nextPlatform.transform.position =  nextPlatformPosition;
                    }
                    else
                    {
                        GameObject nextPlatform = GameObject.Instantiate(standardPlatformPrefab, transform);
                        nextPlatform.transform.position =  nextPlatformPosition;
                    }
                    currentPlatformPosition = nextPlatformPosition;
                    currentBridgeLength += nextDistance;
                }
                nextDistance = Random.Range(minNextDistance, maxNextDistance);
                nextAngle = Random.Range(minNextAngle, maxNextAngle);
                nextDirection = Quaternion.AngleAxis(nextAngle, Vector3.up) * initialBridgeDirection;
                nextPlatformPosition = currentPlatformPosition + nextDirection * nextDistance;

                GameObject goal = GameObject.Instantiate(goalPrefab, transform);
                goal.transform.position = nextPlatformPosition;
                goalObject = goal;
                break;
            }
        }
        
    }
}
