using SimpleFileBrowser;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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

        public const bool DEBUG_MODE = true;
        public const int HOME_SCENE = 0;
        public const int WEB_CAM_SCENE = 1;
        public const int IMAGE_PROCESSOR_SCENE = 2;
        public const string IMAGE_PATH_KEY = "CurrentImagePath";

        [Header("Cell Management")]
        public Color defaultColor = Color.white;
        public Color defaultTextColor = Color.black;
        public Color selectionColor = Color.blue;
        public Color invalidCellColor = Color.red;
        public Color invalidTextColor = Color.white;
        public Color invalidSelectionColor = Color.yellow;

        [Header("Miscellaneous")]
        public GameObject modalOverlay;
        
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

        void Start()
        {
            InitializeFileBrowser();
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
         * Perform setup tasks for SimpleFileBrowser plugin to prepare for loading
         * photos
         */
        public void InitializeFileBrowser()
        {
            // Set filters (optional)
            // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
            // if all the dialogs will be using the same filters
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png"), new FileBrowser.Filter("Text Files", ".txt", ".pdf"));

            // Set default filter that is selected when the dialog is shown (optional)
            // Returns true if the default filter is set successfully
            // In this case, set Images filter as the default filter
            FileBrowser.SetDefaultFilter(".png");

            // Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
            // Note that when you use this function, .lnk and .tmp extensions will no longer be
            // excluded unless you explicitly add them as parameters to the function
            FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

            // Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
            // It is sufficient to add a quick link just once
            // Name: Users
            // Path: C:\Users
            // Icon: default (folder icon)
            FileBrowser.AddQuickLink("Users", "C:\\Users", null);

            // Show a save file dialog 
            // onSuccess event: not registered (which means this dialog is pretty useless)
            // onCancel event: not registered
            // Save file/folder: file, Initial path: "C:\", Title: "Save As", submit button text: "Save"
            // FileBrowser.ShowSaveDialog( null, null, false, "C:\\", "Save As", "Save" );

            // Show a select folder dialog 
            // onSuccess event: print the selected folder's path
            // onCancel event: print "Canceled"
            // Load file/folder: folder, Initial path: default (Documents), Title: "Select Folder", submit button text: "Select"
            // FileBrowser.ShowLoadDialog( (path) => { Debug.Log( "Selected: " + path ); }, 
            //                                () => { Debug.Log( "Canceled" ); }, 
            //                                true, null, "Select Folder", "Select" );

            
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

        public void OnModalOverlay()
        {
            modalOverlay.SetActive(true);
        }

        /**
         * Launch the camera scene to parse an image and solve it
         */
        public void OnCamera() {
            // Deselect current active cell
            m_currentActiveCell = null;
            modalOverlay.SetActive(false);
            if (DEBUG_MODE) { Debug.Log("Launching camera scene..."); }

            // Change to web cam scene
            SceneManager.LoadScene(WEB_CAM_SCENE);
        }

        public void OnGallery()
        {
            // Deselect current active cell
            m_currentActiveCell = null;
            modalOverlay.SetActive(false);
            if (DEBUG_MODE) { Debug.Log("Launching gallery scene..."); }

            // Open file chooser dialog (using SimpleFileBrowser)
            StartCoroutine(ShowLoadDialogCoroutine());
        }

        public void OnCancelModalOverlay()
        {
            modalOverlay.SetActive(false);
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

        /**
         * Write a file to the local file system provided with the bytes data of the respecive file
         
         * @param string path the base path to where the file is to be written
         * @param string fileName the name of the file to be written
         * @param byte[] bytes the array of bytes representing the data of the file
         * @param FileMode mode the writing strategy to perform on the file
         * 
         * @see https://answers.unity.com/questions/1397703/system-io-file-directory-problem.html
         * @see System.IO.FileMode
         */
        public static bool WriteFile(string path, string fileName, byte[] bytes, FileMode mode)
        {
            bool retValue = false;
            string dataPath = path;

            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            dataPath += fileName;
            try
            {
                File.WriteAllBytes(dataPath, bytes);
                retValue = true;
            }
            catch (System.Exception ex)
            {
                string ErrorMessages = "File Write Error\n" + ex.Message;
                retValue = false;
                Debug.LogError(ErrorMessages);
            }
            return retValue;
        }

        public static Texture2D LoadPNG(string filePath)
        {

            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2, TextureFormat.BGRA32, false);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }

            return tex;
        }

        /**
         * Coroutine for SimpleFileBrowser to show file loading dialog. Yields so that
         * Game waits for response
         */
        IEnumerator ShowLoadDialogCoroutine()
        {
            // Show a load file dialog and wait for a response from user
            // Load file/folder: file, Initial path: default (Documents), Title: "Load File", submit button text: "Load"
            yield return FileBrowser.WaitForLoadDialog(false, null, "Load File", "Load");

            // Dialog is closed
            // Print whether a file is chosen (FileBrowser.Success)
            // and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)
            Debug.Log(FileBrowser.Success + " " + FileBrowser.Result);

            if (FileBrowser.Success)
            {
                // If a file was chosen, read its bytes via FileBrowserHelpers
                // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
                byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result);
            }
        }
    }
}