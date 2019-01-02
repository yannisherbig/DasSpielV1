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
    public int PlayModelNum { get; set; }
}
 

public class ServerScript : MonoBehaviour {

    // TcpListener zum abhören der Verbindungen
    TcpListener tcpListener;

    // Background-Thread für die Arbeit des Tcp-Listener 	
    private Thread tcpListenerThread;

    // Bool'sche Variable, um zu schauen, ob Tcp-Listener noch läuft 	
    private bool isRunning;

    // Referenz auf das Player-Prefab
    //public GameObject playerObject;
    public GameObject[] playerObjectModels;
 
    // Referenz auf die PickUp-Prefabs
    public GameObject pickUp, pickUpCopy, pickUpGolden, pickUpPoisonous;

    // Dictionary zur Verwaltung der verbundenen Spieler
    public Dictionary<string, Player> players = new Dictionary<string, Player>();

    // Liste der Pick-Up-Objects
    //private List<GameObject> pickUps = new List<GameObject>();

    // Referenz auf das Anzeigefeld, welches die Verbindungsinformationen vom Server darstellt, 
    // womit sich die Studenten mit ihrem Rechner zum Server verbinden können
    public Text lEndpointText, top3Text, highscoreText;
    public int highScore;

    // Signalisiert den Spielbeginn
    public bool startButtonClicked;

    // Referenz auf das Hauptmenü-Modal
    public GameObject modalPanelObject;

    // Referenz auf PickUp-Checkbox
    public Toggle pickUpToggle;

    // public float speed = 5;
    int playerModelNumber;

    // Referenz auf die Spieler mit den meisten Punkten
    Player p1stScore, p2ndScore, p3rdScore;

    // Layer-Maske für Physics.OverlapSphere()
    int layerMask;

    IEnumerator Start() {
        startButtonClicked = false;
        modalPanelObject.SetActive(true);
        while (!startButtonClicked)
        {
            yield return null;
        }
        // Es wird nur die Player-Layer benötigt
        layerMask = 1 << 11;
  
        modalPanelObject.SetActive(false);

        isRunning = true;
        if (pickUpToggle.isOn)
        {
            InvokeRepeating("SpawnStandardPickUp", 0, 5);
            InvokeRepeating("SpawnPoisonousPickUp", 6, 12);
            InvokeRepeating("SpawnGoldenPickUp", 29, 29);
        }

        // Die Auskommentierung der unteren beiden Zeilen rückgängig machen, um einen Dummy-Player-Objekt für Testzwecke zu haben
        //UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_AddNewPlayer(new TcpClient(), "999.999.999.999", "Dummy Dummyman", 99999));
        //UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_SpawnPlayer("999.999.999.999"));

        ThreadStart ts = new ThreadStart(StartListening);
        tcpListenerThread = new Thread(ts);
        tcpListenerThread.Start();
           
    }

