image_path=$1

if [ -z $image_path ]; then
    echo "ERROR: Please specify a path to an image file."
    exit 1
fi

cd /opt/Superdoku
python3 -c "import SudokuExtractor; SudokuExtractor.parse_grid('$image_path')"
