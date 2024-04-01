using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitControl : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1000f;
    [SerializeField] private int axisScaler = 10;
    [SerializeField] private Transform rotateAround;

    private List<Collider2D> nearbyObjects;

    #region Technical
    private bool orbitOjects;
    #endregion

    private void Awake()
    {
        nearbyObjects = new List<Collider2D>();
    }

    public void OrbitObjects(InputAction.CallbackContext context)
    {
        orbitOjects = true;
    }

    public void PushObjects(InputAction.CallbackContext context)
    {
        foreach (Collider2D collider in nearbyObjects)
        {
            collider.attachedRigidbody.AddForce(Vector3.forward * 1000000);
        }
        orbitOjects = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        nearbyObjects.Add(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        nearbyObjects.Remove(collision);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (orbitOjects)
        {
            other.transform.RotateAround(rotateAround.position, Vector3.forward * axisScaler, rotationSpeed);
        }

        //// When button releases, push object. TODO
        //if (Input.GetKeyUp(KeyCode.Q))
        //{
        //    other.attachedRigidbody.AddForce(Vector3.forward * 1000000);
        //}
    }

}
