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
using UnityEngine.SceneManagement;

public class Player2
{
    public Player2(GameObject playerObject, string username)
    {
        this.PlayerObject = playerObject;
        this.Username = username;
    }

    public GameObject PlayerObject { get; set; }
    public string Username { get; set; }
    public int PlayModelNum { get; set; }
}
 

public class ServerScript2 : MonoBehaviour {

    // TcpListener zum abhören der Verbindungen
    TcpListener tcpListener;

    // Background-Thread für die Arbeit des Tcp-Listener 	
    private Thread tcpListenerThread;

    // Bool'sche Variable, um zu schauen, ob Tcp-Listener noch läuft 	
    private bool isRunning;

    // Referenz auf das Player-Prefab
    //public GameObject playerObject;
    public GameObject[] playerObjectModels;
 

    // Dictionary zur Verwaltung der verbundenen Spieler
    public Dictionary<string, Player> players = new Dictionary<string, Player>();

  

    // Referenz auf das Anzeigefeld, welches die Verbindungsinformationen vom Server darstellt, 
    // womit sich die Studenten mit ihrem Rechner zum Server verbinden können
    public Text lEndpointText;
    // Signalisiert den Spielbeginn
    public bool startButtonClicked;

    // Referenz auf das Hauptmenü-Modal
    public GameObject modalPanelObject;


    // public float speed = 5;
    int playerModelNumber;


    // Material für TrailRenderer
    public Material trailMaterial;

    IEnumerator Start() {
        startButtonClicked = false;
        modalPanelObject.SetActive(true);
        while (!startButtonClicked)
        {
            yield return null;
        }
  
        modalPanelObject.SetActive(false);

        isRunning = true;

        //// Die Auskommentierung der unteren beiden Zeilen rückgängig machen, um einen Dummy-Player-Objekt für Testzwecke zu haben
        //UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_AddNewPlayer(new TcpClient(), "999.999.999.999", "DUMMYDUMMYMANNNN", 99999));
        //UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_SpawnPlayer("999.999.999.999"));

        //// Ein zweites Dummy-Player-Objekt:
        //UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_AddNewPlayer(new TcpClient(), "999.999.999.111", "DUMMYDUMMYMANNN2", 222222));
        //UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_SpawnPlayer("999.999.999.111"));

        //// Ein drittes Dummy-Player-Objekt:
        //UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_AddNewPlayer(new TcpClient(), "999.999.999.222", "DUMMYDUMMYMANNN3", 333333));
        //UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_SpawnPlayer("999.999.999.222"));

        ThreadStart ts = new ThreadStart(StartListening);
        tcpListenerThread = new Thread(ts);
        tcpListenerThread.Start();
           
    }

    private void Update()
    {
        

            
    }

    IEnumerator WaitForClickOnStart()
    {
        while (!startButtonClicked)
        {
            yield return null;
        }
     
    }
    
    public void OnStartButtonClicked()
    {
        startButtonClicked = true;
    }

