using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementController : MonoBehaviour
{
    public GameObject charaPrefab;
    public GameObject targetIndicator;

    GameObject chara = null;
    ARRaycastManager raycastManager;
    Pose placementPose;
    bool placementPoseIsValid = false;

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        Debug.Log("raycastManager? " + raycastManager);
    }

    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();
        
        if (placementPoseIsValid && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                UpdateObject();
            }
        }   
    }

    private void UpdateObject()
    {
        if (chara == null)
            chara = Instantiate(charaPrefab, placementPose.position, placementPose.rotation);
        else
            chara.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
    }

    private void UpdatePlacementIndicator()
    {
        targetIndicator.SetActive(placementPoseIsValid);
        if (placementPoseIsValid)
        {
            targetIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
    }

    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();

        raycastManager.Raycast(screenCenter, hits, TrackableType.Planes);

        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid)
        {
            placementPose = hits[0].pose;

            var cameraForward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized * -1f;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }
}