    private void Update()
    {
        if (players.Count > 0)
        {
            foreach (var p in players)
            {
                Player cp = p.Value;
                if (p1stScore == null || cp.PlayerObject.GetComponent<PlayerScript>().score > p1stScore.PlayerObject.GetComponent<PlayerScript>().score)
                {
                    p1stScore = cp;
                    if (cp == p2ndScore)
                        p2ndScore = null;                   
                }
                else if (players.Count > 1 && (p2ndScore == null || cp.PlayerObject.GetComponent<PlayerScript>().score > p2ndScore.PlayerObject.GetComponent<PlayerScript>().score) && (cp != p1stScore))
                {
                    p2ndScore = cp;
                    if (cp == p3rdScore)
                        p3rdScore = null;
                }
                else if (players.Count > 2 && (p3rdScore == null || cp.PlayerObject.GetComponent<PlayerScript>().score > p3rdScore.PlayerObject.GetComponent<PlayerScript>().score) && (cp != p1stScore) && (cp != p2ndScore))
                {
                    if (cp != p1stScore && cp != p3rdScore)
                        p3rdScore = cp;
                }
            }

            if (p2ndScore == null && p3rdScore == null)
            {
                top3Text.text = "<b>Top 3</b>\n<size=50>1st: " + p1stScore.Username + " (" + p1stScore.PlayerObject.GetComponent<PlayerScript>().score + "p)</size>";
            }
            else if (p3rdScore == null)
            {
                top3Text.text = "<b>Top 3</b>\n<size=50>1st: " + p1stScore.Username + " (" + p1stScore.PlayerObject.GetComponent<PlayerScript>().score + "p)</size>"
                    + "\n<size=50>2nd: " + p2ndScore.Username + " (" + p2ndScore.PlayerObject.GetComponent<PlayerScript>().score + "p)</size>";
            }
            else if (p1stScore != null && p2ndScore != null && p3rdScore != null)
            {
                top3Text.text = "<b>Top 3</b>\n<size=50>1st: " + p1stScore.Username + " (" + p1stScore.PlayerObject.GetComponent<PlayerScript>().score + "p)</size>"
                + "\n<size=50>2nd: " + p2ndScore.Username + " (" + p2ndScore.PlayerObject.GetComponent<PlayerScript>().score + "p)</size>"
                + "\n<size=50>3rd: " + p3rdScore.Username + " (" + p3rdScore.PlayerObject.GetComponent<PlayerScript>().score + "p)</size>";
            }
        }
        
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
                
            case "MOW":
                UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_StartMowing(ip));
                break;

