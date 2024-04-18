using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public class FragilePlatform : MonoBehaviour
{
    // References
    Collider col;
    MeshRenderer meshRenderer;
    // Settings
    public int maxIntegrity;
    public float rebuildTimeInSeconds;

    public Material standardMaterial;
    public Material destroyedMaterial;
    public bool canRebuild;
    // Runtime
    int currentIntegrity;
    float rebuildTimer;
    bool isRebuilding;
    void Start()
    {
        col = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
        currentIntegrity = maxIntegrity;
    }

    // Update is called once per frame
    void Update()
    {
        if(isRebuilding){
            rebuildTimer += Time.deltaTime;
            if(rebuildTimer >= rebuildTimeInSeconds){
                currentIntegrity = maxIntegrity;
                isRebuilding = false;
                col.enabled = true;
                meshRenderer.material = standardMaterial;
            }
        }
    }

    private void OnCollisionEnter(Collision other){
        if(other.gameObject.layer == LayerMask.NameToLayer("Ball")){
            currentIntegrity--;
            if(currentIntegrity <= 0){
                if(canRebuild){
                    col.enabled = false;
                    meshRenderer.material = destroyedMaterial;
                    rebuildTimer = 0f;
                    isRebuilding = true;
                }
                else{
                    Destroy(this.gameObject);
                }

            }
        }
    }
}
