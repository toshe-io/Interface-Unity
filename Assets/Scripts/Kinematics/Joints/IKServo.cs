// TODO: Port to interface

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKServo : MonoBehaviour
{
    [SerializeField]
    private bool isEnabled = true;

    [SerializeField]
    private int id;

    [SerializeField]
    private bool SCServo;

    [SerializeField]
    private AnimationCurve easingCurve;

    [SerializeField]
    private float easingTime = 1.0f;
    
    [SerializeField]
    private int minPWM = 150;
    
    [SerializeField]
    private int maxPWM = 600;

    [SerializeField]
    private int currentPWM;

    [SerializeField]
    private bool invertedAngles;


    private IKJoint joint;

    private float servoAngle;
    private float jointAngle;

    private int prevServoPWM;
    private int servoPWM;

    private bool isMoving;

    private Protocol.MQTT.Events.SendEvent sendEvent;

    public class ServoMsg
    {
        public int id;
        public int pwm;
        public bool scservo;
    }


	// Use this for initialization
	void Start () {
		joint = GetComponent<IKJoint>();

        sendEvent = new Protocol.MQTT.Events.SendEvent();


        // TEMP
        Graph.YMin = maxPWM;
		Graph.YMax = minPWM;
 
		Graph.channel[ 0 ].isActive = true;
	}
	
	// Update is called once per frame
	void Update () {
        ShowInfo();

        updateJointAngle();

        if (didAnglesChange()) {
            updateServoAngle();
            updateServoPWM();
        }

        if (prevServoPWM != servoPWM) {
            updateServoHardware();
        }

        updateGraph();
	}

    void ShowInfo() {
        Lebug.Log(joint.name, servoAngle + ":" + servoPWM, "IKServo");
    }

    void updateJointAngle() {
        // Set joint pos
        jointAngle =  Helpers.Math.normalizeAngle(joint.getActiveAngle());
    }

    void updateServoAngle() {
        // Is servo already moving
        if (isMoving) {
            return;
        }

        // Check if Easing curve not set, if not just set servo angle to joint angle
        if (easingCurve.length < 2) {
            servoAngle = jointAngle;

            return;
        }

        isMoving = true;

        // Start moving servo
        StartCoroutine(EaseFloat(servoAngle, jointAngle, easingCurve, easingTime));  
    }

    IEnumerator EaseFloat(float f1, float f2, AnimationCurve ac, float time) {
        float timer = 0.0f;

        while (timer <= time) {
            servoAngle = Mathf.Lerp (f1, f2, ac.Evaluate(timer/time));
            timer += Time.deltaTime;

            yield return null;
        }

        isMoving = false;
    }

    bool didAnglesChange() {
        return !Mathf.Approximately(servoAngle, jointAngle);
    }

    void updateServoPWM() {
        float in_min = joint.minAngle;
        float in_max = joint.maxAngle;

        float out_min = minPWM;
        float out_max = maxPWM;

        // Account for inverted axis
        if (invertedAngles) {
            float temp_out_min = out_min;

            out_min = out_max;
            out_max = temp_out_min;
        }

        servoPWM = Mathf.RoundToInt(Helpers.Math.map(servoAngle, in_min, in_max, out_min, out_max));
    }

    void updateServoHardware() {
        ServoMsg msg = new ServoMsg();
        msg.id = id;
        msg.pwm = servoPWM;

        // We dont want to send unecessary bytes
        if (SCServo) {
            msg.scservo = SCServo;
        }

        sendEvent.topic = Protocol.MQTT.Topics.BIO1.LeftArm.SetServo;
        sendEvent.msg = JsonUtility.ToJson(msg);

        EventMessenger.Instance.Raise(sendEvent);

        prevServoPWM = servoPWM;

        //Graph.channel[ 0 ].Feed( servoPWM );
    }

    void updateGraph() {
    }
}