using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Superdoku {
    public class GameManager: MonoBehaviour {
        // Static Refs //
        private static GameManager instance;
        public static GameManager Instance {
            get {
                return instance;
            }
        }

        [Header("Development")]
        public bool DEBUG_MODE = false;

        [Header("Cell Management")]
        public Color defaultColor = Color.white;
        public Color defaultTextColor = Color.black;
        public Color selectionColor = Color.blue;
        public Color invalidCellColor = Color.red;
        public Color invalidTextColor = Color.white;
        public Color invalidSelectionColor = Color.yellow;
        
        private bool m_solved = false;

        private static GameObject m_currentActiveCell;

        /**
         * Ensure this class remains a singleton instance
         * */
        void Awake() {
            // If the instance variable is already assigned...
            if (instance != null) {
                // If the instance is currently active...
                if (instance.gameObject.activeInHierarchy == true) {
                    // Warn the user that there are multiple Game Managers within the scene and destroy the old manager.
                    Debug.LogWarning("There are multiple instances of the Game Manager script. Removing the old manager from the scene.");
                    Destroy(instance.gameObject);
                }

                // Remove the old manager.
                instance = null;
            }

            // Assign the instance variable as the Game Manager script on this object.
            instance = GetComponent < GameManager > ();
        }

        /**
        * Sets all contents of the sudokuPuzzle to zero and clears all text fields as long as
        * they are loaded
        * */
        public void InitializePuzzle()
        {
            m_currentActiveCell = null;
            PuzzleGrid.Instance.Clear();
            SetSolved(false);
            if (DEBUG_MODE) { Debug.Log("Puzzle initialized"); }
        }

        /**
         * Set cell selection and keep track of selected cell
         * @param GameObject cell that has been selected
         */
        public void OnCellSelected(GameObject cell) {
            if (DEBUG_MODE)
            {
                Debug.Log("Selected cell: " + cell.name);
            }

            Button button = cell.GetComponent < Button > ();
            m_currentActiveCell = cell;
        }

        /**
         * Write the respective number value of the button clicked to the
         * currently selected cell. If no cell is selected, then the number will not
         * be written
         * @param GameObject button the number button that was clicked
         */
        public void OnNumberClicked(GameObject button) {
            // Grab the respective number value of the clicked button
            Text buttonText = button.GetComponentInChildren < Text > ();

            // Set the text of the currently selected input field based on the number that has been clicked
            if (m_currentActiveCell != null) {
                Button targetCell = m_currentActiveCell.GetComponent < Button > ();
                targetCell.GetComponentInChildren < Text > ().text = buttonText.text;

                // Highlight the target cell since it lost focus
                targetCell.Select();
            } else {
                if (DEBUG_MODE) { Debug.Log("Must select a cell first!"); }
            }
        }

        /**
         * Solve the Sudoku puzzle when the Solve button is clicked
         * */
        public void OnSolve() {
            if (DEBUG_MODE) { Debug.Log("Solving puzzle..."); }

            float startTime, endTime;

            if (DEBUG_MODE) { Debug.Log("Here is the problem:\n"); }
            PuzzleGrid.Instance.Show();

            startTime = Time.deltaTime * 1000.0f;

            //Begin the SuDoKu-solving algorithm at the beginning of the 9x9 matrix
            PuzzleGrid.Instance.SolvePuzzle(0, 0);

            if (DEBUG_MODE) { Debug.Log("\nHere is the solution:\n"); }
            PuzzleGrid.Instance.Show();

            endTime = Time.deltaTime * 1000.0f;
            if (DEBUG_MODE) { Debug.Log("Solution took " + (endTime - startTime) + " millisecond(s) to derive."); }

            // Deselect current active cell
            m_currentActiveCell = null;
        }

        /**
         * Mutate solved state
         * */
        public void SetSolved(bool b)
        {
            m_solved = b;
        }

        /**
         * Get solved state
         */
        public bool IsSolved()
        {
            return m_solved;
        }

        /**
         * Launch the camera scene to parse an image and solve it
         */
        public void OnCamera() {
            if (DEBUG_MODE) { Debug.Log("Launching camera scene..."); }

            // Deselect current active cell
            m_currentActiveCell = null;
        }

        /**
        * Clears all the text field cells in the 9x9 matrix
        * */
        public void OnClearCells() {
            InitializePuzzle();
        }

        /**
         * Dynamically change a button's color based on validity of the respective
         * cell it belongs to
         * @param Button button the button to change the color of
         * @param valid whether the respective cell the button belongs to is valid or not
         */
        public void SetButtonColor(Button button, bool valid)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = valid ? defaultColor : invalidCellColor;
            colors.highlightedColor = valid ? defaultColor : invalidCellColor;
            colors.selectedColor = valid ? selectionColor : invalidSelectionColor;
            colors.pressedColor = valid ? selectionColor : invalidCellColor;

            // Set the text color based on selection state
            Text text = button.GetComponentInChildren<Text>();
            GameObject selectedObj = EventSystem.current.currentSelectedGameObject;
            if (selectedObj != null)
            {
                // If a cell is selected and it happens to be this button then change the text color accordingly
                Button selectedButton = selectedObj.GetComponentInChildren<Button>();
                text.color = !valid && button.Equals(selectedButton) ? invalidTextColor : defaultTextColor;
            }
            else 
            {
                // Otherwise set the text color to the default color
                text.color = defaultTextColor;
            }
            

            // Commit the button color change
            button.colors = colors;
        }
    }
}