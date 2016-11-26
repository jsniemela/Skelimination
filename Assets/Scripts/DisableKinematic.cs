using UnityEngine;
using System.Collections;
using Vuforia;
using System;

public class DisableKinematic : MonoBehaviour, ITrackableEventHandler
{
    private TrackableBehaviour mTrackableBehaviour;
    public GameObject player;
    public GameObject skeletons;

    void Start()
    {
        mTrackableBehaviour = GetComponent<TrackableBehaviour>();
        if (mTrackableBehaviour)
        {
            mTrackableBehaviour.RegisterTrackableEventHandler(this);
        }
    }

    public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
    {
        if (newStatus == TrackableBehaviour.Status.DETECTED ||
            newStatus == TrackableBehaviour.Status.TRACKED ||
            newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {
            //Disables kinematic on imagetarget detection
            Debug.Log("target detected, set kinematic to false");
            player.GetComponent<Rigidbody>().isKinematic = false;
            skeletons.GetComponentInChildren<Rigidbody>().isKinematic = false;
        }
    }

    // Use this for initialization
    

    // Update is called once per frame
    void Update () {
	
	}
}
