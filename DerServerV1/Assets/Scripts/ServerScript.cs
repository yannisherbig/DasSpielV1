using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using UnityEngine.UI;

public class Player
{
    public Player(GameObject playerObject, string username)
    {
        this.PlayerObject = playerObject;
        this.Username = username;
    }

    public GameObject PlayerObject { get; set; }
    public string Username { get; set; }
}

public class ServerScript : MonoBehaviour {

    // TcpListener zum abhören der Verbindungen
    TcpListener tcpListener;

    // Background-Thread für die Arbeit des Tcp-Listener 	
    private Thread tcpListenerThread;

    // Bool'sche Variable, um zu schauen, ob Tcp-Listener noch läuft 	
    private bool isRunning;

    // Referenz auf das Player-Prefab
    public GameObject playerObject;

    // Referenz auf das PickUp-Prefab
    public GameObject pickUp;

    // Dictionary zur Verwaltung der verbundenen Spieler
    public Dictionary<string, Player> players = new Dictionary<string, Player>();

    // Liste der Pick-Up-Objects
    private List<GameObject> pickUps = new List<GameObject>();

    // Referenz auf das Anzeigefeld, welches die Verbindungsinformationen vom Server darstellt, 
    // womit sich die Studenten mit ihrem Rechner zum Server verbinden können
    public Text lEndpointText;

    // Signalisiert den Spielbeginn
    public bool startButtonClicked;

    // Referenz auf das Hauptmenü-Modal
    public GameObject modalPanelObject;

    // Referenz auf PickUp-Checkbox
    public Toggle pickUpToggle;

    // public float speed = 5;

    IEnumerator Start() {
        Debug.Log("in Start");
        startButtonClicked = false;
        modalPanelObject.SetActive(true);
        while (!startButtonClicked)
        {
            yield return null;
        }

        Debug.Log("Button clicked, Toggle: " + pickUpToggle.isOn);
        modalPanelObject.SetActive(false);

   
        if (pickUpToggle.isOn)
        {
            SpawnPickUps();
        }

        isRunning = true;
        ThreadStart ts = new ThreadStart(StartListening);
        tcpListenerThread = new Thread(ts);
        tcpListenerThread.Start();
     
        Debug.Log("Thread.CurrentThread.ManagedThreadId (in Start()): " + Thread.CurrentThread.ManagedThreadId);

    }

    IEnumerator WaitForClickOnStart()
    {
        Debug.Log("in WaitForCLickOnStart");
        while (!startButtonClicked)
        {
            yield return null;
        }

        Debug.Log("Button clicked");
     

    }
    
    public void OnStartButtonClicked()
    {
        startButtonClicked = true;
    }

