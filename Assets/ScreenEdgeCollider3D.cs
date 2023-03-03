using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ScreenEdgeCollider3D : MonoBehaviour
{
    [SerializeField] private float zDistance = 4;
    [SerializeField] private float colliderZSize = 1;

    [SerializeField] private bool automaticScreenRes = true;
    [SerializeField] private float screenWidth = 1080;
    [SerializeField] private float screenHeight = 1920;
    
    [SerializeField] private GameObject[] colliderParents = new GameObject[4];
    
    [Header("Gizmos")]
    [SerializeField] private bool gizmos = true;
    [SerializeField] private bool rays;
    [SerializeField] private float pointSize = 0.005f;

    private Camera mainCam;
    private Transform camTransform;
    
    //TOP = 0, RIGHT = 1, BOTTOM = 2, LEFT = 3 
    private GameObject[] colliders = new GameObject[4];
    private Vector3[] directions = new Vector3[4];
    
    private Vector3[,] points;
    private Vector3 topRightPoint;
    private Vector3 topLeftPoint;
    private Vector3 bottomRightPoint;
    private Vector3 bottomLeftPoint;
    
    [EditorButton]
    public void SetColliders()
    {
        if (automaticScreenRes)
        {
            screenWidth = Screen.currentResolution.width;
            screenHeight = Screen.currentResolution.height;
        }
        
        SetPoints();
        DestroyColliders();
        
        if(!colliderParents[0])
            colliderParents[0] = new GameObject("TopColliderParent") ;
        if(!colliderParents[1])
            colliderParents[1]= new GameObject("RightColliderParent");
        if(!colliderParents[2])
            colliderParents[2] = new GameObject("BottomColliderParent");
        if(!colliderParents[3])
            colliderParents[3] = new GameObject("LeftColliderParent");
        
        colliderParents[0].transform.position = (topRightPoint + topLeftPoint) / 2f;
        colliderParents[1].transform.position = (topRightPoint + bottomRightPoint) / 2f;
        colliderParents[2].transform.position = (bottomLeftPoint + bottomRightPoint) / 2f;
        colliderParents[3].transform.position = (bottomLeftPoint + topLeftPoint) / 2f;

        directions = new Vector3[4];
        for (int i = 0; i < colliderParents.Length; i++)
        {
            directions[i] = (colliderParents[i].transform.position - camTransform.position).normalized;
            colliderParents[i].transform.rotation =
                Quaternion.LookRotation(directions[i]); 
            colliderParents[i].transform.SetParent(transform);//.rotation = Quaternion.Euler(directions[i]);
        }

        colliders = new GameObject[4];
        for (var i = 0; i < colliders.Length; i++)
        {
            var o = colliders[i];
            if (!o)
                colliders[i] = new GameObject("Collider");
            
            colliders[i].transform.SetParent(colliderParents[i].transform);
            colliders[i].transform.localPosition = Vector3.zero;
            colliders[i].transform.localRotation = Quaternion.identity;
            if(!colliders[i].TryGetComponent<BoxCollider>(out var collider))
                colliders[i].AddComponent<BoxCollider>();
        }
        

        colliders[0].transform.localScale = Vector3.forward * colliderZSize + Vector3.right * Vector3.Distance(points[1, 0], points[1, 1]);
        colliders[2].transform.localScale = Vector3.forward * colliderZSize + Vector3.right * Vector3.Distance(points[1, 0], points[1, 1]);
        
        colliders[1].transform.localScale = Vector3.forward * colliderZSize + Vector3.up * Vector3.Distance(points[0, 0], points[1, 1]);
        colliders[3].transform.localScale = Vector3.forward * colliderZSize + Vector3.up * Vector3.Distance(points[0, 0], points[1, 1]);

    }

    private void DestroyColliders()
    {
        foreach (var o in colliders)
        {
            DestroyImmediate(o);
        }
    }

    private void OnValidate()
    {
        SetPoints();
    }

    private void SetPoints()
    {
        mainCam = Camera.main;
        camTransform = mainCam.transform;

        var forwardPos = camTransform.forward * (mainCam.nearClipPlane + zDistance);

        bottomLeftPoint = mainCam.ScreenToWorldPoint(forwardPos);
        bottomRightPoint = mainCam.ScreenToWorldPoint(Vector3.right * screenWidth + forwardPos);
        topLeftPoint = mainCam.ScreenToWorldPoint(Vector3.up * screenHeight + forwardPos);
        topRightPoint = mainCam.ScreenToWorldPoint(Vector3.right * screenWidth + Vector3.up * screenHeight +
                                                   forwardPos);

        points = new Vector3[2, 2];
        points[0, 0] = bottomLeftPoint;
        points[0, 1] = bottomRightPoint;
        points[1, 0] = topLeftPoint;
        points[1, 1] = topRightPoint;
    }

    private void OnDrawGizmos()
    {
        if(!gizmos) return;
        
        Gizmos.color = Color.green;
        foreach (var point in points)
        {
            Gizmos.DrawSphere(point, pointSize);
        }
        
        Gizmos.color = Color.blue;
        foreach (var colliderParent in colliderParents)
        {
            if(colliderParent)
                Gizmos.DrawSphere(colliderParent.transform.position, pointSize);
        }
        
        if(!rays) return;
        
        Gizmos.color = Color.red;
        foreach (var point in points)
        {
            Gizmos.DrawRay(camTransform.position,point - camTransform.position);

        }

        foreach (var direction in directions)
        {
            Ray ray = new Ray(camTransform.position, direction);
            Gizmos.DrawRay(ray);
        }
    }
}
