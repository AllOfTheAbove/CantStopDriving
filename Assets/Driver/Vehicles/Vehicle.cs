using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

[System.Serializable]
public class WheelPair
{
    public WheelCollider leftWheel;
    public GameObject leftWheelMesh;
    public WheelCollider rightWheel;
    public GameObject rightWheelMesh;
    public bool motor;
    public bool steering;
    public bool reverseTurn;
}

public class Vehicle : MonoBehaviour
{
    public List<WheelPair> wheelPairs;
    public float maxMotorTorque;
    public float maxSteeringAngle;

    public bool rewind = false;

    public GameObject obj;
    new private Transform transform;
    new private Rigidbody rigidbody;

    public GameObject boosts;
    public GameObject wheels;
    public ParticleSystem CoreFire, FrontFire, RearFire;
    public GameObject CarBody, CarFront, CarRear;
    private Transform CF;
    private Transform CC;
    private Transform CR;
    private bool Burning;


    private void Start()
    {
        obj = GameObject.FindGameObjectsWithTag("Driver")[0];
        transform = obj.transform;
        rigidbody = obj.GetComponent<Rigidbody>();

        CF = CarFront.transform;
        CC = CarBody.transform;
        CR = CarRear.transform;
    }

    public void UpdateWheel(WheelPair wheelPair)
    {
        Quaternion rot;
        Vector3 pos;
        wheelPair.leftWheel.GetWorldPose(out pos, out rot);
        wheelPair.leftWheelMesh.transform.position = pos;
        wheelPair.leftWheelMesh.transform.rotation = rot;
        wheelPair.rightWheel.GetWorldPose(out pos, out rot);
        wheelPair.rightWheelMesh.transform.position = pos;
        wheelPair.rightWheelMesh.transform.rotation = rot;
    }

    public void Explode()
    {
        if (CarBody == null)
        {
            return;
        }

        CF.position = new Vector3(CF.position.x, CF.position.y + 2, CF.position.z);
        CR.position = new Vector3(CF.position.x, CF.position.y - 2, CF.position.z);
        CC.position = new Vector3(CF.position.x + 2, CF.position.y, CF.position.z);

        CarFront.transform.position = new Vector3(CarFront.transform.position.x, CarFront.transform.position.y + 2, CarFront.transform.position.z);

        // Core
        Rigidbody CarBodyBody = CarBody.AddComponent<Rigidbody>();
        BoxCollider carBodyCollider = CarBody.AddComponent<BoxCollider>();
        carBodyCollider.size = new Vector3(0.5f, 0.5f, 0.5f);

        var locVel = transform.InverseTransformDirection(CarBodyBody.velocity);
        locVel.y += 12;
        locVel.x += 2;
        CarBodyBody.velocity = transform.TransformDirection(locVel);
        CoreFire.Play();

        // Front
        Rigidbody CarFrontBody = CarFront.AddComponent<Rigidbody>();
        BoxCollider carFrontCollider = CarFront.AddComponent<BoxCollider>();
        carFrontCollider.size = new Vector3(0.5f, 0.5f, 0.5f);
        locVel = transform.InverseTransformDirection(CarFrontBody.velocity);
        locVel.z += 8;
        locVel.y += 6;
        CarFrontBody.velocity = transform.TransformDirection(locVel);
        FrontFire.Play();

        // Rear
        Rigidbody CarRearBody = CarRear.AddComponent<Rigidbody>();
        BoxCollider carRearCollider = CarRear.AddComponent<BoxCollider>();
        carRearCollider.size = new Vector3(0.5f, 0.5f, 0.5f);
        locVel = transform.InverseTransformDirection(CarRearBody.velocity);
        locVel.z -= 8;
        locVel.y += 6;
        CarRearBody.velocity = transform.TransformDirection(locVel);
        RearFire.Play();

        Destroy(wheels);
        Destroy(boosts);
        //transform.DetachChildren(); // crash
    }

    public void Reset()
    {
        transform.position = new Vector3(transform.position.x + 1, transform.position.y + 2, transform.position.z + 1);
        transform.rotation = new Quaternion(0, 0, 0, 0);
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
    }

    public void Jump()
    {
        rigidbody.velocity = new Vector3(rigidbody.velocity.x, 10, rigidbody.velocity.z);
    }

    public void Boost()
    {
        rigidbody.velocity = new Vector3(transform.forward.x * 20f, rigidbody.velocity.y, transform.forward.z * 20f);
    }

    public void Brake()
    {
        rigidbody.velocity = new Vector3(rigidbody.velocity.x * 0.99f, rigidbody.velocity.y * 0.99f, rigidbody.velocity.z * 0.99f);
    }

    public void Drift()
    {
        WheelFrictionCurve w = wheelPairs[1].leftWheel.sidewaysFriction;
        w.extremumSlip = 2f;
        wheelPairs[1].leftWheel.sidewaysFriction = w;
        wheelPairs[1].rightWheel.sidewaysFriction = w;
    }

    public void nDrift()
    {
        WheelFrictionCurve w = wheelPairs[1].leftWheel.sidewaysFriction;
        w.extremumSlip = 0.2f;
        wheelPairs[1].leftWheel.sidewaysFriction = w;
        wheelPairs[1].rightWheel.sidewaysFriction = w;
    }

    public void Update()
    {
        if(rewind)
        {
            return;
        }

        if (rigidbody.velocity.x > 35)
        {
            rigidbody.velocity = new Vector3(35, rigidbody.velocity.y, rigidbody.velocity.z);
        }
        if (rigidbody.velocity.y > 35)
        {
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, 35, rigidbody.velocity.z);
        }
        if (rigidbody.velocity.z > 35)
        {
            rigidbody.velocity = new Vector3(rigidbody.velocity.x, rigidbody.velocity.y, 35);
        }

        if (Input.GetAxis("Drift") > 0)
            Drift();
        else
            nDrift();

        if (Input.GetKey (KeyCode.B))
            Boost();

        //if (Input.GetKey (KeyCode.J))
        //Jump();

        if (Input.GetKey(KeyCode.R))
            Reset();

        if (Input.GetAxis("Brake") > 0)
            Brake();

        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
        float brakeTorque = Mathf.Abs(Input.GetAxis("Jump") * 2);

        if (Input.GetAxis("Vertical") <= 0)
        {
            rigidbody.velocity = new Vector3(rigidbody.velocity.x * 0.99f, rigidbody.velocity.y, rigidbody.velocity.z * 0.99f);
        }

        if (brakeTorque > 0.001)
        {
            brakeTorque = maxMotorTorque;
            motor = 0;
        }
        else
        {
            brakeTorque = 0;
        }

        foreach (WheelPair wheelPair in wheelPairs)
        {
            if (wheelPair.steering)
                wheelPair.leftWheel.steerAngle = wheelPair.rightWheel.steerAngle = ((wheelPair.reverseTurn) ? -1 : 1) * steering;

            if (wheelPair.motor)
            {
                wheelPair.leftWheel.motorTorque = motor;
                wheelPair.rightWheel.motorTorque = motor;
            }

            wheelPair.leftWheel.brakeTorque = brakeTorque;
            wheelPair.rightWheel.brakeTorque = brakeTorque;

            UpdateWheel(wheelPair);
        }

    }

}