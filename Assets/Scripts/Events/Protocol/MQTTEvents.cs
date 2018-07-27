namespace Protocol.MQTT.Events {
    public class SendEvent : GameEvent
    {
        public string topic;
        public string msg;
    }
}