"""
A Sudoko image extraction script using Keras and OpenCV modules.

Filename: SudokuExtractor.py
Author: Brett Allen
Description: 
    Take an image of a Sudoku puzzle, use the OpenCV module to normalize and pre-process
    the image. Once pre-processing is complete, the script uses a pre-training digit
    classification cnn to iteratively classify the set of image regions representing the
    individual sudoku puzzle digits. Lastly, an integer array of the classified digits
    is created and returned as an interfacing component for external programs such as a 
    REST API.
"""
import cv2
import operator
import numpy as np
from matplotlib import pyplot as plt
from keras.preprocessing.image import load_img
from keras.preprocessing.image import img_to_array
from keras.models import load_model
from PIL import Image
import os
from sys import argv as args

# Global constants
# keras_digit_classifier_model.h5
# digit_classifier_cnn.h5
# digit_classifier_cnn_model_with_droupout.h5
DIGIT_CLASSIFIER_MODEL_PATH = 'digit_classifier_cnn.h5'

def plot_many_images(images, titles, rows=1, columns=2):
    """Plot each image in a given list as a grid structure. using Matplotlib."""
    for i, image in enumerate(images):
        plt.subplot(rows, columns, i+1)
        plt.imshow(image, 'gray')
        plt.title(titles[i])
        plt.xticks([]), plt.yticks([])  # Hide tick marks
    plt.show()


def show_image(img, title='image'):
    """Show an image until any key is pressed."""
    # Don't show digits if entry point wasn't from the command line
    if __name__ != '__main__':
        return
    
    cv2.imshow(title, img)  # Display the image
    cv2.waitKey(0)  # Wait for any key to be pressed (with the image window active)
    cv2.destroyAllWindows()  # Close all windows


def show_digits(digits, colour=255):
    """Show list of 81 extracted digits in a grid format."""
    # Don't show digits if entry point wasn't from the command line
    if __name__ != '__main__':
        return
    
    rows = []
    with_border = [cv2.copyMakeBorder(img.copy(), 1, 1, 1, 1, cv2.BORDER_CONSTANT, None, colour) for img in digits]
    for i in range(9):
        row = np.concatenate(with_border[i * 9:((i + 1) * 9)], axis=1)
        rows.append(row)
    show_image(np.concatenate(rows))


def convert_when_colour(colour, img):
    """Dynamically converts an image to colour if the input colour is a tuple and the image is grayscale."""
    if len(colour) == 3:
        if len(img.shape) == 2:
            img = cv2.cvtColor(img, cv2.COLOR_GRAY2BGR)
        elif img.shape[2] == 1:
            img = cv2.cvtColor(img, cv2.COLOR_GRAY2BGR)
    return img


def display_points(in_img, points, radius=5, colour=(0, 0, 255)):
    """Draws circular points on an image."""
    img = in_img.copy()

    # Dynamically change to a colour image if necessary
    if len(colour) == 3:
        if len(img.shape) == 2:
            img = cv2.cvtColor(img, cv2.COLOR_GRAY2BGR)
        elif img.shape[2] == 1:
            img = cv2.cvtColor(img, cv2.COLOR_GRAY2BGR)

    for point in points:
        img = cv2.circle(img, tuple(int(x) for x in point), radius, colour, -1)
    show_image(img)
    return img


def display_rects(in_img, rects, colour=(0, 0, 255)):
    """Display rectangles on the image."""
    img = convert_when_colour(colour, in_img.copy())
    for rect in rects:
        img = cv2.rectangle(img, tuple(int(x) for x in rect[0]), tuple(int(x) for x in rect[1]), colour)
    show_image(img)
    return img


def display_contours(in_img, contours, colour=(0, 0, 255), thickness=2):
    """Display contours on the image."""
    img = convert_when_colour(colour, in_img.copy())
    img = cv2.drawContours(img, contours, -1, colour, thickness)
    show_image(img)


