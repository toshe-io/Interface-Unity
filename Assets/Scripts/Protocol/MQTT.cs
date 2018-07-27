using UnityEngine;
using System.Collections;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;

using System;

public class MQTT : MonoBehaviour {

    [SerializeField]
    private String brokerIP = "127.0.0.1";
    
    [SerializeField]
    private int brokerPort = 1883;

    [SerializeField]
    private String clientId;

    [SerializeField]
    private int publishRatePerSec;

    private int msgPerSecAllowance;
    private float msgPerSecTimer;

    private int currentPublishRate;


    private MqttClient client;

    // Use this for initialization
    void Start () {
        // create client instance 
        client = new MqttClient(IPAddress.Parse(brokerIP), brokerPort, false , null ); 
        
        // register to message received 
        client.MqttMsgPublishReceived += receive; 
        
        String guid = clientId + "-" + Guid.NewGuid().ToString().Substring(0, 5); 
        client.Connect(guid); 
        
        subscribe("status/connect");

        publish("status/connect", guid);
    }

    void Update() {
        ShowStats();
    }

    void ShowStats() {
        Lebug.Log("Message Rate/s", currentPublishRate, "MQTT");
    }

    bool canPublish() {
        float time_passed = Time.time - msgPerSecTimer;
        
        currentPublishRate = (publishRatePerSec - msgPerSecAllowance);

        if (time_passed >= 1) {
            msgPerSecAllowance = publishRatePerSec;
            msgPerSecTimer = Time.time;
        }

        if (msgPerSecAllowance > 0) {
            msgPerSecAllowance--;
            return true;
        }
        
        return false;
    }

    void publish(String topic, String msg) {
        if (canPublish()) {
            client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
        }
    }

    void subscribe(String topic) {
        client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE }); 
    }

    void receive(object sender, MqttMsgPublishEventArgs e) 
    { 
        Debug.Log("Received: [" +  e.Topic + "] " + System.Text.Encoding.UTF8.GetString(e.Message)  );
    }

    // Event handler
    void OnEnable ()
    {
        EventMessenger.Instance.AddListener<Protocol.MQTT.Events.SendEvent>(SendEvent);
    }

    void OnDisable ()
    {
        EventMessenger.Instance.RemoveListener<Protocol.MQTT.Events.SendEvent>(SendEvent);
    }

    void SendEvent(Protocol.MQTT.Events.SendEvent e)
    {
        publish(e.topic, e.msg);
    }
}