    void StartListening()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Any, 5555);
            
            tcpListener.Start();
            //Debug.Log("Server started");
            TcpClient connectedClient = null;
            
            String localEndpointString = "Listening on: <b>" + GetLocalIPAddress() + ":" + ((IPEndPoint)tcpListener.LocalEndpoint).Port.ToString() + "</b>";
            UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_initLocalEndpointText(localEndpointString));
            while (isRunning)
            {                            
                //Debug.Log("Before client connected");
                connectedClient = tcpListener.AcceptTcpClient();
                //Debug.Log("After client connected");
                
                //Debug.Log("Aktuelle Spieleranzahl: " + players.Count);

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
                      
                        //Debug.Log("Before data read");
                        numberOfBytesRead = ns.Read(receivedBuffer, 0, receivedBuffer.Length);
                        //Debug.Log("After data read");
                        //Debug.Log("numberOfBytes read: " + numberOfBytesRead);
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
                            //Debug.Log("byte " + byteCounter + ": " + zeichen);
                            if (receivedBuffer[i].Equals(59))  // Bei einem Semikolon (59) ist die Nachricht zuende 
                            {
                                //Debug.Log("; erreicht");
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
                UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_AddNewPlayer(connectedClient, ip, username, clientPort));
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
                    Debug.Log("Invalid argument for speed. Conversion from string to int did not succeed");
                break;

            case "ROTATE":
                float angle;
                angle = float.Parse(splitData[1]);
                Debug.Log(angle);
                UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_Rotate(ip, angle));            
                break;

                /*
            case "DRAW":
                string color = splitData[1];                  
                UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_DrawLine(ip, color));                  
                break;

            case "STOP_DRAWING":
                UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_StopDrawing(ip));
                break;
                */
            case "SHOOT":
                UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_ShootBullet(ip));
                break;

            case "STATUS":
                UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_GetStatus(connectedClient, ip));                       
                break;
            

            case "DIR_VECTOR":
                UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_GetDirVector(connectedClient, ip));
                break;

            case "DELETE":
                    UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_DeletePlayer(ip));
                    break;

            case "DISCONNECT":
                UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_DisconnectPlayer(ip));
                break;

            case "Angle":
                UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_Angle(connectedClient,ip));
                    break;

            case "WALL":
                UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_DistanzWand(connectedClient, ip));
                break;

            case "TIPP":
                UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_Tipp(connectedClient,ip));
                break;
            
        }               
   
    }

    IEnumerator SetInactive(GameObject go, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        go.SetActive(false);
    }


    public IEnumerator ExecuteOnMainThread_AddNewPlayer(TcpClient client, string ip, string username, int clientPort)
    {       
        if (!players.ContainsKey(ip))
        {
            string msg = "";
            if(username.Length > 16)
            {
                //msg = "{\"error\": {\"message\": \"Nutzername hat zu viele Zeichen\"}}";
                msg = "Dein Nutzername darf nicht mehr als 16 Zeichen haben";
                SendMessage(client, msg);
                yield break;
            }
            Debug.Log(username + " connected from " + ip + ":" + clientPort);
            /*
             * Collider-Array dient dem Scannen der Umgebung des Spieler-Objektes.
             * Zum einen damit sicher gestellt ist, dass diese nicht aufeinander spawnen
             * und zum anderen, um dem Spieler zu ermöglichen, alle Spieler in einem bestimmten 
             * Radius abzufragen
             */

            Vector3 spawnPosition = new Vector3(-6.3f, 3, 10);
          
            if(players.Count == playerObjectModels.Length)
            {
                //msg = "{\"error\": {\"message\": \"Maximale Spieleranzahl erreicht\"}}";
                msg = "Maximale Spieleranzahl erreicht";
                SendMessage(client, msg);
                yield break;
            }
            // Dem Spieler ein Spieler-Modell zuweisen
            if (players.Count > 0)
            {
                while (true)
                {
                    foreach (var player in players)
                    {
                        if (player.Value.PlayModelNum == playerModelNumber)
                        {
                            playerModelNumber++;
                            break;
                        }
                        else
                        {
                            goto endOfLoop;
                        }

                    }

                }
            }
            endOfLoop:
            GameObject temp = Instantiate(playerObjectModels[playerModelNumber], spawnPosition, Quaternion.identity);
            string playerModelName = playerObjectModels[playerModelNumber].name;
            string playerModel = playerModelName.Substring(9, playerModelName.Length - 9 - 1);
            Player newPlayer = new Player(temp, username);
            players.Add(ip, newPlayer);
            players[ip].PlayModelNum = playerModelNumber;
            players[ip].PlayerObject.GetComponent<PlayerScript2>().nameTag.GetComponent<TextMesh>().text = username;
            players[ip].PlayerObject.GetComponent<PlayerScript2>().playerIP = ip;
            players[ip].PlayerObject.GetComponent<Health2>().serverScript = gameObject;
            playerModelNumber++;
            Debug.Log("Client with ip address " + ip + " added to dictionary; players.count = " + players.Count);
            //string msg = "{\"success\": {\"message\": \"Verbindung erfolgreich hergestellt\", \"player_model\": \"" + playerModel + "\"}}";
            msg = "Verbindung erfolgreich hergestellt! Player-Model: " + playerModel;
            SendMessage(client, msg);
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_SpawnPlayer(string ip)
    {
        if (players.ContainsKey(ip))
        {
            if (!players[ip].PlayerObject.activeInHierarchy)
            {
                if(players[ip].PlayerObject.GetComponent<Health2>().currentHealth <= 0)
                {
                    players[ip].PlayerObject.GetComponent<Health2>().currentHealth = 100;
                    RectTransform healthBar = players[ip].PlayerObject.GetComponent<Health2>().healthBar;
                    healthBar.sizeDelta = new Vector2(100, healthBar.sizeDelta.y);
                    /*
                     * Collider-Array dient dem Scannen der Umgebung des Spieler-Objektes.
                     * Zum einen damit sicher gestellt ist, dass diese nicht aufeinander spawnen
                     * und zum anderen, um dem Spieler zu ermöglichen, alle Spieler in einem bestimmten 
                     * Radius abzufragen
                     */
                    Vector3 spawnPosition = new Vector3(-6.3f, 3, 10);
                    //Vector3 spawnPosition = new Vector3(-6.3f, 3, 9.2f);

                    players[ip].PlayerObject.GetComponent<Transform>().position = spawnPosition;
                    players[ip].PlayerObject.GetComponent<Transform>().rotation = Quaternion.identity; 
                }              
                players[ip].PlayerObject.SetActive(true);
                foreach (var mr in players[ip].PlayerObject.GetComponentsInChildren<MeshRenderer>())
                {
                    StartCoroutine(players[ip].PlayerObject.GetComponent<Health2>().FadeTo(mr.material, 1f, 2f)); // Start a coroutine to fade the material to 1.0 alpha over 2 seconds and disable the GameObject
                }
                string username = players[ip].Username;
                Debug.Log(username + " spawned on the field; Player-Model: " + playerObjectModels[players[ip].PlayModelNum].name.Substring(9));
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
            players[ip].PlayerObject.GetComponent<PlayerScript2>().Fire();
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_GetStatus(TcpClient connectedClient, string ip)
    {
        if (players.ContainsKey(ip))
        {
            Transform t = players[ip].PlayerObject.GetComponent<Transform>();
            string svrMsg = "{\"score\": " + players[ip].PlayerObject.GetComponent<PlayerScript>().score
                + ", \"health\": \"" + players[ip].PlayerObject.GetComponent<Health>().currentHealth + "%\""
                + ", \"velocity\": " + players[ip].PlayerObject.GetComponent<Rigidbody>().velocity.magnitude 
                + ", \"position\": {\"x\": " + t.position.x + ", \"y\": " + t.position.y + ", \"z\": " + t.position.z + "}"
                + ", \"rotation\": {\"x\": " + t.rotation.x + ", \"y\": " + t.rotation.y + ", \"z\": " + t.rotation.z + "}}";

            SendMessage(connectedClient, svrMsg);
        }
        yield return null;
    }

    //Das Kopieren
    public IEnumerator ExecuteOnMainThread_GetDistToWall(TcpClient connectedClient, string ip)
    {
        if (players.ContainsKey(ip))
        {
            float dist = players[ip].PlayerObject.GetComponent<PlayerScript2>().distToWallAhead;
       
            SendMessage(connectedClient, dist.ToString());
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_GetDirVector(TcpClient connectedClient, string ip)
    {
        if (players.ContainsKey(ip))
        {
            Vector3 directionVector = players[ip].PlayerObject.GetComponent<PlayerScript2>().directionVector;
            string msg = "{\"x\": " + directionVector.x + ", \"y\": " + directionVector.y + ", \"z\": " + directionVector.z + "}";
            SendMessage(connectedClient, msg);
        }
        yield return null;
    }

   public IEnumerator ExecuteOnMainThread_Tipp(TcpClient connectedClient, string ip)
    {
        if (players.ContainsKey(ip))
        {
            string msg = players[ip].PlayerObject.GetComponent<PlayerScript2>().status;
            SendMessage(connectedClient, msg);
        }
        yield return null;
    }




    public IEnumerator ExecuteOnMainThread_DrawLine(string ip, string color)
    {
        if (players.ContainsKey(ip))
        {
            TrailRenderer tr = players[ip].PlayerObject.GetComponent<PlayerScript2>().trailRendererPos.GetComponent<TrailRenderer>();
            tr.Clear();
            //Material m = tr.material;
            Material m = trailMaterial; 
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
            tr.material = m;
            tr.startWidth = 0.2f;
            tr.endWidth = 0.2f;
            tr.time = 30;
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_StopDrawing(string ip)
    {
        if (players.ContainsKey(ip))
        {
            players[ip].PlayerObject.GetComponent<PlayerScript2>().trailRendererPos.GetComponent<TrailRenderer>().time = 0;
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
            //Vector3 movement ;
            
            //movement.Set(horizontalMove, 0.0f, verticalMove);
            //players[ip].PlayerObject.GetComponent<Rigidbody>().AddForce(forwardVel + horizontalVel, ForceMode.Acceleration);
            //players[ip].PlayerObject.GetComponent<Rigidbody>().AddForce(-horizontalVel, ForceMode.Impulse);
            // players[ip].PlayerObject.GetComponent<Rigidbody>().MovePosition(transform.position + forwardVel + horizontalVel);
            // Vector3 horizontalVel = players[ip].PlayerObject.GetComponent<Transform>().Translate(forwardVel + horizontalVel);
            players[ip].PlayerObject.GetComponent<Rigidbody>().velocity = horizontalVel + forwardVel;

        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_Rotate(string ip, float angle)
    {
        if (players.ContainsKey(ip))
        {
            //Rigidbody rb = players[ip].PlayerObject.GetComponent<Rigidbody>();
            //Set the axis the Rigidbody rotates in (100 in the y axis)
            //Vector3 eulerAngleVelocity = new Vector3(0, angle, 0);
            //Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * Time.deltaTime * 50);
            //rb.MoveRotation(rb.rotation * deltaRotation);
            players[ip].PlayerObject.transform.Rotate(new Vector3(0, angle, 0));


           // UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_Move(ip, (int)players[ip].PlayerObject.GetComponent<Rigidbody>().velocity.magnitude));

            string username = players[ip].Username;
            //Debug.Log(username + " performed a rotation on the y-axis which is now at " + players[ip].PlayerObject.transform.rotation.y);
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_DeletePlayer(string ip)
    {
        if (players.ContainsKey(ip))
        {
            HandleRemoveOfPlayer(ip);
            string username = players[ip].Username;
            Debug.Log(username + " has been removed from the field");
        }
        yield return null;
    }

    public void HandleRemoveOfPlayer(string ip)
    {
        if (players.ContainsKey(ip))
        {
            Player po = players[ip];
            players[ip].PlayerObject.SetActive(false);
            players[ip].PlayerObject.GetComponent<Health2>().currentHealth = 0;
        }
    }

    public IEnumerator ExecuteOnMainThread_DisconnectPlayer(string ip)
    {
            HandleRemoveOfPlayer(ip);
            string username = players[ip].Username;
            Destroy(players[ip].PlayerObject);
            players.Remove(ip);
            Debug.Log(username + " disconnected and has been removed from the field and dictionary");
        
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_initLocalEndpointText(string msg)
    {
        lEndpointText.text = msg;
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_Angle (TcpClient connectedClient, string ip)
    {
        float angle = players[ip].PlayerObject.GetComponent<PlayerScript2>().Angle();
        string msg = angle.ToString();
        SendMessage(connectedClient, msg);
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_DistanzWand(TcpClient connectedClient, string ip)
    {

       float distance = players[ip].PlayerObject.GetComponent<PlayerScript2>().Distance();
        string msg = distance.ToString();
        SendMessage(connectedClient, msg);
        yield return null;

    }



    private void SendMessage(TcpClient client, string serverMessage)
    {
        try
        {
            serverMessage = serverMessage + ";\n";
            Debug.Log("ServerMessage: " + serverMessage);
       
            NetworkStream stream = client.GetStream();
            if (stream.CanWrite)
            {
                byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(serverMessage);
                            
                stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);
                stream.Flush();
                //stream.Close();
                //Debug.Log("message sent; message: " + serverMessage + " (nach dem flush in SendMessage)");
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

    //void RemovePickUps()
    //{
    //    Debug.Log("in RemovePickUps");

    //    foreach (var pickUp in pickUps)
    //    {
    //        Destroy(pickUp.gameObject);
    //    }
    //    pickUps.Clear();
    //    Debug.Log("PickUps have been removed from the field");
    //}

    void RemoveAllPlayers()
    {
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
        StopListening();
        //RemovePickUps();
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

    public void SwitchScene()
    {
        var currentScene = SceneManager.GetActiveScene();
        var currentSceneName = currentScene.name;
        if (currentSceneName.Equals("MainScene"))
            SceneManager.LoadScene("Labyrinth");
        else
            SceneManager.LoadScene("MainScene");
    }
}


