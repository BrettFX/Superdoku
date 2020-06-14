import flask
from flask import request, jsonify
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
    return "pong"

# Return test sudoku for integration testing
@app.route('{}/test'.format(API_BASE_URL), methods=['GET'])
def test():
    return jsonify(test_sudoku)

# Process an image of a Sudoku puzzle and return the parsed results
@app.route('{}/recognize'.format(API_BASE_URL), methods=['POST'])
def recognize():
    # Ensure octet stream is being used
    if request.headers['Content-Type'] == 'application/octet-stream':
        with open('/tmp/superdoku-snap.png', 'wb') as f:
            f.write(request.data)
            f.close()
        return "Binary message written!"
    
    # Pass the image to the SudokuExtractor 

@app.errorhandler(404)
def page_not_found(e):
    return "<h1>404</h1><p>The resource could not be found.</p>", 404

# Run the flask app to begin listening on port 5000 (allow all TCP traffic)
app.run(host='0.0.0.0')