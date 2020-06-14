"""
RESTful Superdoku API using Python Flask module for recognizing digits in images of Sudoku puzzles.

Filename: superdoku-api.py
Author: Brett Allen
"""
import flask
from flask import request, jsonify
import SudokuExtractor
import datetime

API_BASE_URL = "/superdoku-api"

app = flask.Flask(__name__)
app.config["DEBUG"] = True

# Create test Sudoku array
test_sudoku = {
    "puzzle" : [
        5, 3, 0,     0, 7, 0,    0, 0, 0,
        6, 0, 0,     1, 9, 5,    0, 0, 0,
        0, 9, 8,     0, 0, 0,    0, 6, 0,

        8, 0, 0,     0, 6, 0,    0, 0, 3,
        4, 0, 0,     8, 0, 3,    0, 0, 1,
        7, 0, 0,     0, 2, 0,    0, 0, 6,

        0, 6, 0,     0, 0, 0,    2, 8, 0,
        0, 0, 0,     4, 1, 9,    0, 0, 5,
        0, 0, 0,     0, 8, 0,    0, 7, 9
    ]
}

# Ping the server to make sure it's alive
@app.route('{}/ping'.format(API_BASE_URL), methods=['GET'])
def ping():
    """Handle ping request and send a pong response back."""
    return "pong"

# Return test sudoku for integration testing
@app.route('{}/test'.format(API_BASE_URL), methods=['GET'])
def test():
    """Test sending back a json representation of a test Sudoku puzzle."""
    return jsonify(test_sudoku)

# Process an image of a Sudoku puzzle and return the parsed results
@app.route('{}/recognize'.format(API_BASE_URL), methods=['POST'])
def recognize():
    """
    Invoke the image recognition task of the SudokuExtractor library.

    Expects an image as the request body in an application/octet-stream format.

    Returns:
        The json response representing the array of integers for the classified digits.
        of the Sudoku puzzle image.
    """
    # Get current date time for file timestamp
    now = datetime.datetime.now()
    file_timestamp = "{}{}{}_{}{}_{}_{}".format(
        now.year, now.month, now.day, now.hour, now.minute, now.second, now.microsecond
    )

    # Create file path with file timestamp appended
    file_path = '/tmp/superdoku-snap_{}.png'.format(file_timestamp)

    # Ensure octet stream is being used
    if request.headers['Content-Type'] == 'application/octet-stream':
        with open(file_path, 'wb') as f:
            f.write(request.data)
            f.close()
        return "Binary message written!"
    
    # Pass the image to the SudokuExtractor 

@app.errorhandler(404)
def page_not_found(e):
    """Handle invalid REST endpoint request URLs that cannot be found."""
    return "<h1>404</h1><p>The resource could not be found.</p>", 404

# Run the flask app to begin listening on port 5000 (allow all TCP traffic)
app.run(host='0.0.0.0')