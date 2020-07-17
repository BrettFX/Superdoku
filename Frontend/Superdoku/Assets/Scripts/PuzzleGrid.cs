using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Utils;

namespace Superdoku
{
    public class PuzzleGrid : MonoBehaviour
    {
        private static PuzzleGrid instance;
        public static PuzzleGrid Instance { get { return instance; } }

        private static readonly int[] TEST_PUZZLE =
        {
            0, 0, 0,    0, 0, 0,    0, 0, 0,
            0, 0, 0,    0, 0, 3,    0, 8, 5,
            0, 0, 1,    0, 2, 0,    0, 0, 0,

            0, 0, 0,    5, 0, 7,    0, 0, 0,
            0, 0, 4,    0, 0, 0,    1, 0, 0,
            0, 9, 0,    0, 0, 0,    0, 0, 0,

            5, 0, 0,    0, 0, 0,    0, 7, 3,
            0, 0, 2,    0, 1, 0,    0, 0, 0,
            0, 0, 0,    0, 4, 0,    0, 0, 9,
        };

        [Header("Cells")]
        public GameObject[] cellRows;
        private int[,] sudokuPuzzle;

        private volatile bool m_solvingState = false;
        private volatile int m_frameHeartbeat = 0;

        /**
         * Ensure this class remains a singleton instance
         * */
        void Awake()
        {
            // If the instance variable is already assigned...
            if (instance != null)
            {
                // If the instance is currently active...
                if (instance.gameObject.activeInHierarchy == true)
                {
                    // Warn the user that there are multiple Game Managers within the scene and destroy the old manager.
                    Debug.LogWarning("There are multiple instances of the PuzzleGrid script. Removing the old manager from the scene.");
                    Destroy(instance.gameObject);
                }

                // Remove the old manager.
                instance = null;
            }

            // Assign the instance variable as the Game Manager script on this object.
            instance = GetComponent<PuzzleGrid>();
        }

        // Start is called before the first frame update
        void Start()
        {
            // Initialize the sudoku puzzle
            // Attempt to load from prefs
            string puzzleStr = PlayerPrefs.GetString("ScannedPuzzle");
            sudokuPuzzle = new int[9, 9];
            GameManager.Instance.InitializePuzzle();

            // Load test puzzle if debug mode for testing
            if (GameManager.DEBUG_MODE)
            {
                sudokuPuzzle = BuildSudokuGrid(new List<int>(TEST_PUZZLE));
                Show(); // Invoke show method to render to puzzle UI
            }
            
            if (puzzleStr != null && puzzleStr != "")
            {
                // Delete the key so that a previously scanned puzzle isn't loaded all the time
                PlayerPrefs.DeleteKey("ScannedPuzzle");

                // Parse response to integer array
                SudokuPuzzle loadedPuzzle = JsonUtility.FromJson<SudokuPuzzle>(puzzleStr);
                sudokuPuzzle = BuildSudokuGrid(loadedPuzzle.puzzle);
                
                // Invoke show method to render to puzzle UI
                Show();
            }                        
        }

        // Update is called once per frame
        void Update()
        {
            // Keep track of frame changes by cycling through frames (resets to 0 every 100 frames to prevent integer overflow)
            m_frameHeartbeat = (m_frameHeartbeat + 1) % 100;

            // Only update if not currently running the solve process
            if (!m_solvingState)
            {
                // Keep the sudoku puzzle integer 2d array in sync with ui cells
                for (int row = 0; row < cellRows.Length; row++)
                {
                    // Break if the solving state was set to true before the initial catch
                    if (m_solvingState) break;

                    Button[] cellButtons = cellRows[row].GetComponentsInChildren<Button>();

                    for (int col = 0; col < cellButtons.Length; col++)
                    {
                        // Break if the solving state was set to true before the initial catch
                        if (m_solvingState) break;

                        Button cellButton = cellButtons[col];
                        string btnText = cellButton.GetComponentInChildren<Text>().text;

                        // Get integer representation
                        int btnValue = btnText.Equals("") ? 0 : Convert.ToInt32(btnText);
                        sudokuPuzzle[row, col] = btnValue;

                        // Only validate cells that aren't blank
                        if (btnValue != 0)
                        {
                            // Reference respective button colors
                            ColorBlock colors = cellButton.colors;
                            if (FollowsRowRule(row, col, btnValue) && FollowsColRule(col, row, btnValue) && FollowsSquareRule(row, col, btnValue))
                            {
                                // Revert back to default colors as needed
                                GameManager.Instance.SetButtonColor(cellButton, true);
                            }
                            else
                            {
                                // Highlight invalid cells accordingly
                                GameManager.Instance.SetButtonColor(cellButton, false);
                            }
                        }
                        else
                        {
                            // Revert back to default colors as needed (handles cornercase of clearing when formally invalid)
                            GameManager.Instance.SetButtonColor(cellButton, true);
                        }
                    }
                }
            }
        } 

        /**
	    * Determines if the number to be inserted has not already been used in the current row
	    * 
	    * @param row the row to be determined if the number to be inserted is valid
        * @param col the column used to pin-point the exact coordinate location of the cell
	    * @param num the number to determine is valid
	    * @return whether the number to be inserted is valid or not
	    * */
        public bool FollowsRowRule(int row, int col, int num)
        {
            for (int y = 0; y < 9; y++)
            {
                //If a number in the row already exists then the number in question does not satisfy the row rule
                if (sudokuPuzzle[row, y] == num && y != col)
                {
                    return false;
                }
            }
            return true;
        }

