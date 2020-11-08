#! /usr/local/bin/python3
import zmq
import time
import os
import base64
from PIL import Image
from painterly import Painter
from style import Impressionist
from io import BytesIO
from rungisteuclideandist import RunGistEuclideanDist

print("Server Started")
context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:12345")
style = Impressionist()
painter = Painter(style=style, output_dir='D:/College Files/THESIS/Outputs')
while True:
    received = socket.recv()
    receivedMessage = received.decode('utf-8')

    parsedReceive = receivedMessage.split(",")
    if len(parsedReceive) == 9:
        picture = bytes(parsedReceive[1], 'utf-8')
        stringPicture = str(parsedReceive[1])
        brushStrokeIndex = str(parsedReceive[2])
        brush128Opacity = str(parsedReceive[3])
        brush64Opacity = str(parsedReceive[4])
        brush32Opacity = str(parsedReceive[5])
        brush8Opacity = str(parsedReceive[6])
        brush4Opacity = str(parsedReceive[7])
        brush2Opacity = str(parsedReceive[8])
        if parsedReceive[0] == "GetColorPalette":
            # with open("D:\\Thesis\\CapturedUnalteredScene\\UnalteredScene\\UnalteredScene.jpg", "wb") as jpg_file:
            #	jpg_file.write(base64.b64decode(stringPicture))
            if os.path.isfile("D:\\Thesis\\CapturedUnalteredScene\\UnalteredScene\\UnalteredScene.jpg"):
                gist = RunGistEuclideanDist()
                gist.runGist()
                while not os.path.isfile("D:\\Thesis\\CapturedUnalteredScene\\GistOutput\\gist.txt"):
                    pass
                if os.path.isfile("D:\\Thesis\\CapturedUnalteredScene\\GistOutput\\gist.txt"):
                    index = gist.runEuclideanDistance()
                    socket.send_string(str(index))
        elif parsedReceive[0] == "GetBrushStrokes":
            with open("D:\\thePainter\\ImpressMe\\UnalteredScene.jpg", "wb") as jpg_file:
                jpg_file.write(base64.b64decode(stringPicture))

            painterInputs = []
            with open("D:\\thePainter\\ImpressMe\\thePainter_OriginalInput.txt", "r") as thePainterInput:
                painterInputs = thePainterInput.readlines()

            painterInputs[0] = 'UnalteredScene.jpg\n'
            painterInputs[4] = '128 ' + brush128Opacity + ' 1.0 50.0 0.5 0\n'
            painterInputs[5] = '64 ' + brush64Opacity + ' 1.0 50.0 0.5 0\n'
            painterInputs[6] = '32 ' + brush32Opacity + ' 1.0 50.0 0.5 0\n'
            painterInputs[7] = '8 ' + brush8Opacity + ' 1.0 50.0 0.5 0\n'
            painterInputs[8] = '4 ' + brush4Opacity + ' 1.0 50.0 0.5 0\n'
            painterInputs[9] = '2 ' + brush2Opacity + ' 1.0 50.0 0.5 0\n'
            painterInputs[11] = '../texture/brush01/' + brushStrokeIndex + '.png\n'
            painterInputs[12] = '../texture/brush01/' + brushStrokeIndex + '.png\n'
            painterInputs[13] = '../texture/brush01_halfwidth/' + brushStrokeIndex + '.png\n'
            painterInputs[14] = '../texture/brush01_halfwidth/' + brushStrokeIndex + '.png\n'
            painterInputs[15] = 'RenderedImage_' + brushStrokeIndex + '.jpg\n'

            with open("D:\\thePainter\\ImpressMe\\thepainter_input.txt", "w") as txtFile:
                for line in  painterInputs:
                    txtFile.write(line)

            finalImagePath = "D:\\thePainter\\ImpressMe\\RenderedImage_" + brushStrokeIndex + ".jpg"
            if os.path.isfile(finalImagePath):
                os.remove(finalImagePath)

            os.chdir('D:\\thePainter\\ImpressMe')
            os.system('D:\\thePainter\\thepainter.exe')

            if os.path.isfile(finalImagePath):
                with open(finalImagePath, "rb") as img_file:
                    message = base64.b64encode(img_file.read())
            #im = Image.open(BytesIO(base64.b64decode(picture)))
            #painter = Painter(style=style, output_dir='D:/College Files/THESIS/Outputs')
            #res = painter.paint(im)
            #res.write_to_png('D:\\Thesis\\Outputs\\finaloutput3.png')
            #with open('D:\\Thesis\\Outputs\\finaloutput3.png', "rb") as img_file:
            #    message = base64.b64encode(img_file.read())
            socket.send(message)
    time.sleep(1)
