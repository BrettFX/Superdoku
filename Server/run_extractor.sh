image_path=$1

if [ -z $image_path ]; then
    echo "1st argument should be a path to an image file"
    exit 1
fi

cd /opt/Superdoku
python3 -c "import SudokuExtractor; SudokuExtractor.parse_grid('$image_path')"
