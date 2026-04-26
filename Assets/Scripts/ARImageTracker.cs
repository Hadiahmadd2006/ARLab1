using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[Serializable]
public class ImagePrefabEntry {
    public string imageName;
    public GameObject prefab;
}

public class ARImageTracker : MonoBehaviour {
    [SerializeField] private ARTrackedImageManager imageManager;
    [SerializeField] private List<ImagePrefabEntry> imagePrefabs;
    
    // Built once at startup for O(1) lookups during tracking
    private Dictionary<string, GameObject> _prefabLookup = new Dictionary<string, GameObject>();

    private void Awake() {
        foreach (var entry in imagePrefabs) {
            if (!_prefabLookup.ContainsKey(entry.imageName)) {
                _prefabLookup[entry.imageName] = entry.prefab;
            }
        }
    }

    private void OnEnable() {
        imageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    private void OnDisable() {
        imageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    private void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs) {
        foreach (var trackedImage in eventArgs.added) {
            HandleImageAdded(trackedImage);
        }
        
        foreach (var trackedImage in eventArgs.updated) {
            HandleImageUpdated(trackedImage);
        }
        
        foreach (var pair in eventArgs.removed) {
            HandleImageRemoved(pair.Value);
        }
    }

    private void HandleImageAdded(ARTrackedImage trackedImage) {
        string imageName = trackedImage.referenceImage.name;
        
        if (_prefabLookup.TryGetValue(imageName, out GameObject prefab)) {
            GameObject spawnedContent = Instantiate(
                prefab,
                trackedImage.transform.position,
                trackedImage.transform.rotation
            );
            spawnedContent.transform.SetParent(trackedImage.transform);
        }
    }

    private void HandleImageUpdated(ARTrackedImage trackedImage) {
        if (trackedImage.transform.childCount == 0) return;
        
        GameObject content = trackedImage.transform.GetChild(0).gameObject;
        Renderer rend = content.GetComponent<Renderer>();
        
        switch (trackedImage.trackingState) {
            case TrackingState.Tracking:
                content.SetActive(true);
                if (rend != null) rend.material.color = Color.green;
                break;
                
            case TrackingState.Limited:
                content.SetActive(true);
                if (rend != null) rend.material.color = Color.yellow;
                break;
                
            case TrackingState.None:
                content.SetActive(false);
                break;
        }
    }

    private void HandleImageRemoved(ARTrackedImage trackedImage) {
        Debug.Log("Image removed: " + trackedImage.referenceImage.name);
    }
}