from PIL import Image, ImageEnhance

im = Image.open("11-R.jpg")

enhancer = ImageEnhance.Contrast(im)

factor = 0.5 #increase contrast
im_output = enhancer.enhance(factor)
im_output.save('more-contrast-image0.5.jpg')