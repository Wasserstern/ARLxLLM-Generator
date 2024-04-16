using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;

public class Player : Agent
{
    // Player settings
    public float moveSpeed;
    public float rotateSpeed;
    [Range(0f, 1f)]
    public float moveDeadZone;

    public float jumpForce;
    public float standardGravity;
    public float jumpMinGravity;
    public float jumpMaxGravity;
    public float groundCheckDistance;
    public float maxJumpTime;
    public float fallTime;
    [SerializeField]
    bool isJumping;
    [SerializeField]
    bool jumpButtonHeld;
    [SerializeField]
    bool isGrounded;
    // Components and references

    Rigidbody rgbd;
    Collider col;
    Coroutine currentCoroutine;
    public override void Initialize()
    {
        rgbd = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        standardGravity = Physics.gravity.y;
    }
    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        base.CollectObservations(sensor);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float yAxisRotate = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f );
        float move = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        transform.Rotate(new Vector3(0f, yAxisRotate * rotateSpeed * Time.deltaTime, 0f));
        if(move > moveDeadZone || move < -moveDeadZone){
            Vector3 moveVector = transform.forward * move * moveSpeed * Time.deltaTime;
            rgbd.velocity = new Vector3(moveVector.x, rgbd.velocity.y, moveVector.z);
        }
        else{
            rgbd.velocity = new Vector3(0f, rgbd.velocity.y, 0f);
        }
        
        RaycastHit hit = new RaycastHit();
        isGrounded = Physics.Raycast(transform.position, -transform.up, out hit, groundCheckDistance,  LayerMask.GetMask("Ground"));
        jumpButtonHeld = actions.DiscreteActions[0] == 1;
       
        
        
    }
    public void Update(){
        if(isGrounded && !isJumping){
            if(currentCoroutine != null){
                StopCoroutine(currentCoroutine);
            }
            
            Physics.gravity = new Vector3(0f, standardGravity, 0f);
        }
        if(isGrounded && jumpButtonHeld && !isJumping){
            isJumping = true;
            Debug.Log("Should jump");
            if(currentCoroutine != null){
                StopCoroutine(currentCoroutine);
            }
            Physics.gravity = new Vector3(0f, standardGravity, 0f);
            currentCoroutine = StartCoroutine(Jump());
        }
        Debug.Log(Physics.gravity.y);
    }

    private IEnumerator Jump(){
        rgbd.velocity = new Vector3(rgbd.velocity.x, 0f, rgbd.velocity.z);
        float elapsedTime = 0f;
        float startTime = Time.time;
        rgbd.AddForce(jumpForce * transform.up, ForceMode.Impulse);
        while(Time.time - startTime < maxJumpTime && jumpButtonHeld){
            float t = elapsedTime / maxJumpTime;
            Physics.gravity = new Vector3(0f, Mathf.Lerp(standardGravity, jumpMinGravity, t), 0f);
            if(t > 0.8f){
                Physics.gravity = new Vector3(0f, 0f, 0f);
                rgbd.velocity = new Vector3(rgbd.velocity.x, 0f,rgbd.velocity.y);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Physics.gravity = new Vector3(0f, jumpMinGravity, 0f);
        isJumping = false;
        elapsedTime = 0f;
        startTime = Time.time;
        while(Time.time - startTime < fallTime){
            float t = elapsedTime / fallTime;
            Physics.gravity = new Vector3(0f, Mathf.Lerp(jumpMinGravity, jumpMaxGravity, t), 0f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Convert.ToInt32(Input.GetKey(KeyCode.Space));
    }

    private void OnTriggerEnter(Collider other){

    }
    private void OnCollisionEnter(Collision other){

    }
}
