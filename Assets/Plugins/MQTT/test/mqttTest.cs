using UnityEngine;
using System.Collections;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;

using System;

public class mqttTest : MonoBehaviour {
    [SerializeField]
    private String brokerIP = "127.0.0.1";
    
    [SerializeField]
    private int brokerPort = 1883;

    private String clientId;

	private MqttClient client;
	// Use this for initialization
	void Start () {
		// create client instance 
		client = new MqttClient(IPAddress.Parse(brokerIP), brokerPort, false , null ); 
		
		// register to message received 
		client.MqttMsgPublishReceived += receive; 
		
		clientId = Guid.NewGuid().ToString(); 
		client.Connect(clientId); 
		
		subscribe("status/connect");

        publish("status/connect", clientId);
	}

    void publish(String topic, String msg) {
        client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(msg), MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, false);
    }

    void subscribe(String topic) {
        client.Subscribe(new string[] { topic }, new byte[] { MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE }); 
    }

	void receive(object sender, MqttMsgPublishEventArgs e) 
	{ 

		Debug.Log("Received: " + System.Text.Encoding.UTF8.GetString(e.Message)  );
	} 

	// Update is called once per frame
	void Update () {
	}
}
