import flowermeadow.FlowerMeadow;

public class Beispielprogramm_Labyrinth_Java {
	public static void main(String[] args) {
		
		//Instanziieren und Initialisieren des Objekts mit getInstance() 
		//IP und Port muessen eventuell angepasst werden	
		FlowerMeadow client = FlowerMeadow.getInstance("172.22.244.204", 65310);
		
		//Verbindung zum Server aufbauen, Name des Spielers ist Test
		//Diese Methode muss nur einmal aufgerufen werden!
		client.connect("Spieler1");//Einmaliger Aufruf
		
		//Spieler auf dem Spielfeld spawnen. Beim Labyrinth ein fester Spawnpunkt
		client.spawnPlayer(); //Einmaliger Aufruf nur notwendig bis der Spieler zerstört wird.
     
		//client.rotatePlayer(-40);
     	//Spieler bewegen
   		//client.movePlayer(10);
   		
   		//Spieler um 90 Grad nach rechts drehen
   		//client.rotatePlayer(90);
     
     //Gibt die Distanz zur Wand an
    // System.out.println(client.getWallDistance());
     
     //Gibt den Winkel zur Wand an Basierend auf den Object der Wand das am engsten am Spieler dran ist.
     //System.out.println(client.getAngleWall());
     
     //Gibt an bestimmten Stellen einen Tipp aus. Wenn kein Tipp vorhanden gibt es einen leeren String zurück
    // System.out.println(client.getTipp());
    // System.out.println(client.getWallDistance());//Zur Zeigen das ein leeren String zurück gegeben wurde.
		
	//Schießt ein Projektil	. Dieses Ignoriert andere Spieler
   //  client.shoot(); 
     
  //   client.deletePlayer();
  //   client.disconnect();
}
}
