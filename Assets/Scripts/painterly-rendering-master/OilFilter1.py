import cv2
img = cv2.imread('baboon.jpg')
res = cv2.xphoto.oilPainting(img, 10, 1, cv2.COLOR_BGR2Lab)
filename = 'savedImage.jpg'
cv2.imwrite(filename, res)