import random
import cairo
import numpy as np
from PIL import Image, ImageFilter
from scipy import ndimage

def blur(src_file):
    src_img = Image.open(src_file).convert('RGB')
    src_img = src_img.convert('RGB')
    ref_img = src_img.filter(ImageFilter.GaussianBlur(.5 * 2))
    ref_img.save('images/blurredImage3.jpg')

blur('1.jpg')