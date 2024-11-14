import React, { useEffect, useState } from "react";
import mqtt from "mqtt";

const MqttSubscriber = ({ brokerUrl, topic }) => {
  const [messages, setMessages] = useState([]);

  useEffect(() => {
    const client = mqtt.connect(brokerUrl);

    client.on("connect", () => {
      console.log("Connected to MQTT broker");
      client.subscribe(topic, (err) => {
        if (err) {
          console.log("Error subscribing to topic", err);
        } else {
          console.log(`Subscribed to topic ${topic}`);
        }
      });
    });

    client.on("message", (topic, message) => {
      setMessages((prevMessages) => [...prevMessages, message.toString()]);
    });

    return () => {
      client.end();
    };
  }, [brokerUrl, topic]);

  return (
    <div>
      <h2>MQTT Messages</h2>
      <ul>
        {messages.map((message, index) => (
          <li key={index}>
            {index}: {message}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default MqttSubscriber;