def pre_process_image(img, skip_dilate=False):
    """Use a blurring function, adaptive thresholding and dilation to expose the main features of an image."""
    # Gaussian blur with a kernal size (height, width) of 9.
    # Note that kernal sizes must be positive and odd and the kernel must be square.
    proc = cv2.GaussianBlur(img.copy(), (9, 9), 0)

    # Adaptive threshold using 11 nearest neighbour pixels
    proc = cv2.adaptiveThreshold(proc, 255, cv2.ADAPTIVE_THRESH_GAUSSIAN_C, cv2.THRESH_BINARY, 11, 2)

    # Invert colours, so gridlines have non-zero pixel values.
    # Necessary to dilate the image, otherwise will look like erosion instead.
    proc = cv2.bitwise_not(proc, proc)

    if not skip_dilate:
        # Dilate the image to increase the size of the grid lines.
        kernel = np.array([[0., 1., 0.], [1., 1., 1.], [0., 1., 0.]], np.uint8)
        proc = cv2.dilate(proc, kernel)

    return proc


def find_corners_of_largest_polygon(img):
    """Find the 4 extreme corners of the largest contour in the image."""
    contours, h = cv2.findContours(img.copy(), cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)  # Find contours
    contours = sorted(contours, key=cv2.contourArea, reverse=True)  # Sort by area, descending
    polygon = contours[0]  # Largest image

    # Use of `operator.itemgetter` with `max` and `min` allows us to get the index of the point
    # Each point is an array of 1 coordinate, hence the [0] getter, then [0] or [1] used to get x and y respectively.

    # Bottom-right point has the largest (x + y) value
    # Top-left has point smallest (x + y) value
    # Bottom-left point has smallest (x - y) value
    # Top-right point has largest (x - y) value
    bottom_right, _ = max(enumerate([pt[0][0] + pt[0][1] for pt in polygon]), key=operator.itemgetter(1))
    top_left, _ = min(enumerate([pt[0][0] + pt[0][1] for pt in polygon]), key=operator.itemgetter(1))
    bottom_left, _ = min(enumerate([pt[0][0] - pt[0][1] for pt in polygon]), key=operator.itemgetter(1))
    top_right, _ = max(enumerate([pt[0][0] - pt[0][1] for pt in polygon]), key=operator.itemgetter(1))

    # Return an array of all 4 points using the indices
    # Each point is in its own array of one coordinate
    return [polygon[top_left][0], polygon[top_right][0], polygon[bottom_right][0], polygon[bottom_left][0]]


def distance_between(p1, p2):
    """Return the scalar distance between two points."""
    a = p2[0] - p1[0]
    b = p2[1] - p1[1]
    return np.sqrt((a ** 2) + (b ** 2))


def crop_and_warp(img, crop_rect):
    """Crop and warps a rectangular section from an image into a square of similar size."""
    # Rectangle described by top left, top right, bottom right and bottom left points
    top_left, top_right, bottom_right, bottom_left = crop_rect[0], crop_rect[1], crop_rect[2], crop_rect[3]

    # Explicitly set the data type to float32 or `getPerspectiveTransform` will throw an error
    src = np.array([top_left, top_right, bottom_right, bottom_left], dtype='float32')

    # Get the longest side in the rectangle
    side = max([
        distance_between(bottom_right, top_right),
        distance_between(top_left, bottom_left),
        distance_between(bottom_right, bottom_left),
        distance_between(top_left, top_right)
    ])

    # Describe a square with side of the calculated length, this is the new perspective we want to warp to
    dst = np.array([[0, 0], [side - 1, 0], [side - 1, side - 1], [0, side - 1]], dtype='float32')

    # Gets the transformation matrix for skewing the image to fit a square by comparing the 4 before and after points
    m = cv2.getPerspectiveTransform(src, dst)

    # Performs the transformation on the original image
    return cv2.warpPerspective(img, m, (int(side), int(side)))


def infer_grid(img):
    """Infer 81 cell grid from a square image."""
    squares = []
    side = img.shape[:1]
    side = side[0] / 9

    # Note that we swap j and i here so the rectangles are stored in the list reading left-right instead of top-down.
    for j in range(9):
        for i in range(9):
            p1 = (i * side, j * side)  # Top left corner of a bounding box
            p2 = ((i + 1) * side, (j + 1) * side)  # Bottom right corner of bounding box
            squares.append((p1, p2))
    return squares


