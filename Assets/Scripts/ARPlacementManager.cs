using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


public class ARPlacementManager : MonoBehaviour

{
    [Header("Settings")]
    [SerializeField] private GameObject prefabToPlace;
    [Header("References")]
    [SerializeField] private ARInputHandler inputHandler;

    private ARRaycastManager _raycastManager;
    // A list to store the results of the raycast hit
    private static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    private void Awake()
    {
        _raycastManager = FindFirstObjectByType<ARRaycastManager>();
    }
    
    private void OnEnable()
    {
        // 1. Start listening to the Input Handler
        inputHandler.OnPerformTap += PlaceObject;
    }

    private void OnDisable()
    {
        // 2. Stop listening
        inputHandler.OnPerformTap -= PlaceObject;
    }
    
    private void PlaceObject(Vector2 screenPos)
    {
        // 3. Shoot a ray from the screen position into the AR environment
        // TrackableType.PlaneWithinPolygon looks for actual detected floor / table geometry
        if (_raycastManager.Raycast(screenPos, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            // 4. Raycast hits are sorted by distance; [0] is the closest surface
            Pose hitPose = s_Hits[0].pose;
            // 5. Spawn the object at the real-world location
            Instantiate(prefabToPlace, hitPose.position, hitPose.rotation);
        }
    }
}