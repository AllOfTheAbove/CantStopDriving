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

public class Vehicle : NetworkBehaviour
{
	public List<WheelPair> wheelPairs;
	public float maxMotorTorque;
	public float maxSteeringAngle;
	
    public GameObject obj;
	new private Transform transform;
	new private Rigidbody rigidbody;

    private void Start()
    {
        transform = obj.transform;
        rigidbody = obj.GetComponent<Rigidbody>();

        obj = GameObject.FindGameObjectsWithTag("Driver")[0];
        gameObject.transform.parent = obj.transform;
        obj.GetComponent<Driver>().vehicle = gameObject;
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
	
	public void Reset()
	{
		transform.position = new Vector3(0, 0, 0);
        transform.rotation = new Quaternion (0, 0, 0, 0);
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
	}

	public void Jump()
	{
        rigidbody.velocity = new Vector3(rigidbody.velocity.x, 10, rigidbody.velocity.z);
	}

	public void Boost()
	{
        rigidbody.velocity = new Vector3 (rigidbody.velocity.x * 1.01f, rigidbody.velocity.y * 1.01f, rigidbody.velocity.z * 1.01f);
	}

	public void Brake()
	{
        rigidbody.velocity = new Vector3 (rigidbody.velocity.x * 0.999f, rigidbody.velocity.y * 0.999f, rigidbody.velocity.z * 0.999f);
	}

	public void Drift()
	{
		WheelFrictionCurve w = wheelPairs[1].leftWheel.sidewaysFriction;
		w.extremumSlip = 2f;
		wheelPairs[1].leftWheel.sidewaysFriction  = w;
		wheelPairs[1].rightWheel.sidewaysFriction = w;
	}
	
	public void nDrift()
	{
		WheelFrictionCurve w = wheelPairs[1].leftWheel.sidewaysFriction;
		w.extremumSlip = 0.2f;
		wheelPairs[1].leftWheel.sidewaysFriction  = w;
		wheelPairs[1].rightWheel.sidewaysFriction = w;
	}
	
	public void Update()
	{
		if (Input.GetKey (KeyCode.LeftShift))
			Drift();
		else
			nDrift();
		
        if (Input.GetKey (KeyCode.B))
			//Boost();
		
		if (Input.GetKey (KeyCode.J))
			//Jump();
		
		if (Input.GetKey (KeyCode.R))
			Reset();

        if (Input.GetKey (KeyCode.Space))
			Brake();

        float motor = maxMotorTorque * Input.GetAxis("Vertical");
		float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
		float brakeTorque = Mathf.Abs(Input.GetAxis("Jump") * 2);

		if (!Input.GetKey(KeyCode.UpArrow))
		{
            rigidbody.velocity = new Vector3 (rigidbody.velocity.x * 0.995f, rigidbody.velocity.y * 0.995f, rigidbody.velocity.z * 0.995f);
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
				wheelPair.leftWheel.steerAngle = wheelPair.rightWheel.steerAngle = ((wheelPair.reverseTurn)?-1:1)*steering;

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