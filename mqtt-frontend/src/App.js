import React from "react";
import MqttSubscriber from "./components/MqttSubscriber";

function App() {
  const brokerUrl = "ws://broker.hivemq.com:8000/mqtt";
  const topic = "test/topic";

  return (
    <div>
      <h1>Mqtt Subscriber</h1>
      <MqttSubscriber brokerUrl={brokerUrl} topic={topic} />
    </div>
  );
}

export default App;
