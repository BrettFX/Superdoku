# make a prediction for a new image.

from keras.preprocessing.image import load_img
from keras.preprocessing.image import img_to_array
from keras.models import load_model
from sys import argv as args

# load and prepare the image
def load_image(filename):
    # load the image
    img = load_img(filename, color_mode="grayscale", target_size=(28, 28))
    # convert to numpy array
    img = img_to_array(img)
    # reshape into a single sample with 1 channel
    img = img.reshape(1, 28, 28, 1)
    # prepare pixel data
    img = img.astype('float32')
    img = img / 255.0
    return img

# load an image and predict the class
def run_example(source_path):
    # load the image
    img = load_image(source_path)
    # load model
    model = load_model('final_model.h5')
    # predict the class
    digit = model.predict_classes(img)
    print(digit[0])

def main():
    if len(args) == 1 or len(args) > 2:
        print("Invalid CLI args. Pass in an image path of a digit")
    else:
        source_path = args[1]
        run_example(source_path)

if __name__ == '__main__':
    main()
