using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Car_Controller : MonoBehaviour
{
    [Header("Wheel Colliders")]
    [SerializeField]
    private List<WheelCollider> Front_Wheels;

    [SerializeField]
    private List<WheelCollider> Back_Wheels;

    [Header("Wheel Transforms")]
    [SerializeField]
    private List<Transform> Front_Wheel_Transforms;

    [SerializeField]
    private List<Transform> Back_Wheel_Transforms;

    [Header("Wheel Transforms Rotations")]
    [SerializeField]
    private List<Vector3> Front_Wheel_Rotation;

    [SerializeField]
    private List<Vector3> Back_Wheel_Rotation;

    [Header("Car Settings")]
    [SerializeField]
    private float Motor_Torque = 400;

    [SerializeField]
    private float Max_Steer_Angle = 25f;

    [SerializeField]
    private float Maximum_Speed;

    [Space(15)]
    private float tempo; //wird von Wheel_Effects benutzt

    [Header("Particle System(s) Settings")]
    private bool Use_Particle_Systems; //Use the particle system(s)
    private ParticleSystem[] Car_Smoke_From_Silencer;

    [Header("Other Settings")]
    [SerializeField]
    private Transform Center_of_Mass;

    [SerializeField]
    private Rigidbody Car_Rigidbody;

    private float Car_Speed_KPH; //The car speed in KPH

    private int Car_Speed_In_KPH; //Car speed in KPH (integer form)
    private Rigidbody rb; //The rb

    private WheelFrictionCurve Wheel_forwardFriction,
        Wheel_sidewaysFriction;
    private float currSpeed;
    private int currentDirection;

    public void setCar_Speed_KPH(float speedInKph)
    {
        Car_Speed_KPH = speedInKph;
    }

    public Rigidbody Rb
    {
        get => rb;
        set => rb = value;
    }

    public bool isCarMoving()
    {
        float movementThreshold = 0.2f;

        return rb.velocity.magnitude > movementThreshold;
    }

    public float getCurrentSpeedInKPH()
    {
        return Car_Speed_KPH;
    }

    public float getTempo()
    {
        return tempo;
    }

    public bool Is_Flying() //bool for if the car is flying or not
    {
        if (!Back_Wheels[0].isGrounded && !Front_Wheels[0].isGrounded)
        {
            return true;
        }
        return false;
    }

    private void Start()
    {
        //To Prevent The Car From Toppling When Turning Too Much
        rb = GetComponent<Rigidbody>(); //get rigidbody
        rb.centerOfMass = Center_of_Mass.localPosition; //Set the centre of mass of the rigid body to the centre of mass transform

        //Play Car Smoke Particle System
        if (Use_Particle_Systems)
        {
            foreach (ParticleSystem P in Car_Smoke_From_Silencer)
            {
                P.Play(); //Play the smoke from silencer particle system
            }
        }
    }

    private void FixedUpdate()
    {
        //var accelValue = Input.GetAxis("Vertical");
        //var steeringValue = Input.GetAxis("Horizontal");
        //MoveCar(steeringValue,accelValue);
    }

    private void Update()
    {
        RotateWheels();
    }

    private void RotateWheels()
    {
        var pos = Vector3.zero; //position value (temporary)
        var rot = Quaternion.identity; //rotation value (temporary)

        for (var i = 0; i < (Back_Wheels.Count); i++)
        {
            Back_Wheels[i].GetWorldPose(out pos, out rot); //get the world rotation & position of the wheel colliders
            Back_Wheel_Transforms[i].position = pos; //Set the wheel transform positions to the wheel collider positions
            Back_Wheel_Transforms[i].rotation = rot * Quaternion.Euler(Back_Wheel_Rotation[i]); //Rotate the wheel transforms to the rotation of the wheel collider(s) and the rotation offset
        }

        for (var i = 0; i < (Front_Wheels.Count); i++)
        {
            Front_Wheels[i].GetWorldPose(out pos, out rot); //get the world rotation & position of the wheel colliders
            Front_Wheel_Transforms[i].position = pos; //Set the wheel transform positions to the wheel collider positions
            Front_Wheel_Transforms[i].rotation = rot * Quaternion.Euler(Front_Wheel_Rotation[i]); //Rotate the wheel transforms to the rotation of the wheel collider(s) and the rotation offset
        }
    }

    private void CheckDirection()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(Car_Rigidbody.velocity);
        if ((int)localVelocity.z > 0)
        {
            // Das Fahrzeug bewegt sich vorwärts
            currentDirection = 1;
        }
        else if ((int)localVelocity.z < 0)
        {
            // Das Fahrzeug bewegt sich rückwärts
            currentDirection = -1;
        }
        else
        {
            // Das Fahrzeug steht still
            currentDirection = 0;
        }
    }

    public void MoveCar(float steeringValue, float accelValue)
    {
        //Debug.Log("test " + "steeringValue " + steeringValue + "accelValue " + accelValue);
        CheckDirection();

        // Überprüfe, ob die Richtung geändert wurde
        bool directionChanged =
            accelValue != 0
            && currentDirection != 0
            && Math.Sign(accelValue) != Math.Sign(currentDirection);

        // Wenn die Richtung geändert wurde oder die maximale Geschwindigkeit überschritten ist
        if (directionChanged || Car_Speed_In_KPH > Maximum_Speed)
        {
            // Bremskraft hinzufügen, um das Auto schneller zu stoppen
            foreach (WheelCollider wheel in Back_Wheels)
            {
                wheel.brakeTorque = Mathf.Abs(accelValue) * Motor_Torque;
            }
        }
        else
        {
            // Keine Bremskraft ansonsten
            foreach (WheelCollider wheel in Back_Wheels)
            {
                wheel.brakeTorque = 0f;
            }
        }

        if (Car_Speed_In_KPH < Maximum_Speed)
        {
            // Lasse das Auto vorwärts und rückwärts fahren
            foreach (WheelCollider wheel in Back_Wheels)
            {
                wheel.motorTorque =
                    accelValue * ((Motor_Torque * 5) / (Back_Wheels.Count + Front_Wheels.Count));
            }
        }

        if (Car_Speed_In_KPH > Maximum_Speed)
        {
            // Verhindere weiteres Beschleunigen, um die maximale Geschwindigkeit nicht zu überschreiten
            foreach (WheelCollider wheel in Back_Wheels)
            {
                wheel.motorTorque = 0;
            }
        }

        foreach (WheelCollider wheel in Front_Wheels)
        {
            wheel.steerAngle = steeringValue * Max_Steer_Angle; // Lenke die Räder
        }

        Car_Speed_KPH = Car_Rigidbody.velocity.magnitude * 3.6f;
        Car_Speed_In_KPH = (int)Car_Speed_KPH;
    }
}