    void StartListening()
    {
        
        Debug.Log("Thread.CurrentThread.ManagedThreadId (in StartListening()): " + Thread.CurrentThread.ManagedThreadId);

        try
        {
            tcpListener = new TcpListener(IPAddress.Any, 0);
            
            tcpListener.Start();
            Debug.Log("Server started");
            TcpClient connectedClient = null;
            
            String localEndpointString = "Listening on: " + GetLocalIPAddress() + ":" + ((IPEndPoint)tcpListener.LocalEndpoint).Port.ToString();
            UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_initLocalEndpointText(localEndpointString));
            while (isRunning)
            {                            
                Debug.Log("Before client connected");
                connectedClient = tcpListener.AcceptTcpClient();
                Debug.Log("After client connected");
                
                Debug.Log("Aktuelle Spieleranzahl: " + players.Count);

                string ip = ((IPEndPoint)connectedClient.Client.RemoteEndPoint).Address.ToString();
                int clientPort = ((IPEndPoint)connectedClient.Client.RemoteEndPoint).Port;

                NetworkStream ns = null;

                try
                {
                    ns = connectedClient.GetStream();
                    if (ns.CanRead)
                    {
                        byte[] receivedBuffer = new byte[1024];
                        int numberOfBytesRead = 0;

       
                        int remaining = receivedBuffer.Length;
                      
                        Debug.Log("Before data read");
                        numberOfBytesRead = ns.Read(receivedBuffer, 0, receivedBuffer.Length);
                        Debug.Log("After data read");
                        Debug.Log("numberOfBytes read: " + numberOfBytesRead);
                        remaining -= numberOfBytesRead;
                
                        if (numberOfBytesRead <= 0)
                        {
                            throw new EndOfStreamException
                                (String.Format("End of stream reached with {0} bytes left to read", remaining));
                        }


                        StringBuilder msg = new StringBuilder();
                        int byteCounter = 0;
           
                        for (int i = 0; i < numberOfBytesRead; i++)
                        {
                            char zeichen = Convert.ToChar(receivedBuffer[i]);
                            byteCounter++;
                            Debug.Log("byte " + byteCounter + ": " + zeichen);
                            if (receivedBuffer[i].Equals(59))  // Bei einem Semikolon (59) ist die Nachricht zuende 
                            {
                                Debug.Log("; erreicht");
                                string message = msg.ToString();
                                HandleData(connectedClient, message, ip, clientPort);
                                if (message.Contains("DISCONNECT;"))
                                {
                                    ns.Close();
                                    connectedClient.Close();

                                }
                                msg = new StringBuilder();

                            }
                            else
                            {
                                msg.Append(zeichen.ToString());
                            }
                        }


                    }
                }
                catch (EndOfStreamException e)
                {
                    ns.Close();
                    connectedClient.Close();
                    Debug.Log(e);

                }
                catch (SocketException socketException)
                {
                    ns.Close();
                    connectedClient.Close();
                    Debug.Log("Socket exception: " + socketException);

                }
                catch (Exception ex)
                {
                    Debug.Log(ex.ToString());

                }
            }
                   
        }

        catch (Exception ex)
        {
            Debug.Log(ex.ToString());
            Debug.Log("Server stopped");
        }

