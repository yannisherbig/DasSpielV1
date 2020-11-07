//Löschen Sie die Kommentare von den Methoden, welche Sie testen möchten

import flowermeadow.FlowerMeadow;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.math.BigDecimal;

public class Beispielprogramm_Java 
{
	public static void main(String args[]) 
	{
		//Instanziieren und Initialisieren des Objekts mit getInstance() 
		//IP und Port muessen eventuell angepasst werden
		FlowerMeadow f = FlowerMeadow.getInstance("192.168.33.1", 5555);
		
		//Verbindung zum Server aufbauen, Name des Spielers ist Test
		//Diese Methode muss nur einmal aufgerufen werden!
		f.connect("Spieler1");
		
		//Spieler auf dem Spielfeld spawnen
		f.spawnPlayer();
		
		
		
		
		// Pick-Ups automatisch einsammeln:
	        collectPickUps(f);
		
		
		
		// Das Haus vom Nikolaus in den Rasen mähen; Spieler muss vorher passend platziert sein, um genug Platz zu haben:		
		/*
		 * Wenn Rasenmäher im Ursprung spawnt (ist immer der Fall, wenn dort Platz ist),
		 * lässt sich der Rasenmäher mit den folgenden beiden Methodenaufrufen passend für 
		 * den Aufruf "dasHausVomNikolaus()" platzieren: 
		 */
//		f.movePlayer(-20); // Spieler-Objekt rückwärts bewegen
//		f.rotatePlayer(-45); // Spieler-Objekt nach links um 45 Grad rotieren
		
//		dasHausVomNikolaus(f); // Aufruf zum starten der Mäh-Funktion und anschließendes Abfahren des Musters  
		
		
		
		
		//Spieler bewegen
		//f.movePlayer(10);
		
		//Spieler um 90 Grad nach rechts drehen
		//f.rotatePlayer(90);
		
		//Methoden können auch mit Schleifen benutzt werden
		/*for(int i = 0; i < 3; i++)
		{
			//Spieler erst bewegen...
			f.movePlayer(20);
			//...dann um 20 Grad drehen
			f.rotatePlayer(20);
			
			//Vorgang kann beliebig oft ausgeführt werden, hier 3 Mal
		}*/
		
		//Schießt ein Projektil
		//f.shoot();
		
		//Schaltet das Zeichnen der Linie hinter dem Spieler ein, hier in Rot
		//f.startDrawing("Red");
		
		//f.movePlayer(50);
		
		//Linie wieder ausschalten und löschen
		//f.stopAndClearDrawing();
		
		//Gibt die Distanz zur Wand an
		//f.getDistToWall();
		
		//Loescht den Spieler vom Spielfeld
		//f.deletePlayer();
		
		//Trennt die Verbindung zum Server
		//Zusätzliche Konsolenausgaben können manuell eingerichtet werden:
		//f.disconnect();
		//System.out.println("Feierabend!");
		
		//Die restlichen Methoden werden auf dieselbe Weise aufgerufen.
	}