def cut_from_rect(img, rect):
    """Cut a rectangle from an image using the top left and bottom right points."""
    return img[int(rect[0][1]):int(rect[1][1]), int(rect[0][0]):int(rect[1][0])]


def center_pad(size, length):
    """Handle centering for a given length that may be odd or even."""
    if length % 2 == 0:
        side1 = int((size - length) / 2)
        side2 = side1
    else:
        side1 = int((size - length) / 2)
        side2 = side1 + 1
    return side1, side2


def scale(r, x):
    """Scale by truncating the product of r and x."""
    return int(r * x)


def scale_and_centre(img, size, margin=0, background=0):
    """Scale and centres an image onto a new background square."""
    h, w = img.shape[:2]
    if h > w:
        t_pad = int(margin / 2)
        b_pad = t_pad
        ratio = (size - margin) / h
        w, h = scale(ratio, w), scale(ratio, h)
        l_pad, r_pad = center_pad(size, w)
    else:
        l_pad = int(margin / 2)
        r_pad = l_pad
        ratio = (size - margin) / w
        w, h = scale(ratio, w), scale(ratio, h)
        t_pad, b_pad = center_pad(size, h)

    img = cv2.resize(img, (w, h))
    img = cv2.copyMakeBorder(img, t_pad, b_pad, l_pad, r_pad, cv2.BORDER_CONSTANT, None, background)
    return cv2.resize(img, (size, size))


def find_largest_feature(inp_img, scan_tl=None, scan_br=None):
    """
    Fill this structure in white, reducing the rest to black.

    Use the fact the `floodFill` function returns a bounding box of the area it filled to find the biggest
    connected pixel structure in the image.

    Parameters:
        inp_img (cv2.image): the region of image (roi) representing a digit
        scan_tl ([nparray]): margin top, margin left
        scan_br ([nparray]): margin bottom, margin right

    Returns:
        img, np.array(bbox, dtype='float32'), seed_point
    """
    img = inp_img.copy()  # Copy the image, leaving the original untouched
    height, width = img.shape[:2]

    max_area = 0
    seed_point = (None, None)

    if scan_tl is None:
        scan_tl = [0, 0]

    if scan_br is None:
        scan_br = [width, height]

    # Loop through the image
    for x in range(scan_tl[0], scan_br[0]):
        for y in range(scan_tl[1], scan_br[1]):
            # Only operate on light or white squares
            if img.item(y, x) == 255 and x < width and y < height:  # Note that .item() appears to take input as y, x
                area = cv2.floodFill(img, None, (x, y), 64)
                if area[0] > max_area:  # Gets the maximum bound area which should be the grid
                    max_area = area[0]
                    seed_point = (x, y)

    # Colour everything grey (compensates for features outside of our middle scanning range
    for x in range(width):
        for y in range(height):
            if img.item(y, x) == 255 and x < width and y < height:
                cv2.floodFill(img, None, (x, y), 64)

    mask = np.zeros((height + 2, width + 2), np.uint8)  # Mask that is 2 pixels bigger than the image

    # Highlight the main feature
    if all([p is not None for p in seed_point]):
        cv2.floodFill(img, mask, seed_point, 255)

    top, bottom, left, right = height, 0, width, 0

    for x in range(width):
        for y in range(height):
            if img.item(y, x) == 64:  # Hide anything that isn't the main feature
                cv2.floodFill(img, mask, (x, y), 0)

            # Find the bounding parameters
            if img.item(y, x) == 255:
                top = y if y < top else top
                bottom = y if y > bottom else bottom
                left = x if x < left else left
                right = x if x > right else right

    bbox = [[left, top], [right, bottom]]
    return img, np.array(bbox, dtype='float32'), seed_point


