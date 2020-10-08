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
	if len(parsedReceive) == 2:
		picture = bytes(parsedReceive[1], 'utf-8')
		stringPicture = str(parsedReceive[1])
		if parsedReceive[0] == "GetColorPalette":
			#with open("D:\\Thesis\\CapturedUnalteredScene\\UnalteredScene\\UnalteredScene.jpg", "wb") as jpg_file:
			#	jpg_file.write(base64.b64decode(stringPicture))
			if(os.path.isfile("D:\\Thesis\\CapturedUnalteredScene\\UnalteredScene\\UnalteredScene.jpg")):
				gist = RunGistEuclideanDist()
				gist.runGist()
				while os.path.isfile("D:\\Thesis\\CapturedUnalteredScene\\GistOutput\\gist.txt") == False:
					pass
				if(os.path.isfile("D:\\Thesis\\CapturedUnalteredScene\\GistOutput\\gist.txt")):
					index = gist.runEuclideanDistance()
					socket.send_string(str(index))
		elif parsedReceive[0] == "GetBrushStrokes":
			print("YUP")
			im = Image.open(BytesIO(base64.b64decode(picture)))
			res = painter.paint(im)
			res.write_to_png('D:\\Thesis\\Outputs\\finaloutput3.png')
			with open('D:\\Thesis\\Outputs\\finaloutput3.png', "rb") as img_file:
				message = base64.b64encode(img_file.read())
			socket.send(message)
	time.sleep(1)
