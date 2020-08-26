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
painter = Painter(style=style, output_dir='test-images/')
while True:
	received = socket.recv()
	print(received)
	im = Image.open(BytesIO(base64.b64decode(received)))
	res = painter.paint(im)
	res.write_to_png('test-images/finaloutput3.png')
	with open('test-images/finaloutput2.png', "rb") as img_file:
		message = base64.b64encode(img_file.read())
	print(message)
	socket.send(message)
	time.sleep(1)
