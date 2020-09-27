#! /usr/local/bin/python3
import zmq
import time
import random
import base64
import os
import cairo
import numpy as np
from PIL import Image, ImageFilter
from scipy import ndimage
from painterly import Painter
from style import Style, Impressionist, Expressionist, ColoristWash, Pointillist
from io import BytesIO

context = zmq.Context()
socket = context.socket(zmq.REP)
socket.bind("tcp://*:12345")
style = Impressionist()
painter = Painter(style=style, output_dir='D:/Thesis/Outputs/')
while True:
	received = socket.recv()

	receivedMessage = received.decode('utf-8')

	parsedReceive = receivedMessage.split(",")
	if(len(parsedReceive) == 2):
		picture = bytes(parsedReceive[1], 'utf-8')
		print(picture)
		if(parsedReceive[0] == "GetColorPalette"):
			print("hek")
		elif(parsedReceive[0] == "GetBrushStrokes"):
			print("YUP")
			im = Image.open(BytesIO(base64.b64decode(picture)))
			res = painter.paint(im)
			res.write_to_png('D:\\Thesis\\Outputs\\finaloutput3.png')
			with open('D:\\Thesis\\Outputs\\finaloutput3.png', "rb") as img_file:
				message = base64.b64encode(img_file.read())
			print(message)
			socket.send(message)
	time.sleep(1)