        /**
         * Determines if the number to be inserted has not already been used in the current column
         * 
         * @param col the column of the puzzle to be determined if valid
         * @param row the row provided as a means to pin-point the exact coordinate location of the cell
         * @param num the number to determine is valid
         * @return whether the number to be inserted is valid or not
         * */
        public bool FollowsColRule(int col, int row, int num)
        {
            for (int x = 0; x < 9; x++)
            {
                //If a number in the column already exists then the number in question does not satisfy the column rule
                if (sudokuPuzzle[x, col] == num && x != row)
                {
                    return false;
                }
            }

            return true;
        }

        /**
         * Determines if the number to be inserted has not already been used in the 3x3 subspace
         * that the current cell lives in
         * 
         * @param row the row in the 3x3 subspace of the puzzle to be determined if valid
         * @param col the column in the 3x3 subspace of the puzzle to be determined if valid
         * @param num the number to determine is valid
         * @return whether the number to be inserted is valid within the 3x3 subspace or not
         * */
        public bool FollowsSquareRule(int row, int col, int num)
        {
            int xStart, yStart;

            if (row >= 6)
            {
                xStart = 6;
            }
            else if (row >= 3)
            {
                xStart = 3;
            }
            else
            {
                xStart = 0;
            }

            if (col >= 6)
            {
                yStart = 6;
            }
            else if (col >= 3)
            {
                yStart = 3;
            }
            else
            {
                yStart = 0;
            }

            for (int x = xStart; x < xStart + 3; x++)
            {
                for (int y = yStart; y < yStart + 3; y++)
                {
                    if (x != row && y != col)
                    {
                        if (sudokuPuzzle[x, y] == num)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /**
	    * Driver method to solve the SuDoKu puzzle.
	    * Invokes a recursive delegate function to determine the correct numbers for each cell
	    * 
	    * @param row the current row within the 9x9 SuDoKu puzzle
	    * @param col the current column  within the 9x9 SuDoKu puzzle
	    * */
        public void SolvePuzzle(int row, int col)
        {
            // Need to set the solving state to prevent conflicting with the update function
            m_solvingState = true;
            int currentFrame = m_frameHeartbeat;
            
            // Wait for end of frame so that the update function doesn't render a part of the solving process
            // (Equivalent to WaitForEndOfFrame)
            while (currentFrame == m_frameHeartbeat);

            SolvePuzzleRecursive(row, col);
            m_solvingState = false;
        }

        /**
         * Recursive delegate function to solve the sudoku puzzle using the
         * classic backtacking algorithm.
         * 
         * @param row the current row within the 9x9 SuDoKu puzzle
	     * @param col the current column  within the 9x9 SuDoKu puzzle
         */
        private void SolvePuzzleRecursive(int row, int col)
        {
            if (col > 8)
            {
                col = 0;
                row++;
            }

            //Puzzle solved (we reached the last cell without any errors)
            if (row > 8 || GameManager.Instance.IsSolved())
            {
                GameManager.Instance.SetSolved(true);
                return;
            }

            if (sudokuPuzzle[row, col] == 0)
            {
                //Try each number 1-9 until it satisfies all three rules
                for (int num = 1; num <= 9; num++)
                {
                    if (FollowsRowRule(row, col, num) && FollowsColRule(col, row, num) && FollowsSquareRule(row, col, num))
                    {
                        sudokuPuzzle[row, col] = num;
                        SolvePuzzleRecursive(row, col + 1);

                        if (GameManager.Instance.IsSolved())
                            return;

                        sudokuPuzzle[row, col] = 0;
                    }
                }
            }
            else
            {
                //Skip cells that already have numbers 1-9
                SolvePuzzleRecursive(row, col + 1);
            }
        }

        public bool IsValidSolution()
        {
            bool valid = true;

            // Itereate through the current sudoku puzzle and confirm that there isn't a zero in any cell
            // if there is a zero then we know that solution is not valid
            for (int row = 0; row < 9; row++)
            {
                if (!valid)
                {
                    break;
                }

                for (int col = 0; col < 9; col++)
                {
                    // As soon as a cell with 0 is found, the puzzle is invalid
                    if (sudokuPuzzle[row, col] == 0)
                    {
                        valid = false;
                        break;
                    }
                }
            }

            return valid;
        }

        /**
        * Clears the in-memory puzzle grid by setting each element to 0 and sets the text
        * of UI grid cells to blank
        */
        public void Clear()
        {  
            for (int row = 0; row < cellRows.Length; row++)
            {
                Button[] cellButtons = cellRows[row].GetComponentsInChildren<Button>();

                for (int col = 0; col < cellButtons.Length; col++)
                {
                    // Clear the respective UI cell and sudoku puzzle cell
                    Text btnText = cellButtons[col].GetComponentInChildren<Text>();
                    btnText.text = "";
                    sudokuPuzzle[row, col] = 0;
                }
            }
        }

        /**
        * Display the Sudoku puzzle based on the current values of the 2d integer array
        * representation. Update the UI grid cells.
        */
        public void Show()
        {
            // String to build while iterating sudoku puzzle for displaying/logging
            string display = "";

            for (int row = 0; row < cellRows.Length; row++)
            {
                Button[] cellButtons = cellRows[row].GetComponentsInChildren<Button>();
                display += "\n";
                for (int col = 0; col < cellButtons.Length; col++)
                {
                    // Set the cell grid ui text to the respective cell value from 2d int array
                    Text btnText = cellButtons[col].GetComponentInChildren<Text>();
                    int cellVal = sudokuPuzzle[row, col];
                    btnText.text = cellVal == 0 ? "" : cellVal.ToString(); // Display 0 as an empty string

                    // Print out a divider every third line
                    display += ((col % 3 == 0) ? "\t" : " ") + sudokuPuzzle[row, col].ToString();
                }

                display += ((row + 1) % 3 == 0) ? "\n" : "";
            }

            if (GameManager.DEBUG_MODE) { Debug.Log(display); }
        }
    }
}
