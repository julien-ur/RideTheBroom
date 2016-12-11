import wiiboard
import time
import sys
import getopt
from PyQt4 import QtGui, QtCore

from bluetooth import *

starttime = time.time()
rate = 0.05

def writeFile(valuesSides):
    #top, bottom, left, right
    with open("balanceBoardData", "w") as btDataFile:
        dataString = ""
        for val in valuesSides:
            dataString += str(int(val)) + " "
        btDataFile.write(dataString)

def main():
    ready = False
    board = None
    board = wiiboard.Wiiboard()
    connected = False
    valuesSides = []

    while(True):
        # Check if connection status changed
        if connected is not board.isConnected():
            connected = board.isConnected()
            if connected:
                print "connected"
            else:                
                print "disconnected"
                #Turn off lights
                time.sleep(0.1) # This is needed for wiiboard.py
                board.setLight(False)

        # Connect to balance board
        if not board.isConnected():
            # Re initialize each run due to bug in wiiboard
            # Note: Seems to be working though :/
            board = wiiboard.Wiiboard()
            print "SYNC"

            address = board.discover()

            if not address:
                sleep = True
                print "NO DEVICE FOUND"
                return []

            print "CONNECTING"
            board.connect(address)

            if board.isConnected():
                connected = True
                print "CONNECTED"

        #Board is connected and ready
        if board.isConnected():

            # Post ready status once
            if not ready:
                ready = True
                time.sleep(0.1) # This is needed for wiiboard.py
                board.setLight(True)
                print "READY"

            valuesSides = board.mass.valuesSides
            print valuesSides
            writeFile(valuesSides)

        time.sleep(rate - ((time.time() - starttime) % rate))

if __name__ == "__main__":
    main()
