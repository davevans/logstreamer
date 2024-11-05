import {useEffect, useState} from 'react';
import './App.css';
import {
  HubConnectionBuilder,
} from '@microsoft/signalr';

export type NewMessage = {
  message: string
}

function App() {

  const [messages, setMessages] = useState<string[]>([]);
  const [statusMessage, setStatusMessage] = useState<string>();
  const [connectionStatus, setConnectionStatus] = useState<string>();


  useEffect(() => {

    const getSignalRConnection = async () => {
      const connection = new HubConnectionBuilder()
        .withUrl('http://localhost:5121/logs')
        .withAutomaticReconnect()
        .build();

      var requestObj =  {
        nameSpace:"default", 
        podName:"logproducer-5bdcbc74b-22p4k",
        containerName:"logproducer",
        previous: true
      }

      connection.on("ReceiveLogMessage", (newMessage: NewMessage) => {
        console.log("Receiving log line...")
        setMessages((existingItems) => [...existingItems, newMessage.message] );
      })

      connection.on("ReceiveStatusMessage", m => {
        setStatusMessage(m);
      })

      connection.onclose(() => {
        setConnectionStatus(connection.state);
      })

      connection.onreconnecting(() => {
        setConnectionStatus(connection.state);
      })

      connection.onreconnected(() => {
        setConnectionStatus(connection.state);
      })

      await connection.start().then(x => setStatusMessage(""))
      
      setConnectionStatus(connection.state);
      setMessages([]);  
      await connection.invoke("GetLogs", requestObj).catch(console.error);
    }

    getSignalRConnection().catch(console.error);
    
  }, []);

  return (
    <div>
      <div>status: {statusMessage}</div>
      <div>connection status: {connectionStatus}</div>
      <ul>
        {messages.map((m,i) => <li key={i}>{m}</li>)}
      </ul>
    </div>
  );
}

export default App;
