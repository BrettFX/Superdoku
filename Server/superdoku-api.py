"""
RESTful Superdoku API using Python Flask module for recognizing digits in images of Sudoku puzzles.

Filename: superdoku-api.py
Author: Brett Allen
"""
import flask
from flask import request, jsonify
import datetime
import os
import re
import SudokuExtractor

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
@app.route('{}/recognize'.format(API_BASE_URL), methods=['PUT'])
def recognize():
    """
    Invoke the image recognition task of the SudokuExtractor library.

    Expects an image as the request body in an application/octet-stream format.

    Returns:
        The json response representing the array of integers for the classified digits.
        of the Sudoku puzzle image.
    """
    # Build response object to contain all necessary information (including metadata)
    response = {}

    # Get current date time for file timestamp
    now = datetime.datetime.now()
    file_timestamp = "{}{}{}_{}{}_{}_{}".format(
        now.year, now.month, now.day, now.hour, now.minute, now.second, now.microsecond
    )

    # Get file type from content disposition in request header
    pattern = re.compile("filetype[^;=\n]*=((['\"]).*?\2|[^;\n]*)")
    result = pattern.search(request.headers["Content-Disposition"])
    file_type = result.group(1)

    # Create file path with file timestamp appended and dynamically set file extension based on file type
    file_path = '/tmp/superdoku-snap_{}.{}'.format(file_timestamp, "jpg" if "jpg" in file_type.lower() else "png")

    # Ensure multipart form-data is being used
    if 'application/octet-stream' in request.headers['Content-Type']:
        if len(request.data) > 0:
            with open(file_path, 'wb') as f:
                f.write(request.data)
                f.close()
        
            print("Superdoku snap image saved to {}".format(file_path))
            print("|-- data length: {}".format(len(request.data)))

            # Pass the image to the SudokuExtractor and get the resulting array
            response["puzzle"] = SudokuExtractor.parse_grid(file_path)

            # Cleanup and remove temporary sudoku snaped image from the server
            os.remove(file_path)

        else:
            response["error"] = "Could not get request data to write file with."
    else:
       response["error"] = "Invalid request header: {}. Needs to be application/octet-stream.".format(request.headers['Content-Type']) 
    
    return jsonify(response)

@app.errorhandler(404)
def page_not_found(e):
    """Handle invalid REST endpoint request URLs that cannot be found."""
    return "<h1>404</h1><p>The resource could not be found.</p>", 404

# Run the flask app to begin listening on port 5000 (allow all TCP traffic)
app.run(host='0.0.0.0', port=5000, threaded=False)