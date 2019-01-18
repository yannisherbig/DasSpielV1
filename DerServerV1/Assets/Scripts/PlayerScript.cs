using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour {

    public float raycastDistance = 400;
    GameObject collidingPlayerObject;
    public GameObject slightForwardPoint;
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletSpeed;	
	public float distToWallAhead;
    public int score;
    public bool mowing;
    RaycastHit hit;
    Ray landingRay;
    string wallTag = "Wall";
    public Terrain terrain;
    public GameObject nameTag;
    public Vector3 directionVector;
    protected int[,,] TerrainBackup;
    public string playerIP;
    public Material grassMaterial;
    public GameObject trailRendererPos;

    void Start () {
        GetComponent<Rigidbody>().inertiaTensorRotation = Quaternion.identity;
        terrain = Terrain.activeTerrain;
        //GetComponent<TrailRenderer>().transform.position = new Vector3(transform.position.x - 2, transform.position.y - 0.3f, transform.position.z - 2);
    }

    void Update()
    {
        //if (mowing)
        //{
        //    //CutGrass(terrain, transform.position, 4);
        //    //DetailMapCutoff(terrain, 0);
        //    StartTheMow();
        //}


        //transform.eulerAngles = new Vector3(0, transform.rotation.y, 0);
        //transform.rotation.Set(0, transform.rotation.y, 0, transform.rotation.w);
        //Debug.Log("1  " + transform.rotation.x + " ; " + transform.rotation.z + " ; " + transform.rotation.z);
        //transform.Rotate(new Vector3(0, transform.rotation.y, 0));
        directionVector = slightForwardPoint.transform.position - transform.position;
        Vector3 directionVector2 = Vector3.Normalize(slightForwardPoint.transform.position - transform.position);
        landingRay = new Ray(transform.position, directionVector2);  
        //Debug.DrawRay(transform.position, Vector3.Normalize(slightForwardPoint.transform.position - transform.position) * raycastDistance);
        if (Physics.Raycast(landingRay, out hit, raycastDistance))
        {
            if (hit.collider.gameObject.tag.Equals(wallTag))
            {
                distToWallAhead = hit.distance;
            }
        }
    }

    public void StartTheMow()
    {
        //TrailRenderer tr = GetComponent<TrailRenderer>();
        TrailRenderer tr = trailRendererPos.GetComponent<TrailRenderer>();
        tr.Clear();
        //Vector3 newPos = GetComponent<Transform>().forward + GetComponent<Transform>().right;
        //newPos = newPos(newPos.x)
        //tr.transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        Debug.Log("trM: " + tr.material);
        tr.material = grassMaterial;
        Debug.Log("trM: " + tr.material);
        tr.startWidth = 0.9f;
        tr.endWidth = 0.9f;
        tr.time = 30;
    }

    public void StopTheMow()
    {
        trailRendererPos.GetComponent<TrailRenderer>().time = 0;
    }

    private Vector3 ConvertToSplatMapCoordinate(Vector3 playerPos)
    {
        float PrPxSize = terrain.terrainData.detailResolution / terrain.terrainData.size.x;

        Vector3 TexturePoint3D = playerPos - terrain.transform.position;
        TexturePoint3D = TexturePoint3D * PrPxSize;
        Debug.Log(PrPxSize);
        Debug.Log(TexturePoint3D);

        Vector3 vecRet = new Vector3();
        Vector3 terPosition = terrain.transform.position;
        vecRet.x = ((playerPos.x - terPosition.x) / terrain.terrainData.size.x) * terrain.terrainData.detailWidth;
        Debug.Log("ter.terrainData.alphamapWidth: " + terrain.terrainData.alphamapWidth);
        Debug.Log("ter.terrainData.alphamapHeight: " + terrain.terrainData.alphamapHeight);
        vecRet.z = ((playerPos.z - terPosition.z) / terrain.terrainData.size.z) * terrain.terrainData.detailHeight;
        return vecRet;
    }

    // Set all pixels in a detail map below a certain threshold to zero.
    void DetailMapCutoff(Terrain t, int threshold)
    {
        //RaycastHit hit;
        //Ray ray = Camera.main.ScreenPointToRay(transform.position);
        //Debug.Log("ray: " + ray.ToString());
        //Vector2 uvTextureCoordinate1, uvTextureCoordinate2;
        //if (Physics.Raycast(ray, out hit, 1000))
        //{
        //    uvTextureCoordinate1 = hit.textureCoord;
        //    uvTextureCoordinate2 = hit.textureCoord2;

        //    Debug.Log("uvTextureCoordinate1.x: " + uvTextureCoordinate1.x + "uvTextureCoordinate1.y: " + uvTextureCoordinate1.y);
        //    Debug.Log("uvTextureCoordinate2.x: " + uvTextureCoordinate2.x + "uvTextureCoordinate2.y: " + uvTextureCoordinate2.y);
        //}



        // read all detail layers into a 3D int array:
        int numDetails = t.terrainData.detailPrototypes.Length;
        numDetails = 50;

        Debug.Log("detailHeight: " + t.terrainData.detailHeight);
        Debug.Log("detailWidth: " + t.terrainData.detailWidth);
        Debug.Log("t.transform.position: " + t.transform.position);
        Vector3 worldPos = transform.position;
        Vector3 test = ConvertToSplatMapCoordinate(worldPos);
        Debug.Log("test: " + test.x + " " + test.z);
        Vector3 terrainLocalPos = new Vector3();
        terrainLocalPos.x = (worldPos.x - t.transform.position.x) / t.terrainData.size.x;
        terrainLocalPos.z = (worldPos.z - t.transform.position.z) / t.terrainData.size.z;
        Debug.Log("t.GetPosition(): " + t.GetPosition());
        Debug.Log("worldPos: " + worldPos);
        Debug.Log("terrainLocalPos: " + terrainLocalPos);
        Debug.Log("t.terrainData.size.x: " + t.terrainData.size.x);
        Debug.Log("t.terrainData.size.y: " + t.terrainData.size.z);

        Vector2 normalizedPos = new Vector2(Mathf.InverseLerp(0.0f, t.terrainData.size.x, terrainLocalPos.x),
            Mathf.InverseLerp(0.0f, t.terrainData.size.z, terrainLocalPos.z));
        Debug.Log("normalizedPos before multiplication: " + normalizedPos);
        Debug.Log("normalizedPos.x before multiplication: " + normalizedPos.x);
        Debug.Log("normalizedPos.y before multiplication: " + normalizedPos.y);
        Debug.Log("t.terrainData.detailWidth / t.terrainData.detailHeight: " + t.terrainData.detailWidth / t.terrainData.detailHeight);
        //normalizedPos *= (t.terrainData.detailWidth / t.terrainData.detailHeight);
        normalizedPos *= 1000;
        Debug.Log(normalizedPos.x + " " + normalizedPos.y);
        Debug.Log("normalizedPos after multiplication: " + normalizedPos);
        float[] xymaxmin = new float[4];
        xymaxmin[0] = test.z + 3;
        xymaxmin[1] = test.z - 3;
        xymaxmin[2] = test.x + 3;
        xymaxmin[3] = test.x - 3;
        //int[,,] detailMapData = new int[t.terrainData.detailWidth, t.terrainData.detailHeight, numDetails];
        int xRounded = (int)System.Math.Round(test.x);
        int yRounded = (int)System.Math.Round(test.z);
        Debug.Log("rounded X: " + xRounded + " rounded Y: " + yRounded);
        for (int layerNum = 0; layerNum < numDetails; layerNum++)
        {
            int[,] detailLayer = t.terrainData.GetDetailLayer(0, 0, t.terrainData.detailWidth, t.terrainData.detailHeight, layerNum);

            // For each pixel in the detail map...
            for (var y = 0; y < t.terrainData.detailHeight; y++)
            {
                for (var x = 0; x < t.terrainData.detailWidth; x++)
                {
                    //// If the pixel value is below the threshold then
                    //// set it to zero.
                    //if (detailLayer[x, y] > threshold)
                    //{
                    //    detailLayer[x, y] = 0;
                    //}
                    if (xymaxmin[0] > x && xymaxmin[1] < x && xymaxmin[2] > y && xymaxmin[3] < y)
                        detailLayer[x, y] = 0;
                }
            }
            // write all detail data to terrain data:
            t.terrainData.SetDetailLayer(0, 0, layerNum, detailLayer);
        }


    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("PickUp"))
        {          
            other.gameObject.SetActive(false);
            score++;
        }
        else if (other.gameObject.CompareTag("PickUpGolden"))
        {
            other.gameObject.SetActive(false);
            score += 3;
        }
        else if (other.gameObject.CompareTag("PickUpPoisonous"))
        {
            other.gameObject.SetActive(false);
            score -= 2;
        }
    }

    public void Fire()
    {    
        // Create the Bullet from the Bullet Prefab
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody>().velocity = (transform.forward + transform.right) * bulletSpeed;
        bullet.GetComponent<Bullet>().shotCameFromPlayerIP = playerIP;
        // Destroy the bullet after 2 seconds
        Destroy(bullet, 4.0f);
    }

    /*
    * Scannen der Umgebung des Spieler-Objektes,
    * um dem Spieler zu ermöglichen, alle Spieler in einem bestimmten 
    * Radius abzufragens
    */
    public Collider[] GetSurroundingPlayers(float radius)
    {
        // Es wird nur die Player-Layer benötigt
        int layerMask = 1 << 11;
        // Array, in das alle sich in der Umgebung befindlichen Collider gespeichert werden
        Collider[] collidersInSurrounding;

        // Sich im Umgebungsradius befindlichen Collider in Array schreiben
        collidersInSurrounding = Physics.OverlapSphere(transform.position, radius, layerMask);
        return collidersInSurrounding;
    }

    /*
    * Scannen der Umgebung des Spieler-Objektes,
    * um dem Spieler zu ermöglichen, alle Pick-Ups in einem bestimmten 
    * Radius abzufragens
    */
    public Collider[] GetSurroundingPickUps(float radius)
    {
        // Es wird nur die PickUp-Layer benötigt
        int layerMask = 1 << 10;
        // Array, in das alle sich in der Umgebung befindlichen Collider gespeichert werden
        Collider[] collidersInSurrounding;

        // Sich im Umgebungsradius befindlichen Collider in Array schreiben
        collidersInSurrounding = Physics.OverlapSphere(transform.position, radius, layerMask);
        return collidersInSurrounding;
    }

    public void CutGrass(Terrain t, Vector3 position, int radius)
    {
        if (t == null)
            t = Terrain.activeTerrain;

        int TerrainDetailMapSize = t.terrainData.detailResolution;
        if (t.terrainData.size.x != t.terrainData.size.z)
        {
            Debug.Log("X and Y Size of terrain have to be the same");
            return;
        }

        float PrPxSize = TerrainDetailMapSize / t.terrainData.size.x;

        Vector3 TexturePoint3D = position - t.transform.position;
        TexturePoint3D = TexturePoint3D * PrPxSize;

        float[] xymaxmin = new float[4];
        xymaxmin[0] = TexturePoint3D.z + radius;
        xymaxmin[1] = TexturePoint3D.z - radius;
        xymaxmin[2] = TexturePoint3D.x + radius;
        xymaxmin[3] = TexturePoint3D.x - radius;
        Debug.Log("xymaxmin[0]: " + xymaxmin[0]);
        Debug.Log("xymaxmin[1]: " + xymaxmin[1]);
        Debug.Log("xymaxmin[2]: " + xymaxmin[2]);
        Debug.Log("xymaxmin[3]: " + xymaxmin[3]);

        int numDetails = t.terrainData.detailPrototypes.Length;
        int xRounded = (int)System.Math.Round(TexturePoint3D.x);
        int yRounded = (int)System.Math.Round(TexturePoint3D.z);
        Debug.Log("xRounded: " + xRounded + " yRounded: " + yRounded);
        Debug.Log("t.terrainData.detailHeight: " + t.terrainData.detailHeight + "t.terrainData.detailWidth: " + t.terrainData.detailWidth);
        
      
        for (int layerNum = 0; layerNum < numDetails; layerNum++)
        {
            int[,] map = t.terrainData.GetDetailLayer(0, 0, t.terrainData.detailWidth, t.terrainData.detailHeight, layerNum);
            //Debug.Log("layerNum " + layerNum + " map.Length" + map.Length);
            
            for (int y = 0; y < t.terrainData.detailHeight; y++)
            {
                for (int x = 0; x < t.terrainData.detailWidth; x++)
                {
                    //Debug.Log("x: "+ x + "y:" + y);
                    if (xymaxmin[0] >= y && xymaxmin[1] <= y && xymaxmin[2] >= x && xymaxmin[3] <= x)
                    {
                        map[x, y] = 0;
                        Debug.Log("if erfüllt: x: " + x + " y: " + y);
                    }
                }
            }
            t.terrainData.SetDetailLayer(0, 0, layerNum, map);
        }
        mowing = false;
    }
}
