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
                while(currentBridgeLength < bridgeMaxLength)
                {
                    float nextDistance = Random.Range(minNextDistance, maxNextDistance);
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
                while(currentBridgeLength < bridgeMaxLength)
                {
                    float nextDistance = Random.Range(minNextDistance, maxNextDistance);
                    float nextAngle = Random.Range(minNextAngle, maxNextAngle);
                    Vector3 nextDirection = Quaternion.AngleAxis(nextAngle, Vector3.up) * initialBridgeDirection;
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
        }
        
    }
}
