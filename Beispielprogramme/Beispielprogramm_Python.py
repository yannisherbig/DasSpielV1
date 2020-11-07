# Löschen Sie die Kommentare von den Methoden, welche Sie testen möchten

# Importieren der Klasse FlowerMeadow aus dem Modul flowermeadow
from flowerMeadow import FlowerMeadow

if __name__ == '__main__':
    myGame = FlowerMeadow.getInstance("192.168.33.1", 5555)
    myGame.connect("Yannis")
    myGame.spawnPlayer()
    myGame.startMowing()
    # myGame.startDrawing(Blue) # Anstelle von Rasenmähen eine Linie malen
    myGame.rotatePlayer(100)
    myGame.movePlayer(20)
    myGame.shoot()
    # myGame.movePlayerLabyrinth(20) # Im Labyrinth, diesen Aufruf zum Bewegen benutzen 
    print(myGame.getStatus())
    print(myGame.getDistToWall())
    print(myGame.getPickUpsInRadius(220))
    print(myGame.getPlayersInRadius(220))
    print(myGame.getDirectionVector())
    # print(myGame.getTipp()) # Im Labyrinth hat diese Funktion eine Wirkung
    # print(myGame.getAngleWall()) # Im Labyrinth hat diese Funktion eine Wirkung
    # print(myGame.WallDistance()) # Im Labyrinth hat diese Funktion eine Wirkung
    myGame.stopMowing()
    # myGame.stopAndClearDrawing()
    myGame.deletePlayer()
    myGame.disconnect()
   