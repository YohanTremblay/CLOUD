from random import random
from time import time
from constantes import P01, TOPIC_MODULE, SON_DEMARRAGE, SON_ENCOURS, SON_FERMETURE
from panneau import Panneau
from information import Information
import json
import time
import random

# Protocole 01
def protocole_01(panneau: Panneau, information: Information):
    # 1. Initialisation
    jsonPublish("Début P01", SON_DEMARRAGE, information)
    nb = 1

    # 2. Test button pressed
    while nb<4:
        panneau.scan()
        if (panneau.getChangementBouton("btnA") == True) :
            panneau.setBtnLed_On("ledA")
            jsonPublish("Boutton Appuyé",SON_ENCOURS, information)
        elif (panneau.getChangementBouton("btnA") == False) :
            panneau.setBtnLed_Off("ledA") 
            jsonPublish("Boutton Relâché", "", information)
            panneau.setRGBLed(nb, (0,255,0))
            nb+=1


    # 3. Fin protocole
    jsonPublish("Fin P01", SON_FERMETURE, information)

# Protocole 02
def protocole_02(panneau: Panneau, information: Information):
    # 1. Initialisation
    jsonPublish("Début P02", SON_DEMARRAGE, information)
    
    # Allumage led fusee
    panneau.scan()
    for x in range(20):
        panneau.setRGBLed(x, (0,0,0))
    time.sleep(0.5)
    nbLed = 4
    while nbLed <= 6 :
        panneau.setRGBLed(nbLed, (255,0,0))
        jsonPublish("Lumière " + str(nbLed) + " allumé", SON_ENCOURS, information)
        nbLed+=1

    # Switch 1
    while (panneau.getChangementSwitch("switchA") != True) :
        panneau.scan()
    panneau.setRGBLed(4, (0,128,0))
    jsonPublish("Switch 4 Activée",SON_ENCOURS ,information)
    panneau.setRGBLed(8, (255,0,0))
    time.sleep(5)
    panneau.setRGBLed(8, (0,128,0))
    jsonPublish("Led 8 Allumé",SON_ENCOURS ,information)

    # Switch 2
    while (panneau.getChangementSwitch("switchB") != True) :
        panneau.scan()
    panneau.setRGBLed(5, (0,128,0))
    jsonPublish("Switch 5 Activée",SON_ENCOURS ,information)
    panneau.setRGBLed(9, (255,0,0))
    time.sleep(5)
    panneau.setRGBLed(9, (0,128,0))
    jsonPublish("Led 9 Allumé",SON_ENCOURS ,information)

    # Switch 3
    while (panneau.getChangementSwitch("switchC") != True) :
        panneau.scan()
    panneau.setRGBLed(6, (0,128,0))
    jsonPublish("Switch 6 Activée",SON_ENCOURS ,information)
    panneau.setRGBLed(10, (255,0,0))
    time.sleep(5)
    panneau.setRGBLed(10, (0,128,0))
    jsonPublish("Led 10 Allumé",SON_ENCOURS ,information)

    # Publication etat
    panneau.setBtnLed_On("ledD")
    jsonPublish("Alert",SON_ENCOURS ,information)

    # Danger button
    while (panneau.getChangementBouton("btnD") != True) :
        panneau.scan()
    panneau.setRGBLed(7, (0,128,0))
    jsonPublish("Désactivation du Danger",SON_ENCOURS ,information)

    panneau.setRGBLed(11, (255,0,0))
    time.sleep(5)
    panneau.setRGBLed(11, (0,128,0))

    # 3. Fin protocole
    jsonPublish("Fin P02", SON_FERMETURE, information)

# Protocole 03
def protocole_03(panneau: Panneau, information: Information) :
    # Initialisation
    jsonPublish("Début P03", SON_DEMARRAGE, information)
    panneau.scan()
    for x in range(20):
        panneau.setRGBLed(x, (0,0,0))
    time.sleep(0.5)

    nb = 1
    jauge = 0
    panneau.setJauge(jauge)
    color = 0

    # Roulette
    while nb<=3:
        stop = False
        panneau.scan()
        if (panneau.getChangementRotatif() == 5) :
                panneau.scan()
                jauge = jauge+10
                panneau.setJauge(jauge)
                if(jauge <= 20) :
                    panneau.setRGBLed(0, (jauge+50,0,jauge+50))
                if(jauge <= 40 and jauge > 20) :
                    panneau.setRGBLed(0, (0,jauge+50,0))
                if(jauge <= 60 and jauge > 40) :
                    panneau.setRGBLed(0, (0,jauge+50,jauge+50))
                if(jauge <= 80 and jauge > 60) :
                    panneau.setRGBLed(0, (0,0,jauge+50))
                if(jauge <= 100 and jauge > 80) :
                    panneau.setRGBLed(0, (jauge+50,jauge+50,0))
                if(jauge > 100) :
                    panneau.setRGBLed(0, (jauge+50,0,0))
        if (panneau.getChangementRotatif() == 6) :
                panneau.scan()
                jauge = jauge-10
                panneau.setJauge(jauge)
                if(jauge <= 20) :
                    panneau.setRGBLed(0, (jauge+50,0,jauge+50))
                if(jauge <= 40 and jauge > 20) :
                    panneau.setRGBLed(0, (0,jauge+50,0))
                if(jauge <= 60 and jauge > 40) :
                    panneau.setRGBLed(0, (0,jauge+50,jauge+50))
                if(jauge <= 80 and jauge > 60) :
                    panneau.setRGBLed(0, (0,0,jauge+50))
                if(jauge <= 100 and jauge > 80) :
                    panneau.setRGBLed(0, (jauge+50,jauge+50,0))
                if(jauge > 100) :
                    panneau.setRGBLed(0, (jauge+50,0,0))
        if(panneau.getChangementBouton("Rotatif_btn") == True) :
            time.sleep(3)
            if(jauge <= 20) :
                panneau.setRGBLed(nb, (jauge+50,0,jauge+50))
            if(jauge <= 40 and jauge > 20) :
                panneau.setRGBLed(nb, (0,jauge+50,0))
            if(jauge <= 60 and jauge > 40) :
                panneau.setRGBLed(nb, (0,jauge+50,jauge+50))
            if(jauge <= 80 and jauge > 60) :
                panneau.setRGBLed(nb, (0,0,jauge+50))
            if(jauge <= 100 and jauge > 80) :
                panneau.setRGBLed(nb, (jauge+50,jauge+50,0))
            if(jauge > 100) :
                panneau.setRGBLed(nb, (jauge+50,0,0))
            nb=nb+1

    # 3. Fin protocole
    jsonPublish("Fin P03", SON_FERMETURE, information)




def jsonPublish(affichage, son, information : Information ):
    send_msg = {
        'affichage' : affichage,
        'son'       : son
    }
    information.client.publish(TOPIC_MODULE + information.code, json.dumps(send_msg))