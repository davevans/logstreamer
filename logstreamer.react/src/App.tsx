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


  useEffect(() => {

    const getSignalRConnection = async () => {
      const connection = new HubConnectionBuilder()
        .withUrl('http://localhost:5121/logs')
        .withAutomaticReconnect()
        .build();

      var requestObj =  {
        nameSpace:"default", 
        podName:"logproducer",
        containerName:"logproducer",
      }

      

      //setup handler for arriving messages
      connection.on("ReceiveLogMessages", (newMessages: NewMessage[]) => {
        
        console.log("Receiving broadcast...")

        var messageBodies = newMessages.map(x => x.message);
        setMessages((existingItems) => [...existingItems, ...messageBodies] );
      })

      connection.on("ReceiveLogMessage", (newMessage: NewMessage) => {
        console.log("Receiving log line...")
        setMessages((existingItems) => [...existingItems, newMessage.message] );
      })

      await connection.start()
      setMessages([]);  
      await connection.invoke("GetLogs", requestObj).catch(console.error);
    }

    getSignalRConnection().catch(console.error);
    
  }, []);

  return (
    <div>
      <ul>
        {messages.map((m,i) => <li key={i}>{m}</li>)}
      </ul>
    </div>
  );
}

export default App;