        finally
        {
            isRunning = false;
            tcpListener.Stop();          
	    }
    }


    void HandleData(TcpClient connectedClient, string msgStr, string ip, int clientPort)
    {      

        Debug.Log("Receiving: " + msgStr + "  From: " + ip + ":" + clientPort + "; ThreadID: " + Thread.CurrentThread.ManagedThreadId);
        

        string[] splitData = msgStr.Split('|');

            switch (splitData[0])
            {
                case "CONNECT":
                    string username = splitData[1];
            
                    UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_AddNewPlayer(ip, username, clientPort));
                    break;

                case "SPAWN":
                    Debug.Log("SPAWN called for client with ip address " + ip);
                    UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_SpawnPlayer(ip));
                    break;

                case "MOVE":
                    int speed;
                    if (Int32.TryParse(splitData[1], out speed))
                        UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_Move(ip, speed));
                    else
                        Console.WriteLine("Invalid argument for speed. Conversion from string to int did not succeed");
                                  
                    break;

                case "ROTATE":
                    int angle;
                    if (Int32.TryParse(splitData[1], out angle))
                        UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_Rotate(ip, angle));
                    else
                        Console.WriteLine("Invalid argument for speed. Conversion from string to int did not succeed");
                    break;

                case "DRAW":
                    string color = splitData[1];                  
                    UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_DrawLine(ip, color));                  
                    break;

                case "STOP_DRAWING":
                    UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_StopDrawing(ip));
                    break;

                case "SHOOT":
                    UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_ShootBullet(ip));
                    break;

                case "STATUS":
                    UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_GetStatus(connectedClient, ip));                       
                    break;
                case "DIST":
                    UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_GetDistToWall(connectedClient, ip));
                    break;
                case "DELETE":
                        UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_DeletePlayer(ip));
                        break;

                case "DISCONNECT":
                    UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_DisconnectPlayer(ip));
                    break;
            }
   
    }



    void SpawnPickUps()
    {
        Debug.Log("in SpawnPickUps");
        for (int i = 0; i < 5; i++)
        {
            int spawnPointX = UnityEngine.Random.Range(-49, 49);
            int spawnPointZ = UnityEngine.Random.Range(-28, 27);
            Vector3 spawnPosition = new Vector3(spawnPointX, 0, spawnPointZ);

            GameObject newPickUp = Instantiate(pickUp, spawnPosition, Quaternion.identity);
            pickUps.Add(newPickUp);
        }

        Debug.Log("PickUps have been instantiated");
    }


    public IEnumerator ExecuteOnMainThread_AddNewPlayer(string ip, string username, int clientPort)
    {
        
        if (!players.ContainsKey(ip))
        {

            Debug.Log(username + " connected from " + ip + ":" + clientPort);

            int spawnPointX = UnityEngine.Random.Range(-20, 20);
            int spawnPointZ = UnityEngine.Random.Range(-20, 20);
            Vector3 spawnPosition = new Vector3(spawnPointX, 0, spawnPointZ);

            GameObject temp = Instantiate(playerObject, spawnPosition, Quaternion.identity);

            Player newPlayer = new Player(temp, username);
            players.Add(ip, newPlayer);
            Debug.Log("Client with ip address " + ip + " added to dictionary; players.count = " + players.Count);
        }
        yield return null;
    }

    

    public IEnumerator ExecuteOnMainThread_SpawnPlayer(string ip)
    {
        if (players.ContainsKey(ip))
        {
            if (!players[ip].PlayerObject.activeInHierarchy)
            {
                players[ip].PlayerObject.SetActive(true);
                string username = players[ip].Username;
                Debug.Log(username + " spawned on the field");
            }
        }
        else
        {
            Debug.Log("Player from " + ip + " trying to spawn, but has not connected yet");
            
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_ShootBullet(string ip)
    {
        if (players.ContainsKey(ip))
        {
            players[ip].PlayerObject.GetComponent<PlayerScript>().Fire();
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_GetStatus(TcpClient connectedClient, string ip)
    {
        if (players.ContainsKey(ip))
        {
            string serverMessage = "Your current speed is: " + players[ip].PlayerObject.GetComponent<Rigidbody>().velocity.magnitude + "\n, Your current score is: " + players[ip].PlayerObject.GetComponent<PlayerScript>().score + "\n, Your current health is at: " + players[ip].PlayerObject.GetComponent<Health>().currentHealth + "%\n";
            Debug.Log("serverMessage in getStatus: " + serverMessage);
            SendMessage(connectedClient, serverMessage);
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_GetDistToWall(TcpClient connectedClient, string ip)
    {
        if (players.ContainsKey(ip))
        {
            float dist = players[ip].PlayerObject.GetComponent<PlayerScript>().distToWallAhead;
       
            SendMessage(connectedClient, dist.ToString());
        }
        yield return null;
    }
    public IEnumerator ExecuteOnMainThread_initLocalEndpointText(string msg)
    {
        lEndpointText.text = msg;
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_DrawLine(string ip, string color)
    {
        if (players.ContainsKey(ip))
        {
            Material m = players[ip].PlayerObject.GetComponent<TrailRenderer>().material;

            switch (color)
            {
                case "Red":
                    m.color = new Color(255, 0, 0, 255);
                    break;
                case "Green":
                    m.color = new Color(0, 255, 0, 255);
                    break;
                case "Blue":
                    m.color = new Color(0, 0, 255, 255);
                    break;
                case "Black":
                    m.color = new Color(0, 0, 0, 255);
                    break;
                case "White":
                    m.color = new Color(255, 255, 255, 255);
                    break;
            }
            players[ip].PlayerObject.GetComponent<TrailRenderer>().time = 500;
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_StopDrawing(string ip)
    {
        if (players.ContainsKey(ip))
        {
            players[ip].PlayerObject.GetComponent<TrailRenderer>().time = 0;
        }
        yield return null;
    }
    public IEnumerator ExecuteOnMainThread_Move(string ip, int speed)
    {
        if (players.ContainsKey(ip))
        {
            //Rigidbody.MovePosition(Vector3) to set your new position
            Vector3 forwardVel = players[ip].PlayerObject.GetComponent<Transform>().forward * speed;
            Vector3 horizontalVel = players[ip].PlayerObject.GetComponent<Transform>().right * speed;

            players[ip].PlayerObject.GetComponent<Rigidbody>().velocity = forwardVel + horizontalVel;

            /*      float moveHorizontal = 0f;
                    float moveVertical = 0f;

                    direction = direction.ToUpper();
                    switch (direction)
                    {
                        case "UP":
                            moveVertical = 1f;
                            break;
                        case "DOWN":
                            moveVertical = -1f;
                            break;
                        case "LEFT":
                            moveHorizontal = -1f;
                            break;
                        case "RIGHT":
                            moveHorizontal = 1f;
                            break;
                    }

                    Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
                    players[ip].PlayerObject.GetComponent<Rigidbody>().velocity = movement * speed;
            */
            //string username = players[ip].Username;
            //Debug.Log(username + " moved position, speed: " + players[ip].PlayerObject.GetComponent<Rigidbody>().velocity.magnitude);
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_Rotate(string ip, int angle)
    {
        if (players.ContainsKey(ip))
        {
            players[ip].PlayerObject.transform.Rotate(new Vector3(0, angle, 0));
            UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_Move(ip, (int)players[ip].PlayerObject.GetComponent<Rigidbody>().velocity.magnitude));
            string username = players[ip].Username;
            //Debug.Log(username + " performed a rotation on the y-axis which is now at " + players[ip].PlayerObject.transform.rotation.y);
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_DeletePlayer(string ip)
    {
        if (players.ContainsKey(ip))
        {
            players[ip].PlayerObject.SetActive(false);
            string username = players[ip].Username;
            Debug.Log(username + " has been removed from the field");
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_DisconnectPlayer(string ip)
    {
        if (players.ContainsKey(ip))
        {
            string username = players[ip].Username;
            Destroy(players[ip].PlayerObject);
            players.Remove(ip);
            Debug.Log(username + " disconnected and has been removed from the field and dictionary");
        }
        yield return null;
    }

    private void SendMessage(TcpClient client, string serverMessage)
    {
        try
        {
            serverMessage = serverMessage + ";\n";
            Debug.Log("ServerMessage: " + serverMessage);
            Debug.Log("in SendMessage");
   
            NetworkStream stream = client.GetStream();
            if (stream.CanWrite)
            {

                Debug.Log("im if statement in SendMessage");
                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(serverMessage);
                            
                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                stream.Flush();
                //stream.Close();
                Debug.Log("message sent; message: " + serverMessage + " (nach dem flush in SendMessage)");
            }
            else
            {
                Debug.Log("Stream cannot write");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    void RemovePickUps()
    {
        Debug.Log("in RemovePickUps");

        foreach (var pickUp in pickUps)
        {
            Destroy(pickUp.gameObject);
        }
        pickUps.Clear();
        Debug.Log("PickUps have been removed from the field");
    }

    void RemoveAllPlayers()
    {
        Debug.Log("in RemoveAllPlayers");
        foreach (var player in players)
        {
            Destroy(player.Value.PlayerObject);
        }
        players.Clear();
        Debug.Log("All players have been removed from the field");
    }

    
    public void StopServer()
    {
        Debug.Log("in StopServer");
        OnApplicationQuit();
        
        Debug.Log("in StopServer after OnApplicationQuit");
        StartCoroutine(Start());
        Debug.Log("after Start() in StopServer");
    }

    public void StopListening()
    {
        isRunning = false;
    }

    public void QuitGame()
    {
        Debug.Log("in QuitGame");
        Application.Quit();
        Debug.Log("in QuitGame after Application.Quit");
    }

    void OnApplicationQuit()
    {
        Debug.Log("in OnApplicationQuit");
        StopListening();
        RemovePickUps();
        RemoveAllPlayers();
 
        try
        {
            tcpListenerThread.Join(500);
            tcpListener.Stop();
            Debug.Log("tcpListener stopped");
        }
        catch (NullReferenceException e)
        {
            Debug.Log(e);
        }
    }

}