	public static void shootAndApproachMovingOpponent(FlowerMeadow g, String opponentUsername) {
		try {
			boolean opponentStillInRadius = false, opponentDead = false;
			float xDirVectPlayer, zDirVectPlayer, xPosPlayer, zPosPlayer, xPosOpponent, zPosOpponent;
			float[] dirVectPlayer, dirVectToOpponent = { 0, 0 }, playerPos, posOpponent;
			int dimensions = 2; // Wir befinden uns immer auf einer Ebene

			while (!opponentDead) {
				opponentStillInRadius = false; // Annahme
				JSONArray players = new JSONArray(g.getPlayersInRadius(200)); // Radius von 200 ist ausreichend, um
																				// gesamtes Spielfeld abzudecken
				for (int playerCounter = 0; playerCounter < players.length(); playerCounter++) {

					JSONObject player = (JSONObject) players.get(playerCounter);
					String playerName = (String) player.get("name");

					if (playerName.equals(opponentUsername)) {
						opponentStillInRadius = true;

						// Position vom eigenen Spieler-Objekt anfordern:
						JSONObject playerStatusJSONObject = new JSONObject(g.getStatus());
						JSONObject playerPosJSONObject = (JSONObject) playerStatusJSONObject.get("position");
						xPosPlayer = BigDecimal.valueOf(playerPosJSONObject.getDouble("x")).floatValue();
						zPosPlayer = BigDecimal.valueOf(playerPosJSONObject.getDouble("z")).floatValue();
						playerPos = new float[] { xPosPlayer, zPosPlayer };
						System.out.println("playerPos: " + java.util.Arrays.toString(playerPos));

						// Richtungsvektor vom Spieler-Objekt anfordern:
						JSONObject dirVectJSONObject = new JSONObject(g.getDirectionVector());
						xDirVectPlayer = BigDecimal.valueOf(dirVectJSONObject.getDouble("x")).floatValue();
						zDirVectPlayer = BigDecimal.valueOf(dirVectJSONObject.getDouble("z")).floatValue();
						dirVectPlayer = new float[] { xDirVectPlayer, zDirVectPlayer };
						System.out.println("dirVectPlayer: " + java.util.Arrays.toString(dirVectPlayer));

						// Position vom Opponent extrahieren:
						JSONObject posPlayerJSONObject = (JSONObject) player.get("position");
						xPosOpponent = BigDecimal.valueOf(posPlayerJSONObject.getDouble("x")).floatValue();
						zPosOpponent = BigDecimal.valueOf(posPlayerJSONObject.getDouble("z")).floatValue();
						posOpponent = new float[] { xPosOpponent, zPosOpponent };
						System.out.println("posOpponent: " + java.util.Arrays.toString(posOpponent));

						// Richtungsvektor von Spielerposition zu Opponent-Position berechnen:
						for (int coordinateCounter = 0; coordinateCounter < dimensions; coordinateCounter++) {
							dirVectToOpponent[coordinateCounter] = posOpponent[coordinateCounter]
									- playerPos[coordinateCounter];
						}
						System.out.println("dirVectToOpponent: " + java.util.Arrays.toString(dirVectToOpponent));

						// Winkel berechnen:
						double requiredRotation = Math
								.toDegrees(Math.atan2(crossProduct(dirVectToOpponent, dirVectPlayer),
										dotProduct(dirVectToOpponent, dirVectPlayer, 2)));

						// Player beschießen:
						if (requiredRotation != 0)
							g.rotatePlayer(requiredRotation);

						g.shoot(); // Schießen nur einmal, da sich der Spieler inzwischen wieder neu bewegt haben
									// kann
						g.movePlayer(10); // Wir bewegen uns langsam auf unseren Gegner zu
						Thread.sleep(300); // Schießpause wird benötigt

						break; // Uns interessiert nur der spezifizierte Gegner
					}
				}
				if (!opponentStillInRadius)
					opponentDead = true;
			}

		} catch (JSONException e) {
			e.printStackTrace();
		} catch (InterruptedException e) {
			e.printStackTrace();
		}
	}

