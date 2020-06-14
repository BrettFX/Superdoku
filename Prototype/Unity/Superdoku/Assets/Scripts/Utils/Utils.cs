using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    [System.Serializable]
    public class SudokuPuzzle
    {
        public List<int> puzzle;
    }
    
    /**
     * Build a 2-dimentional representation of a 1-dimensional Sudoku puzzle
     * array
     * @param List<int> puzzle the 1-dimensional Sudoku puzzle to convert
     * @return the 2-dimensional grid representation
     */
    public static int[,] BuildSudokuGrid(List<int> puzzle)
    {
        // Get 2d version of test sudoku
        int bounds = 9;
        int puzzleCount = puzzle.Count;

        Debug.Log("Rows: " + bounds + ", Columns: " + bounds);

        int[,] grid = new int[bounds, bounds];
        int i = 0;
        for (int x = 0; x < 9; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                if (i < puzzleCount)
                {
                    grid[x, y] = puzzle[i];
                }
                i++;
            }
        }

        return grid;
    }
}
