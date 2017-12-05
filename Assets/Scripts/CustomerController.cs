﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CustomerController : MonoBehaviour {
    public bool isHungry = true;
    public bool isSeated = false;
    public GameObject money;   
    [SerializeField]
    public float fullness = 0f;

    private NavMeshAgent agent;
    [SerializeField]
    private GameObject target;
   // private Renderer rend;
    private Collider coll;
    private Animator animator;

    private Rigidbody rb;

    GameController gameCtrl;
    public string state;

    // Use this for initialization
    void Start () {
        gameObject.layer = 9;
        agent = GetComponent<NavMeshAgent>();
        //rend = GetComponent<Renderer>();
        coll = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        agent.enabled = true;
        animator.SetBool("isSitting", false);
        state = "looking for seat";
        gameCtrl = GameObject.Find("GameController").GetComponent<GameController>();
        
    }
	
	// Update is called once per frame
	void Update () {

        switch (state)
        {
            case "looking for seat":

                target = FindNearestEmptySeat(GameObject.FindGameObjectsWithTag("empty seat"));
                animator.SetBool("isWalking", true);
                if (target != null) 
                {
                    target.tag = "taken seat";
                    state = "going to seat";
                } else
                {
                    target = GameObject.Find("Player");
                    state = "angry";
                }
                agent.SetDestination(new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z));
                
                break;

            case "going to seat":
                animator.SetBool("isWalking", true);
                if (isSeated == true) state = "waiting for food";
                break;

            case "waiting for food":
                gameObject.layer = 0;
                animator.SetBool("isWalking", false);
                animator.SetTrigger("isSitting");
                if (isHungry)
                {
                    //rend.material.color = Color.red;

                }
                else
                {
                   // rend.material.color = Color.green;
                }

                if (fullness >= 100) state = "start leaving";
                    break;
            case "start leaving":
                //restarts movement
                
                target.tag = "empty seat";
                agent.enabled = true;
                agent.isStopped = false;
                coll.isTrigger = false;
                rb.isKinematic = false;
                gameObject.layer = 9;
                //customer leaves
                target = GameObject.Find("portal");
                agent.SetDestination(new Vector3(target.transform.position.x, target.transform.position.y, target.transform.position.z));
                animator.SetBool("isWalking", true);

                gameCtrl.customersFed++;
                state = "leaving";
                break;
            case "leaving":
                animator.SetBool("isWalking", true);
                if (Vector3.Distance(target.transform.position, transform.position) <= 2f)
                {
                    Destroy(gameObject);
                }
                break;
            case "angry":
                animator.SetBool("isWalking", true);
                //print(name.ToString()+ " is angry >:( !!");
                break;
        }
        
    }


    void OnTriggerEnter(Collider other){
        switch (other.tag)
        {
            case "finished food":
                if (isSeated)
                {
                    fullness += 100f;
                   
                    DropReward(Random.Range(1,4));
                    Destroy(other.gameObject);
                    PickupController pickupCtrl = Camera.main.GetComponent<PickupController>();
                    pickupCtrl.pickedup_object = null;
                    pickupCtrl.pickedup_objectRb = null;
                    pickupCtrl.pickedup = false;
                }
                break;
            case "taken seat":
                if (other.gameObject == target && isSeated == false)
                {
                    target.tag = "seat";
                    isSeated = true;
                    agent.isStopped = true;
                    agent.enabled = false;
                    coll.isTrigger = true;
                    rb.isKinematic = true;
                    //moves customer to seat
                    transform.position = target.transform.position;
                    transform.rotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
                }
                break;
        }     
    }

    GameObject FindNearestEmptySeat(GameObject[] emptySeats)
    {
        GameObject chosenSeat = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (GameObject seat in emptySeats)
        {
            float dist = Vector3.Distance(seat.transform.position, currentPos); 
            if (dist < minDist)
            {
                chosenSeat = seat;
                minDist = dist;
            }
        }
        return chosenSeat;

    }
    void DropReward(int repeat)
    {
        //print("drop reward"+gameObject.name.ToString());
        for (int i = 0; i < repeat; i++)
        {
            Instantiate(money, transform.position +transform.forward * 2, transform.rotation);
            Instantiate(gameCtrl.ingredientReward[Random.Range(0, gameCtrl.ingredientReward.Length-1)], transform.position, transform.rotation);
            
        }

    }

}
