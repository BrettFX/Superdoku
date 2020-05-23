using System.Collections;
using System.Collections.Generic;
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

		[Header("Cell Management")]
		public Color defaultColor = Color.white;
		public Color selectionColor = Color.blue;

		private bool m_selected = false;
        private bool m_solved = false;

		private Color[] m_colorWheel;

		private static GameObject m_currentActiveCell;

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

		// Start is called before the first frame update
		void Start() {
			// Selection color wheel for toggling between selected and deselected
			m_colorWheel = new Color[2] {
				defaultColor,
				selectionColor
			};

            PuzzleGrid.Instance.Show();
		}

		// Update is called once per frame
		void Update() {

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
            Debug.Log("Puzzle initialized");
        }

        public void OnCellSelected(GameObject cell) {
			Debug.Log("Selected cell: " + cell.name);

			Button button = cell.GetComponent < Button > ();
			m_currentActiveCell = cell;
		}

		public void OnNumberClicked(GameObject button) {
			// Grab the respective number value of the clicked button
			Text buttonText = button.GetComponentInChildren < Text > ();

			// Set the text of the currently selected input field based on the number that has been clicked
			if (m_currentActiveCell != null) {
				Button targetCell = m_currentActiveCell.GetComponent < Button > ();
				Debug.Log(targetCell);

				targetCell.GetComponentInChildren < Text > ().text = buttonText.text;

                // TODO Highlight cell color as red if it's not a valid number according to Sudoku rules
                //ValidateInput();

				// Highlight the target cell since it lost focus
				targetCell.Select();
			} else {
				Debug.Log("Must select a cell first!");
			}
		}

		public void OnSolve() {
			Debug.Log("Solving puzzle...");

            float startTime, endTime;

            Debug.Log("Here is the problem:\n");
            PuzzleGrid.Instance.Show();

            startTime = Time.deltaTime * 1000;

            //Begin the SuDoKu-solving algorithm at the beginning of the 9x9 matrix
            PuzzleGrid.Instance.SolvePuzzle(0, 0);

            Debug.Log("\nHere is the solution:\n");
            PuzzleGrid.Instance.Show();

            endTime = Time.deltaTime * 1000;
            Debug.Log("Solution took " + (endTime - startTime) + " millisecond(s) to derive.");

            // Deselect current active cell
            m_currentActiveCell = null;
		}

        public void SetSolved(bool b)
        {
            m_solved = b;
        }

        public bool IsSolved()
        {
            return m_solved;
        }

		public void OnCamera() {
			Debug.Log("Launching camera scene...");

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
        * Toggle the selection color of a game object based on two colors to cycle between
        * @param GameObject obj the game object to extract the renderer from and alter the material color
        */
		private void ToggleSelectionColor(GameObject obj) {
			// Toggle selection state (change before processing for onClick change)
			m_selected = !m_selected;

			// Choose the respective color from the color wheel based on the selection state
			Color color = m_colorWheel[m_selected ? 1 : 0];

			// Obtain the renderer from the game object and set its material color
			Renderer rend = obj.GetComponent < Renderer > ();
			rend.material.color = color;
		}
	}
}