	public static void dasHausVomNikolaus(FlowerMeadow g) {
		g.startMowing();
		g.movePlayer(18);
		try {
			Thread.sleep(2000);
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		g.rotatePlayer(30);
		g.movePlayer(18);
		try {
			Thread.sleep(1300);
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		g.rotatePlayer(120);
		g.movePlayer(17);
		try {
			Thread.sleep(1300);
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		g.rotatePlayer(30);
		g.movePlayer(18);
		try {
			Thread.sleep(1800);
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		g.rotatePlayer(135);
		g.movePlayer(22);
		try {
			Thread.sleep(2500);
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		g.rotatePlayer(135);
		g.movePlayer(21);
		try {
			Thread.sleep(1000);
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		g.rotatePlayer(135);
		g.movePlayer(24);
		try {
			Thread.sleep(2000);
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		g.rotatePlayer(-135);
		g.movePlayer(18);
		try {
			Thread.sleep(1200);
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	public static void collectPickUps(FlowerMeadow fm) {
		float xDirVectPlayer, zDirVectPlayer, xPosPlayer, 
			zPosPlayer, xPosPickUp, zPosPickUp;
		float[] dirVectPlayer, dirVectToPickUp = { 0, 0 }, 
				playerPos, posPickUp;
		int dimensions = 2; // Wir befinden uns immer auf einer Ebene

		while (true) { // Solange du Lust hast, Programm muss manuell gestoppt werden
			try {
				// Radius von 200 ist ausreichend, um gesamtes Spielfeld abzudecken:
				JSONArray pickUps = new JSONArray(fm.getPickUpsInRadius(200));
				
				for (int pickUpCounter = 0; pickUpCounter < pickUps.length(); pickUpCounter++) {

					JSONObject pickUp = (JSONObject) pickUps.get(pickUpCounter);
					String pickUpName = (String) pickUp.get("name");

					// Mushroom-Pick-Up vermeiden
					if (pickUpName.contains("FlowerPickUp") || pickUpName.contains("FlowerPickUpGolden")) { 
						// Position vom Spieler-Objekt anfordern:
						JSONObject playerStatusJSONObject = new JSONObject(fm.getStatus());
						JSONObject playerPosJSONObject = (JSONObject) playerStatusJSONObject.get("position");
						xPosPlayer = BigDecimal.valueOf(playerPosJSONObject.getDouble("x")).floatValue();
						zPosPlayer = BigDecimal.valueOf(playerPosJSONObject.getDouble("z")).floatValue();
						playerPos = new float[] { xPosPlayer, zPosPlayer };
						System.out.println("playerPos: " + java.util.Arrays.toString(playerPos));

						// Richtungsvektor vom Spieler-Objekt anfordern:
						JSONObject dirVectJSONObject = new JSONObject(fm.getDirectionVector());
						xDirVectPlayer = BigDecimal.valueOf(dirVectJSONObject.getDouble("x")).floatValue();
						zDirVectPlayer = BigDecimal.valueOf(dirVectJSONObject.getDouble("z")).floatValue();
						dirVectPlayer = new float[] { xDirVectPlayer, zDirVectPlayer };
						System.out.println("dirVectPlayer: " + java.util.Arrays.toString(dirVectPlayer));

						// Position vom PickUp extrahieren:
						JSONObject posPickUpJSONObject = (JSONObject) pickUp.get("position");
						xPosPickUp = BigDecimal.valueOf(posPickUpJSONObject.getDouble("x")).floatValue();
						zPosPickUp = BigDecimal.valueOf(posPickUpJSONObject.getDouble("z")).floatValue();
						posPickUp = new float[] { xPosPickUp, zPosPickUp };
						System.out.println("posPickUp: " + java.util.Arrays.toString(posPickUp));

						// Richtungsvektor von Spielerposition zu PickUp-Position berechnen:
						for (int coordinateCounter = 0; coordinateCounter < dimensions; coordinateCounter++) {
							dirVectToPickUp[coordinateCounter] = posPickUp[coordinateCounter]
									- playerPos[coordinateCounter];
						}
						System.out.println("dirVectToPickUp: " + java.util.Arrays.toString(dirVectToPickUp));

						// Winkel berechnen:
						double requiredRotation = Math
								.toDegrees(Math.atan2(crossProduct(dirVectToPickUp, dirVectPlayer),
										dotProduct(dirVectToPickUp, dirVectPlayer, 2)));

						// Pick-Up einsammeln:
						fm.rotatePlayer(requiredRotation);
						Thread.sleep(500); // Warten bis die Rotation vollzogen ist
						fm.movePlayer(100);
						Thread.sleep(1000); // Warten bis Pick-Up eingesammelt ist
						if(fm.getDistToWall() < 15)
							fm.movePlayer(-5);
						Thread.sleep(500);
						fm.movePlayer(0); // Spieler-Objekt stoppen um beim nächstem mal erfolgreich rotieren zu können
						break; // Lieber aus dem Loop ausbrechen, und die neusten Positionsdaten abfragen, als
								// nochmal die alten zu verwenden
					}
				}
			} catch (JSONException e) {
				e.printStackTrace();
			} catch (InterruptedException e) {
				e.printStackTrace();
			}
		}
	}

	// Kreuzprodukt für zwei Vektoren in einer Ebene, ergibt ein Skalar:
	public static double crossProduct(float vect_A[], float vect_B[]) {
		return vect_A[0] * vect_B[1] - vect_A[1] * vect_B[0];
	}

	// Skalarprodukt:
	public static double dotProduct(float[] a, float[] b, int dimensions) {
		int scalar = 0;
		for (int coordinateCounter = 0; coordinateCounter < dimensions; coordinateCounter++) {
			scalar += a[coordinateCounter] * b[coordinateCounter];
		}
		return scalar;
	}
}