def extract_digit(img, rect, size):
    """Extract a digit (if one exists) from a Sudoku square."""
    digit = cut_from_rect(img, rect)  # Get the digit box from the whole square

    # Use fill feature finding to get the largest feature in middle of the box
    # Margin used to define an area in the middle we would expect to find a pixel belonging to the digit
    h, w = digit.shape[:2]
    margin = int(np.mean([h, w]) / 2.5)
    _, bbox, seed = find_largest_feature(digit, [margin, margin], [w - margin, h - margin])
    digit = cut_from_rect(digit, bbox)

    # Scale and pad the digit so that it fits a square of the digit size we're using for machine learning
    w = bbox[1][0] - bbox[0][0]
    h = bbox[1][1] - bbox[0][1]

    # Ignore any small bounding boxes
    if w > 0 and h > 0 and (w * h) > 100 and len(digit) > 0:
        return scale_and_centre(digit, size, 4)
    else:
        return np.zeros((size, size), np.uint8)


def get_digits(img, squares, size):
    """Extract digits from their cells and builds an array."""
    digits = []
    img = pre_process_image(img.copy(), skip_dilate=True)
    for square in squares:
        digits.append(extract_digit(img, square, size))
    return digits


def classify_digit(digit_img, model):
    """
    Use ML model to classify an image of a digit to the respective integer representation in memory.

    Parameters:
        digit_img (PIL.Image): the digit image in question, assumed to be in an inverted black and white format.
        model (keras.models.load_model) the machine learning model to use for prediction.
    
    Returns:
        the integer representation of the digit image in question.
    """
    img_arr = img_to_array(digit_img)

    # reshape into a single sample with 1 channel
    reshaped = img_arr.reshape(1, 28, 28, 1)

    # prepare pixel data
    float_data = reshaped.astype('float32')
    final_img = float_data / 255.0

    # predict the class
    digit = model.predict_classes(final_img)
    return digit[0]


def get_classified_digits(digits, stage_output):
    """
    Get region of images (roi) using training CNN for recognizing digits.

    Parameters:
        digits (nparray): the array representation of the extracted digits.
        stage_output (bool): whether to create staging output images for testing.

    Returns:
        the integer array list of classified digits.
    """
    model = load_model(DIGIT_CLASSIFIER_MODEL_PATH)
    classified_digits = []
    i = 0
    for digit in digits:
        
        img = Image.fromarray(digit) 
        if stage_output:
            # Create staging dir if it does not exist

            if not os.path.exists('staging'):
                os.makedirs('staging')

            img.save('staging/cell_{}.png'.format(i))
        
        # Set classified digit to 0 if the cell is blank
        if not np.any(digit):
            classified_digits.append(0)
        else:
            classified_digit = classify_digit(img, model)

            # Handle corner cases
            if classified_digit == 0:
                classified_digit = 4    # Common for 4 to be misclassified as 0

            # Append the classified digit (convert from numpy.int64 to int)
            classified_digits.append(int(classified_digit))
        
        i += 1
        
    return classified_digits


def get_grid_array(classified_digits):
    """
    Get 2-Dimensional representation of the 1-Dimensional integer array of classified digits.

    Parameters:
        classified_digits ([int]): the array of integers representing the classified digits.

    Returns:
        the 2-Dimensional grid.
    """
    row = []
    grid = []
    for i in range(len(classified_digits)):
        if i != 0 and (i % (len(classified_digits) / 9)) == 0:
            grid.append(row)
            row = []

        row.append(classified_digits[i])
    grid.append(row)
    return grid


def print_sudoku_puzzle(classified_digits):
    """
    Display Sudoku puzzle to the console.

    Parameters:
        classified_digits ([int]): the array of integers representing the classified digits.
    """
    # Don't show digits if entry point wasn't from the command line
    if __name__ != '__main__':
        return
     
    # Get the grid array based on the extracted digits
    grid_array = get_grid_array(classified_digits)

    # The character to determine formatting
    display = ""

    # Use the values from the sudokuPuzzle and write them to the respective cell using the txtFieldMap
    for row in range(9):
        display += "\n"
        for col in range(9):
            # Print out a divider every third line
            display += ("\t" if (col % 3 == 0) else " ") + str(grid_array[row][col])
        display += "\n" if ((row + 1) % 3 == 0) else ""
            
    print("\nSudoku Puzzle:")
    print(display)


