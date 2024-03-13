using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Old_Car_Controller : MonoBehaviour
{
    //Public Variables
    [Header("Wheel Colliders")]
    public List<WheelCollider> Front_Wheels; //The front wheels
    public List<WheelCollider> Back_Wheels; //The rear wheels

    [Header("Wheel Transforms")]
    public List<Transform> Front_Wheel_Transforms; //The front wheel transforms
    public List<Transform> Back_Wheel_Transforms; //The rear wheel transforms

    [Header("Wheel Transforms Rotations")]
    public List<Vector3> Front_Wheel_Rotation; //The front wheel rotation Vectors
    public List<Vector3> Back_Wheel_Rotation; //The rear wheel rotation Vectors


    [Header("Car Settings")]
    public float Motor_Torque = 400; //Motor torque for the car
    public float Max_Steer_Angle = 25f; //The Maximum Steer Angle for the front wheels
    public float  BrakeForce = 150f; //The brake force of the wheels
    public float Maximum_Speed; //The top speed of the car

    [Space(15)]

    public float handBrakeFrictionMultiplier = 2; //The handbrake friction multiplier
    private float handBrakeFriction  = 0.05f; //The handbrake friction
    public float tempo; //Tempo (don't edit this)

    [Header("Car States")]
    public bool Use_Car_States; //Use car states?
    public bool Car_Started; //Car stared?
    public KeyCode Car_Start_Key; //Key to start the car
    

    [Header("Drift Settings")]
    public bool Set_Drift_Settings_Automatically = true; //Set the drift setting automatically?
    public float Forward_Extremium_Value_When_Drifting; //Forward extremium value when drifting
    public float Sideways_Extremium_Value_When_Drifting; //Sideways extremium value when drifting
    

    [Space(15)]

    [Header("Particle System(s) Settings")]
    public bool Use_Particle_Systems; //Use the particle system(s)
    public ParticleSystem[] Car_Smoke_From_Silencer; //Sorry, couldn't think of a better name :P

    [Header("Scene Settings")]
    public bool Use_Scene_Settings; //Use scene setting(s)
    public KeyCode Scene_Reset_Key = KeyCode.R; //Scene reset key

    [Header("Other Settings")]
    public Transform Center_of_Mass; //Centre of mass of car
    public  float frictionMultiplier = 3f; //Friction Multiplier
    public Rigidbody Car_Rigidbody; //Car rigidbody

    [Header("Debug Values")]
    public float Car_Speed_KPH; //The car speed in KPH
    public float Car_Speed_MPH; //The car speed in MPH
    
    [Space(15)]

    public bool HeadLights_On; //Headlights on/off?

    //Debug Values in Int Form
    public int Car_Speed_In_KPH; //Car speed in KPH (integer form)
    public int Car_Speed_In_MPH; //Car speed in MPH (integer form)


    public bool isCarMoving()
    {
        return Car_Speed_KPH == 0;
    }

    public float getCurrentSpeedInKPH()
    {
        return Car_Speed_KPH;
    }
    public bool Is_Flying () //bool for if the car is flying or not
	{
		if (!Back_Wheels[0].isGrounded && !Front_Wheels[0].isGrounded) {
			return true;
		}
        return false;
	}

    //private Variables
    private Rigidbody rb; //The rb
    private float Brakes = 0f; //Brakes
    private WheelFrictionCurve  Wheel_forwardFriction, Wheel_sidewaysFriction; //Wheel friction curve(s)
    private float Next_Boost_Time; //Next boost time

    //Private Audio Variables
    private float pitch; //Pitch

    //Hidden Variables (not private, but hidden in inspector)
    [HideInInspector] public float currSpeed; //Current speed

    void Start(){
        //To Prevent The Car From Toppling When Turning Too Much
        rb = GetComponent<Rigidbody>(); //get rigidbody
        rb.centerOfMass = Center_of_Mass.localPosition; //Set the centre of mass of the rigid body to the centre of mass transform

        //Play Car Smoke Particle System
        if(Use_Particle_Systems){
            foreach(ParticleSystem P in Car_Smoke_From_Silencer){
                P.Play(); //Play the smoke from silencer particle system
            }
        }
    }

    public void FixedUpdate(){
        //Turning Car on
        if(Input.GetKeyDown(Car_Start_Key) && Use_Car_States){ //if the "use car states" is true and that the car start key is pressed
            Car_Started = true;
        }

        //If the car states are not in use
        if(!Use_Car_States){
            Car_Started = true;
        }

        //Applying Maximum Speed
        if(Car_Speed_In_KPH < Maximum_Speed && Car_Started){ //if the car's current speed is less than the maximum speed
            //Let car move forward and backward
            foreach(WheelCollider Wheel in Back_Wheels){
                Wheel.motorTorque = Input.GetAxis("Vertical") * ((Motor_Torque * 5)/(Back_Wheels.Count + Front_Wheels.Count));
            }
        }

        if(Car_Speed_In_KPH > Maximum_Speed && Car_Started){ //if the car's current speed is more than the top speed
            //Don't let the car accelerate anymore so it does not exceed the maximum speed
            foreach(WheelCollider Wheel in Back_Wheels){
                Wheel.motorTorque = 0;
            }
        }

        //Making The Car Turn/Steer
        if(Car_Started){
            foreach(WheelCollider Wheel in Front_Wheels){
                Wheel.steerAngle = Input.GetAxis("Horizontal") * Max_Steer_Angle; //Turn the wheels
            }
        }

        //Changing speed of the car
        Car_Speed_KPH = Car_Rigidbody.velocity.magnitude * 3.6f; //Calculate car speed in KPH
        Car_Speed_MPH = Car_Rigidbody.velocity.magnitude * 2.237f; //Calculate the car's speed in MPH

        Car_Speed_In_KPH = (int) Car_Speed_KPH; //Convert the float values of the speed to int
        Car_Speed_In_MPH = (int) Car_Speed_MPH; //Convert the float values of the speed to int

        //Make Car Drift
        WheelHit wheelHit;

        foreach(WheelCollider Wheel in Back_Wheels){
            Wheel.GetGroundHit(out wheelHit);

            if(wheelHit.sidewaysSlip < 0 )	
                tempo = (1 + -Input.GetAxis("Horizontal")) * Mathf.Abs(wheelHit.sidewaysSlip *handBrakeFrictionMultiplier);

                if(tempo < 0.5) tempo = 0.5f;

            if(wheelHit.sidewaysSlip > 0 )	
                tempo = (1 + Input.GetAxis("Horizontal") )* Mathf.Abs(wheelHit.sidewaysSlip *handBrakeFrictionMultiplier);

                if(tempo < 0.5) tempo = 0.5f;

            if(wheelHit.sidewaysSlip > .99f || wheelHit.sidewaysSlip < -.99f){
                //handBrakeFriction = tempo * 3;
                float velocity = 0;
                handBrakeFriction = Mathf.SmoothDamp(handBrakeFriction,tempo* 3,ref velocity ,0.1f * Time.deltaTime);
                }

            else{
                handBrakeFriction = tempo;
            }
        }

        foreach(WheelCollider Wheel in Front_Wheels){
            Wheel.GetGroundHit(out wheelHit);

            if(wheelHit.sidewaysSlip < 0 )	
                tempo = (1 + -Input.GetAxis("Horizontal")) * Mathf.Abs(wheelHit.sidewaysSlip *handBrakeFrictionMultiplier);

                if(tempo < 0.5) tempo = 0.5f;

            if(wheelHit.sidewaysSlip > 0 )	
                tempo = (1 + Input.GetAxis("Horizontal") )* Mathf.Abs(wheelHit.sidewaysSlip *handBrakeFrictionMultiplier);

                if(tempo < 0.5) tempo = 0.5f;

            if(wheelHit.sidewaysSlip > .99f || wheelHit.sidewaysSlip < -.99f){
                //handBrakeFriction = tempo * 3;
                float velocity = 0;
                handBrakeFriction = Mathf.SmoothDamp(handBrakeFriction,tempo* 3,ref velocity ,0.1f * Time.deltaTime);
                }

            else{
                handBrakeFriction = tempo;
            }
        }
    }

    public void Update(){
        //Scene Settings
        if(Use_Scene_Settings){
            if(Input.GetKeyDown(Scene_Reset_Key)){ //When the reset key is pressed
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); //Restart the current scene
            }
        }

        //Rotating The Wheels Meshes so they have the same position and rotation as the wheel colliders
        var pos = Vector3.zero; //position value (temporary)
        var rot = Quaternion.identity; //rotation value (temporary)
        
        for (int i = 0; i < (Back_Wheels.Count); i++)
        {
            Back_Wheels[i].GetWorldPose(out pos, out rot); //get the world rotation & position of the wheel colliders
            Back_Wheel_Transforms[i].position = pos; //Set the wheel transform positions to the wheel collider positions
            Back_Wheel_Transforms[i].rotation = rot * Quaternion.Euler(Back_Wheel_Rotation[i]); //Rotate the wheel transforms to the rotation of the wheel collider(s) and the rotation offset
        }

        for (int i = 0; i < (Front_Wheels.Count); i++)
        {
            Front_Wheels[i].GetWorldPose(out pos, out rot); //get the world rotation & position of the wheel colliders
            Front_Wheel_Transforms[i].position = pos; //Set the wheel transform positions to the wheel collider positions
            Front_Wheel_Transforms[i].rotation = rot * Quaternion.Euler(Front_Wheel_Rotation[i]); //Rotate the wheel transforms to the rotation of the wheel collider(s) and the rotation offset
        }

        //Make Car Brake
        if(Input.GetKey(KeyCode.Space) && Car_Started){
            Brakes = BrakeForce;

            //Drifting and changing wheel collider values
            if(Set_Drift_Settings_Automatically){
                foreach(WheelCollider Wheel in Back_Wheels){
                    Wheel_forwardFriction = Wheel.forwardFriction;
                    Wheel_sidewaysFriction = Wheel.sidewaysFriction;

                    Wheel_forwardFriction.extremumValue = Wheel_forwardFriction.asymptoteValue = ((currSpeed * frictionMultiplier) / 300) + 1;
                    Wheel_sidewaysFriction.extremumValue = Wheel_sidewaysFriction.asymptoteValue = ((currSpeed * frictionMultiplier) / 300) + 1;
                }

                foreach(WheelCollider Wheel in Front_Wheels){
                    Wheel_forwardFriction = Wheel.forwardFriction;
                    Wheel_sidewaysFriction = Wheel.sidewaysFriction;

                    Wheel_forwardFriction.extremumValue = Wheel_forwardFriction.asymptoteValue = ((currSpeed * frictionMultiplier) / 300) + 1;
                    Wheel_sidewaysFriction.extremumValue = Wheel_sidewaysFriction.asymptoteValue = ((currSpeed * frictionMultiplier) / 300) + 1;
                }
            }

            if(!Set_Drift_Settings_Automatically){
                foreach(WheelCollider Wheel in Back_Wheels){
                    //Variables getting assigned
                    Wheel_forwardFriction = Wheel.forwardFriction;
                    Wheel_sidewaysFriction = Wheel.sidewaysFriction;

                    //Setting The Extremium values to the ones that the user defined
                    Wheel_forwardFriction.extremumValue = Forward_Extremium_Value_When_Drifting;
                    Wheel_sidewaysFriction.extremumValue = Sideways_Extremium_Value_When_Drifting;
                }

                foreach(WheelCollider Wheel in Front_Wheels){
                    //Variables getting assigned
                    Wheel_forwardFriction = Wheel.forwardFriction;
                    Wheel_sidewaysFriction = Wheel.sidewaysFriction;

                    //Setting The Extremium values to the ones that the user defined
                    Wheel_forwardFriction.extremumValue = Forward_Extremium_Value_When_Drifting;
                    Wheel_sidewaysFriction.extremumValue = Sideways_Extremium_Value_When_Drifting;
                }
            }
        }

        else{
            Brakes = 0f;
        }

        //Apply brake force
        foreach(WheelCollider Wheel in Front_Wheels){
            Wheel.brakeTorque = Brakes; //set the brake torque of the wheels to the brake torque
        }

        foreach(WheelCollider Wheel in Back_Wheels){
            Wheel.brakeTorque = Brakes; //set the brake torque of the wheels to the brake torque
        }
    }
    
}