using System;

using UnityEngine;

public class SinglePlatformGenerator : MonoBehaviour
{
    [Header("Generator Settings")]
    public float minX, maxX, minY, maxY, minZ, maxZ;
    public float minDistance;
    public GameObject platformPrefab;
    public GameObject currentPlatform;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P)){
            Generate();
        }
    }

    public void Generate(){
        
        if(currentPlatform != null){
            Destroy(currentPlatform);
        }
        float x = UnityEngine.Random.Range(minX, maxX);
        if(x < 0f){
            x = Mathf.Clamp(x, minX, -minDistance);
        }
        else{
            x = Math.Clamp(x, minDistance, maxX);
        }
        float y = UnityEngine.Random.Range(minY, maxY);

        float z = UnityEngine.Random.Range(minZ, maxZ);
        if(z < 0f){
            z = Mathf.Clamp(z, minZ, -minDistance);
        }
        else{
            z = Math.Clamp(z, minDistance, maxZ);
        }
        Vector3 platformPosition = new Vector3(x, y, z);
        GameObject platform = Instantiate(platformPrefab, transform);
        platform.transform.localPosition = platformPosition;
        currentPlatform = platform;

    }
}