def image_resize(image, width = None, height = None, inter = cv2.INTER_AREA):
    """
    Resize an image to be within the specified threshold width and height.
    
    This function is used primarily to ensure the size of an image is large enough to 
    be processed.

    Parameters:
        image (cv2.imread): The cv2 image to resize
        width (int): The desired width of the resized image
        height (int): The desired height of the resized image
        inter (int): Global constant integer representing the interpolation method for cv2. 
                     Defaults to cv2.INTER_AREA

    Returns:
        the resized cv2 image
    """
    # initialize the dimensions of the image to be resized and
    # grab the image size
    dim = None
    (h, w) = image.shape[:2]

    # if both the width and height are None, then return the
    # original image
    if width is None and height is None:
        return image

    # check to see if the width is None
    if width is None:
        # calculate the ratio of the height and construct the
        # dimensions
        r = height / float(h)
        dim = (int(w * r), height)

    # otherwise, the height is None
    else:
        # calculate the ratio of the width and construct the
        # dimensions
        r = width / float(w)
        dim = (width, int(h * r))

    # resize the image
    resized = cv2.resize(image, dim, interpolation = inter)

    # return the resized image
    return resized


def autocrop(image, threshold=0):
    """
    Crop any edges below or equal to threshold.

    Crops blank image to 1x1.

    Returns cropped image.

    """
    if len(image.shape) == 3:
        flatImage = np.max(image, 2)
    else:
        flatImage = image
    assert len(flatImage.shape) == 2

    rows = np.where(np.max(flatImage, 0) > threshold)[0]
    if rows.size:
        cols = np.where(np.max(flatImage, 1) > threshold)[0]
        image = image[cols[0]: cols[-1] + 1, rows[0]: rows[-1] + 1]
    else:
        image = image[:1, :1]

    return image

def write_digits_to_file(classified_digits, outfile_name):
    """
    Write an array of classified digits to a text file for the purpose of post-processing as needed.

    Parameters:
        classified_digits (array): the array of classified digits to iterate and write to a file
    """
    with open(outfile_name, "w") as f:
        csv_string = ",".join(str(digit) for digit in classified_digits)
        f.write(csv_string)
    

def parse_grid(path):
    """
    Parse an image of a Sudoku puzzle given a path to the respective image.

    Parameters:
        path (string): the local path to the source image in question.

    Returns:
        the integer array of classified digits in result of parsing the Sudoku image.
    """
    print("Parsing grid from {}".format(path))
    original = cv2.imread(path, cv2.IMREAD_COLOR)              # Read in the input file with color
    show_image(original, title='Original')
    resized = image_resize(original, height=600)               # Resize the image as needed (e.g., 600x600)
    show_image(resized, title='Resized')
    normalized = autocrop(resized, threshold=50)               # Normalize the image by croping black edges from top and bottom
    show_image(normalized, title='Normalized')
    grayscale = cv2.cvtColor(normalized, cv2.COLOR_BGR2GRAY)   # Convert the image to grayscale for preprocessing
    show_image(grayscale, title='Grayscale')
    processed = pre_process_image(grayscale)                   # Preprocess the image to prepare for identifying corners
    corners = find_corners_of_largest_polygon(processed)       # Find the corners of the Sudoku puzzle
    cropped = crop_and_warp(grayscale, corners)                # Crop the image to only encompass the Sudoku portion
    show_image(cropped, title='Cropped')
    squares = infer_grid(cropped)                              # Draw squares for each Suduko grid cell
    digits = get_digits(cropped, squares, 28)                  # Grab all the digits from the cells (e.g., roi)
    show_digits(digits)
    
    # Get the classified digits (array of digits to be returned as a response)
    classified_digits = get_classified_digits(digits, stage_output=False)
    
    print_sudoku_puzzle(classified_digits)                     # Print the parsed Sudoku puzzle (unsolved)

    # Write to outfile for post-processing as needed
    outfile = os.path.splitext(os.path.basename(path))[0] + ".out"
    write_digits_to_file(classified_digits, outfile)

    # Return parsed classified digits
    return classified_digits


def main():
    """Entry point of this script if invoked at the command line."""
    if len(args) == 1 or len(args) > 2:
        print("Invalid CLI args. Pass in an image path for a Sudoku puzzle")
    else:
        source_path = args[1]
        parse_grid(source_path)

if __name__ == '__main__':
    main()
