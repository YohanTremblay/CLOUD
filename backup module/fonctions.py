from random import random
from time import time
from constantes import P01, TOPIC_MODULE
from panneau import Panneau
from information import Information
import json
import time
import random


def protocole_01(panneau: Panneau, information: Information):
    jsonPublish("Début P01", "max", information)
    nb = 0
    while nb<3:
        panneau.scan()
        if (panneau.getChangementBouton("btnA") == True) :
            panneau.setBtnLed_On("ledA")
            jsonPublish("Boutton Appuyé","hub", information)
        elif (panneau.getChangementBouton("btnA") == False) :
            panneau.setBtnLed_Off("ledA") 
            jsonPublish("Boutton Relâché", "", information)
            nb+=1    
        print(str(nb))
    jsonPublish("Fin P01","cheer" ,information)

def protocole_02(panneau: Panneau, information: Information):
    jsonPublish("Début P02", "max", information)
    panneau.scan()
    for x in range(20):
        panneau.setRGBLed(x, (0,0,0))
    time.sleep(0.5)
    nbLed = 4
    while nbLed <= 6 :
        panneau.setRGBLed(nbLed, (255,0,0))
        jsonPublish("Lumière " + str(nbLed) + " allumé","test" ,information)
        nbLed+=1
    while (panneau.getChangementSwitch("switchA") != True) :
        panneau.scan()
    panneau.setRGBLed(4, (0,128,0))
    jsonPublish("Switch 4 Activée","test" ,information)
    panneau.setRGBLed(8, (255,0,0))
    time.sleep(10)
    panneau.setRGBLed(8, (0,128,0))
    jsonPublish("Led 8 Allumé","hub" ,information)

    while (panneau.getChangementSwitch("switchB") != True) :
        panneau.scan()
    panneau.setRGBLed(5, (0,128,0))
    jsonPublish("Switch 5 Activée","test" ,information)
    panneau.setRGBLed(9, (255,0,0))
    time.sleep(10)
    panneau.setRGBLed(9, (0,128,0))
    jsonPublish("Led 9 Allumé","hub" ,information)

    while (panneau.getChangementSwitch("switchC") != True) :
        panneau.scan()
    panneau.setRGBLed(6, (0,128,0))
    jsonPublish("Switch 6 Activée","test" ,information)
    panneau.setRGBLed(10, (255,0,0))
    time.sleep(10)
    panneau.setRGBLed(10, (0,128,0))
    jsonPublish("Led 10 Allumé","hub" ,information)

    panneau.setBtnLed_On("ledD")
    jsonPublish("Alert","alarm_beep" ,information)

    while (panneau.getChangementBouton("btnD") != True) :
        panneau.scan()
    panneau.setRGBLed(7, (0,128,0))
    jsonPublish("Désactivation du Danger","buzzer" ,information)

    jsonPublish("Fin P02","cheering" ,information)

def protocole_03(panneau: Panneau, information: Information) :
    jsonPublish("Début P03", "blue", information)
    panneau.scan()
    for x in range(20):
        panneau.setRGBLed(x, (0,0,0))
    time.sleep(0.5)

    nb = 1
    jauge = 0
    panneau.setJauge(jauge)
    color = 0
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


def jsonPublish(affichage, son, information : Information ):
    send_msg = {
        'affichage' : affichage,
        'son'       : son
    }
    information.client.publish(TOPIC_MODULE + "/" + information.code, json.dumps(send_msg))