            case "STOP_MOWING":
                UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_StopMowing(ip));
                break;

            case "DIR_VECTOR":
                UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_GetDirVector(connectedClient, ip));
                break;

            case "SURR_PLAYERS":
                int radius;
                if (Int32.TryParse(splitData[1], out radius))
                    UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_GetSurroundingPlayers(connectedClient, ip, radius));
                else
                    Debug.Log("Invalid argument for radius. Conversion from string to int did not succeed");
                break;

            case "SURR_PICKUPS":
                int rad;
                if (Int32.TryParse(splitData[1], out rad))
                    UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_GetSurroundingPickUps(connectedClient, ip, rad));
                else
                    Debug.Log("Invalid argument for radius. Conversion from string to int did not succeed");
                break;

            case "DELETE":
                    UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_DeletePlayer(ip));
                    break;

            case "DISCONNECT":
                UnityMainThreadDispatcher.Instance().Enqueue(ExecuteOnMainThread_DisconnectPlayer(ip));
                break;
            }
   
    }

    void SpawnStandardPickUp()
    {
        int spawnPointX = UnityEngine.Random.Range(-49, 49);
        int spawnPointZ = UnityEngine.Random.Range(-28, 27);
        Vector3 spawnPosition = new Vector3(spawnPointX, 4, spawnPointZ);
        if (pickUp.activeInHierarchy)
        {
            pickUpCopy.transform.position = spawnPosition;
            pickUpCopy.SetActive(true);
            pickUpCopy.GetComponent<PickUpScript>().isActive = true;
            StartCoroutine(SetInactive(pickUpCopy, 9.9f));
        }
        else if (pickUpCopy.activeInHierarchy)
        {
            pickUp.transform.position = spawnPosition;
            pickUp.SetActive(true);
            pickUp.GetComponent<PickUpScript>().isActive = true;
            StartCoroutine(SetInactive(pickUp, 9.9f));
        }
        else
        {
            pickUp.transform.position = spawnPosition;
            pickUp.SetActive(true);
            pickUp.GetComponent<PickUpScript>().isActive = true;
            StartCoroutine(SetInactive(pickUp, 9.9f));
        }
        //GameObject newPickUp = Instantiate(pickUp, spawnPosition, pickUp.transform.rotation);

        //Destroy(newPickUp, 10);
        //pickUps.Add(newPickUp);   
    }

    void SpawnPoisonousPickUp()
    {
        int spawnPointX = UnityEngine.Random.Range(-49, 49);
        int spawnPointZ = UnityEngine.Random.Range(-28, 27);
        Vector3 spawnPosition = new Vector3(spawnPointX, 1, spawnPointZ);
        pickUpPoisonous.transform.position = spawnPosition;
        pickUpPoisonous.SetActive(true);
        pickUpPoisonous.GetComponent<PickUpScript>().isActive = true;
        //GameObject newPickUp = Instantiate(pickUpPoisonous, spawnPosition, pickUp.transform.rotation);
        StartCoroutine(SetInactive(pickUpPoisonous, 9.9f));
        //Destroy(newPickUp, 10);
        //pickUps.Add(newPickUp);   
    }

    void SpawnGoldenPickUp()
    {
        int spawnPointX = UnityEngine.Random.Range(-49, 49);
        int spawnPointZ = UnityEngine.Random.Range(-28, 27);
        Vector3 spawnPosition = new Vector3(spawnPointX, 6, spawnPointZ);
        pickUpGolden.transform.position = spawnPosition;
        pickUpGolden.SetActive(true);
        pickUpGolden.GetComponent<PickUpScript>().isActive = true;
        //GameObject newPickUp = Instantiate(pickUpGolden, spawnPosition, pickUp.transform.rotation);
        StartCoroutine(SetInactive(pickUpGolden, 10));
        //Destroy(newPickUp, 10);
        //pickUps.Add(newPickUp);   
    }

    IEnumerator SetInactive(GameObject go, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        go.SetActive(false);
    }

    public IEnumerator ExecuteOnMainThread_StartMowing(string ip)
    {
        if (players.ContainsKey(ip))
        {
            players[ip].PlayerObject.GetComponent<PlayerScript>().mowing = true;
        }
        else
        {
            Debug.Log("Player from " + ip + " trying to mow, but has not connected yet");
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_StopMowing(string ip)
    {
        if (players.ContainsKey(ip))
        {
            players[ip].PlayerObject.GetComponent<PlayerScript>().mowing = false;
        }
        else
        {
            Debug.Log("Player from " + ip + " trying to stop mowing, but has not connected yet");
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_AddNewPlayer(TcpClient client, string ip, string username, int clientPort)
    {       
        if (!players.ContainsKey(ip))
        {
            string msg = "";
            if(username.Length > 18)
            {
                //msg = "{\"error\": {\"message\": \"Nutzername hat zu viele Zeichen\"}}";
                msg = "Dein Nutzername hat zu viele Zeichen";
                SendMessage(client, msg);
                yield break;
            }
            Debug.Log(username + " connected from " + ip + ":" + clientPort);
            bool freeSpawnPosFound = false;
            /*
             * Collider-Array dient dem Scannen der Umgebung des Spieler-Objektes.
             * Zum einen damit sicher gestellt ist, dass diese nicht aufeinander spawnen
             * und zum anderen, um dem Spieler zu ermöglichen, alle Spieler in einem bestimmten 
             * Radius abzufragen
             */
            Collider[] collidersInSpawnPos;
            // Radius für Scannen der Umgebung
            float radius = 4;

            // Zähle die Anzahl der Versuche eine freie Spawn-Position zu finden
            int loopCounter = 0;

            Vector3 spawnPosition = new Vector3(0, 1.03f, 0);
            while (!freeSpawnPosFound)
            {
                loopCounter++;
                if (loopCounter > 20)
                {
                    Debug.Log("Kein Platz mehr auf Spielfeld");
                    yield break;  // return
                }
                // Sich im Umgebungsradius befindlichen Collider in Array schreiben
                collidersInSpawnPos = Physics.OverlapSphere(spawnPosition, radius, layerMask);

                // Wenn Array nicht leer ist, dann ist mind. ein Collider im Weg
                if (collidersInSpawnPos.Length != 0)
                {
                    int spawnPointX = UnityEngine.Random.Range(-20, 20);
                    int spawnPointZ = UnityEngine.Random.Range(-7, 7);
                    spawnPosition = new Vector3(spawnPointX, 1.03f, spawnPointZ);

                    continue;  // Schleifendurchlauf neu starten
                }
                else
                {
                    freeSpawnPosFound = true;
                }                            
             
            }
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
            temp.GetComponent<Health>().highScoreText = highscoreText;
            string playerModelName = playerObjectModels[playerModelNumber].name;
            string playerModel = playerModelName.Substring(9, playerModelName.Length - 9);
            Player newPlayer = new Player(temp, username);
            players.Add(ip, newPlayer);
            players[ip].PlayModelNum = playerModelNumber;
            players[ip].PlayerObject.GetComponent<PlayerScript>().nameTag.GetComponent<TextMesh>().text = username;
            players[ip].PlayerObject.GetComponent<PlayerScript>().playerIP = ip;
            players[ip].PlayerObject.GetComponent<Health>().serverScript = gameObject;
            players[ip].PlayerObject.GetComponent<Health>().highScoreText = highscoreText;
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
                if(players[ip].PlayerObject.GetComponent<Health>().currentHealth < 0)
                {
                    players[ip].PlayerObject.GetComponent<Health>().currentHealth = 100;
                    RectTransform healthBar = players[ip].PlayerObject.GetComponent<Health>().healthBar;
                    healthBar.sizeDelta = new Vector2(100, healthBar.sizeDelta.y);
                    bool freeSpawnPosFound = false;
                    /*
                     * Collider-Array dient dem Scannen der Umgebung des Spieler-Objektes.
                     * Zum einen damit sicher gestellt ist, dass diese nicht aufeinander spawnen
                     * und zum anderen, um dem Spieler zu ermöglichen, alle Spieler in einem bestimmten 
                     * Radius abzufragen
                     */
                    Collider[] collidersInSpawnPos;
                    // Radius für Scannen der Umgebung
                    float radius = 4;

                    // Zähle die Anzahl der Versuche eine freie Spawn-Position zu finden
                    int loopCounter = 0;

                    Vector3 spawnPosition = new Vector3(0, 1.03f, 0);
                    while (!freeSpawnPosFound)
                    {
                        loopCounter++;
                        if (loopCounter > 20)
                        {
                            Debug.Log("Kein Platz mehr auf Spielfeld");
                            yield break;  // return
                        }
                        // Sich im Umgebungsradius befindlichen Collider in Array schreiben
                        collidersInSpawnPos = Physics.OverlapSphere(spawnPosition, radius, layerMask);

                        // Wenn Array nicht leer ist, dann ist mind. ein Collider im Weg
                        if (collidersInSpawnPos.Length != 0)
                        {
                            int spawnPointX = UnityEngine.Random.Range(-20, 20);
                            int spawnPointZ = UnityEngine.Random.Range(-7, 7);
                            spawnPosition = new Vector3(spawnPointX, 1.03f, spawnPointZ);

                            continue;  // Schleifendurchlauf neu starten
                        }
                        else
                        {
                            freeSpawnPosFound = true;
                        }

                    }
                    players[ip].PlayerObject.GetComponent<Transform>().position = spawnPosition;
                }              
                players[ip].PlayerObject.SetActive(true);
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
            players[ip].PlayerObject.GetComponent<PlayerScript>().Fire();
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

    public IEnumerator ExecuteOnMainThread_GetDistToWall(TcpClient connectedClient, string ip)
    {
        if (players.ContainsKey(ip))
        {
            float dist = players[ip].PlayerObject.GetComponent<PlayerScript>().distToWallAhead;
       
            SendMessage(connectedClient, dist.ToString());
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_GetDirVector(TcpClient connectedClient, string ip)
    {
        if (players.ContainsKey(ip))
        {
            Vector3 directionVector = players[ip].PlayerObject.GetComponent<PlayerScript>().directionVector;
            string msg = "{\"x\": " + directionVector.x + ", \"y\": " + directionVector.y + ", \"z\": " + directionVector.z + "}";
            SendMessage(connectedClient, msg);
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_GetSurroundingPlayers(TcpClient connectedClient, string ip, float radius)
    {
        Debug.Log("in GetSurroundings");
        if (players.ContainsKey(ip))
        {
            string msg = "[";
            Collider[] colliders = players[ip].PlayerObject.GetComponent<PlayerScript>().GetSurroundingPlayers(radius);
            Debug.Log("colliders.Length: " + colliders.Length);
            Vector3 surPlayerPos;
            foreach (var col in colliders)
            {
                Debug.Log("col.name: " + col.name);
                Debug.Log("col.gameObject.transform.parent.gameObject.name: " + col.gameObject.transform.parent.gameObject.name);
                foreach (var player in players)
                {
                    Debug.Log("player.Value.Username: " + player.Value.Username);
                    if (player.Value.PlayerObject == col.gameObject.transform.parent.gameObject)
                    {
                        surPlayerPos = col.gameObject.transform.parent.gameObject.transform.position;
                        Debug.Log("even deeper in GetSurroundings");
                        if (col == colliders[colliders.Length - 1])
                        {
                            msg += "{\"name\": \"" + player.Value.Username + "\", \"position\" : {\"x\": " + surPlayerPos.x + ", \"y\": " + surPlayerPos.y + ", \"z\": " + surPlayerPos.z + "}}]";
                            //msg += player.Value.Username;
                        }
                        else
                        {
                            msg += "{\"name\": \"" + player.Value.Username + "\", \"position\" : {\"x\": " + surPlayerPos.x + ", \"y\": " + surPlayerPos.y + ", \"z\": " + surPlayerPos.z + "}}, ";
                            //msg += player.Value.Username + ", ";
                        }                   
                        break;
                    }
                }
            }
            if(colliders.Length == 0)
            {
                msg = "Es befinden sich keine Spieler-Objekte im spezifiziertem Umkreis";
            }
            SendMessage(connectedClient, msg);
        }
        yield return null;
    }

    public IEnumerator ExecuteOnMainThread_GetSurroundingPickUps(TcpClient connectedClient, string ip, float radius)
    {
        Debug.Log("in GetSurroundings");
        if (players.ContainsKey(ip))
        {
            string msg = "[";
            Vector3 surPickUpPos;
            Collider[] colliders = players[ip].PlayerObject.GetComponent<PlayerScript>().GetSurroundingPickUps(radius);
            Debug.Log("colliders.Length: " + colliders.Length);
            foreach (var col in colliders)
            {
                Debug.Log("col.name: " + col.name);
                surPickUpPos = col.gameObject.transform.position;
                if (col == colliders[colliders.Length - 1])
                {
                    msg += "{\"name\": \"" + col.name + "\", \"position\": {\"x\": " + surPickUpPos.x + ", \"y\": " + surPickUpPos.y + ", \"z\": " + surPickUpPos.z + "}}]";
                    //msg += player.Value.Username;
                }
                else
                {
                    msg += "{\"name\": \"" + col.name + "\", \"position\": {\"x\": " + surPickUpPos.x + ", \"y\": " + surPickUpPos.y + ", \"z\": " + surPickUpPos.z + "}}, ";
                    //msg += player.Value.Username + ", ";
                }
            }
            if (colliders.Length == 0)
            {
                msg = "Es befinden sich keine Pick-Ups im spezifiziertem Umkreis";
            }
            SendMessage(connectedClient, msg);
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
            players[ip].PlayerObject.GetComponent<TrailRenderer>().time = 30;
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
        p1stScore = null;
        p2ndScore = null;
        p3rdScore = null;
        top3Text.text = "";
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

}


