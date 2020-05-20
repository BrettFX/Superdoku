using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Superdoku
{
    [Serializable]
    public class Row
    {
        public GameObject[] rowdata;
    }

    public class PuzzleGrid : MonoBehaviour
    {
        private static PuzzleGrid instance;
        public static PuzzleGrid Instance { get { return instance; } }

        [Header("Cells")]
        //public Row[] cells;
        public GameObject[] cellRows;

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
            Show();
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void Show()
        {
            // String to build while iterating sudoku puzzle for displaying/logging
            string displayStr = "";

            for (int row = 0; row < cellRows.Length; row++)
            {
                Button[] cellButtons = cellRows[row].GetComponentsInChildren<Button>();

                for (int col = 0; col < cellButtons.Length; col++)
                {
                    string btnText = cellButtons[col].GetComponentInChildren<Text>().text;

                    // Get integer representation
                    int btnValue = btnText.Equals("") ? 0 : Convert.ToInt32(btnText);

                    //Print out a divider every third line
                    displayStr += (col % 3 == 0) ? "\t" : " ";
                    displayStr += btnValue.ToString();
                    //Debug.Log("Cell at (" + row + ", " + col + ") value: " + btnValue);
                }

                displayStr += ((row + 1) % 3 == 0) ? "\n" : "";
            }

            Debug.Log(displayStr);
        }
    }
}
