using ExifLib;
using Kakera;
using System.Collections;
using System.IO;
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
        public const int IMAGE_PROCESSOR_SCENE = 1;
        public const string IMAGE_PATH_KEY = "CurrentImagePath";

        [Header("Cell Management")]
        public Color defaultColor = Color.white;
        public Color defaultTextColor = Color.black;
        public Color selectionColor = Color.blue;
        public Color invalidCellColor = Color.red;
        public Color invalidTextColor = Color.white;
        public Color invalidSelectionColor = Color.yellow;

        [Header("Modals")]
        public GameObject cameraModal;
        public GameObject errorModal;
        public GameObject loadingModal;

        [Header("Plugins")]
        public Unimgpicker imagePicker;

        [Header("Error Handle")]
        public Text errorMsgText;

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

            // Add the Completed handle function for when the user has selected an image
            imagePicker.Completed += (string path) =>
            {
                StartCoroutine(ShowLoadDialogCoroutine(path));
            };
        }

        void Start()
        {
            // Turn off loading modal if it's active
            ToggleLoadingModal(false);

            // Handle case when rest request error occurred
            string requestError = PlayerPrefs.GetString("RestRequestError");
            if (requestError != null && requestError != "")
            {
                // Display appropriate error modal
                DisplayError(requestError);

                // Remove RestRequestError key from player prefs
                PlayerPrefs.DeleteKey("RestRequestError");
            }
        }

        /**
         * Display an error message using the error modal
         */
        public void DisplayError(string msg)
        {
            errorMsgText.text = msg;
            errorModal.SetActive(true);
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

            // Validate solved puzzle to ensure that a solution was found (e.g., solution exists)
            if (PuzzleGrid.Instance.IsValidSolution())
            {
                if (DEBUG_MODE) { Debug.Log("\nHere is the solution:\n"); }
                PuzzleGrid.Instance.Show();

                endTime = Time.deltaTime * 1000.0f;
                if (DEBUG_MODE) { Debug.Log("Solution took " + (endTime - startTime) + " millisecond(s) to derive."); }
            }
            else
            {
                Debug.Log("Solution not valid. No solution possible.");
                // Set error message text
                string msg = "A solution does not exist for the given Sudoku puzzle. " +
                             "Please make sure the puzzle has been entered or loaded" +
                             " correctly and try to solve again.";
                errorMsgText.text = msg;
                errorModal.SetActive(true);
            }

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

        public void OnModalOverlay(GameObject modalOverlay)
        {
            modalOverlay.SetActive(true);
        }

        /**
         * Launch the camera scene to parse an image and solve it
         */
        public void OnCamera() {
            // Deselect current active cell
            m_currentActiveCell = null;
            cameraModal.SetActive(false);

            // Display appropriate error if the device doesn't have network connectivity
            if (IsNetworkDisconnected())
            {
                DisplayError("Network is disconnected. This feature requires network connectivity." +
                    " Please connect to the Internet.");
                return;
            }

            if (DEBUG_MODE) { Debug.Log("Launching camera scene..."); }

            // Don't attempt to use the camera if it is already open
            if (NativeCamera.IsCameraBusy())
                return;

            // Take a picture with the camera
            // Don't enforce any limit on the size of the image to ensure the best image quality
            TakePicture(int.MaxValue);
        }

        public void OnGallery()
        {
            // Deselect current active cell
            m_currentActiveCell = null;
            cameraModal.SetActive(false);

            // Display appropriate error if the device doesn't have network connectivity
            if (IsNetworkDisconnected())
            {
                DisplayError("Network is disconnected. This feature requires network connectivity." +
                    " Please connect to the Internet.");
                return;
            }

            if (DEBUG_MODE) { Debug.Log("Launching gallery scene..."); }

            // Open image picker (using Unimgpicker)
            imagePicker.Show("Select Image", "unimgpicker", int.MaxValue);
        }

        public void OnCancelModalOverlay(GameObject modalOverlay)
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

        public static bool IsNetworkDisconnected()
        {
            return Application.internetReachability == NetworkReachability.NotReachable;
        }

        /**
         * Rotate a given Texture2D 90 degrees either clockwise or counterclockwise based on the 
         * value of the respective clockwise flag.
         */
        public static Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
        {
            Color32[] original = originalTexture.GetPixels32();
            Color32[] rotated = new Color32[original.Length];
            int w = originalTexture.width;
            int h = originalTexture.height;

            int iRotated, iOriginal;

            for (int j = 0; j < h; ++j)
            {
                for (int i = 0; i < w; ++i)
                {
                    iRotated = (i + 1) * h - j - 1;
                    iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                    rotated[iRotated] = original[iOriginal];
                }
            }

            Texture2D rotatedTexture = new Texture2D(h, w);
            rotatedTexture.SetPixels32(rotated);
            rotatedTexture.Apply();
            return rotatedTexture;
        }

        /**
         * Inspect a given Texture2D to ensure that it has the correct orientation.
         * If it is not in top-left orientation then perform the necessary correction to
         * make it top-left
         */
        public static Texture2D CorrectRotation(Texture2D texture, string orientationString)
        {
            switch (orientationString)
            {
                case "TopRight": // Rotate clockwise 90 degrees
                    texture = RotateTexture(texture, true);
                    break;
                case "TopLeft": // Rotate 0 degrees...
                    break;
                case "BottomRight": // Rotate clockwise 180 degrees
                    texture = RotateTexture(texture, true);
                    texture = RotateTexture(texture, true);
                    break;
                case "BottomLeft": // Rotate clockwise 270 degrees
                    texture = RotateTexture(texture, true);
                    texture = RotateTexture(texture, true);
                    break;
                default:
                    break;
            }

            return texture;
        }

        /**
         * Set the loading animation to start or stop based on the specified boolean flag
         */
        private void ToggleLoadingModal(bool b)
        {
            loadingModal.SetActive(b);
            Animator loadingAnimator = loadingModal.GetComponentInChildren<RawImage>().GetComponent<Animator>();
            loadingAnimator.SetTrigger(b ? "LoadingStart" : "LoadingStop");
        }

        /**
         * Take a picture using the NativeCamera plugin.
         * The specified max size is used as a threshold for keeping the snapped picture
         * within the bounds of the max size
         */
        private void TakePicture(int maxSize)
        {
            // Temporarilty show loading modal before transitioning to scanning
            ToggleLoadingModal(true);
            NativeCamera.Permission permission = NativeCamera.TakePicture((path) =>
            {
                Debug.Log("Image path: " + path);
                if (path != null)
                {
                    PlayerPrefs.SetString(IMAGE_PATH_KEY, path);
                    SceneManager.LoadScene(IMAGE_PROCESSOR_SCENE);
                    //StartCoroutine(ShowLoadDialogCoroutine(path));
                }
            }, maxSize);
            
            Debug.Log("Permission result: " + permission);
        }

        /**
         * Simple file loading utility to load an image at a given path to a Texture2D object 
         */
        public static byte[] GetImageData(string filePath)
        {
            byte[] fileData = null;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
            }

            return fileData;
        }

        /**
         * Process provided texture data to be in a state suitable for image processing
         * to be sent to the Superdoku REST api. One of the core steps of this function
         * is to handle the case when the orientation of a jpeg image is not in portrait. This
         * function handles reading the respective jpeg image's Exchange Image File (EXIF) data
         * to determine the proper course of action based on the state of the orientation key.
         */
        public static RequestContent ProcessAndBuildRequest(byte[] textureData)
        {
            Texture2D tex = new Texture2D(2, 2, TextureFormat.BGRA32, false);
            tex.LoadImage(textureData);

            // Build a request content object
            RequestContent requestContent = new RequestContent();

            Debug.Log("Image Size (in bytes): " + textureData.Length.ToString());
            JpegInfo jpi = ExifReader.ReadJpeg(textureData, "LoadedFile");

            // Determine if the provided file was a jpg or not
            Debug.Log("Loaded image is in " + (jpi.IsValid ? "JPG" : "PNG") + " format.");

            // If it's a jpg image then we need to do some preprocessing before invoking the rest request
            if (jpi.IsValid)
            {
                double[] Latitude = jpi.GpsLatitude;
                double[] Longitude = jpi.GpsLongitude;
                string orientationString = jpi.Orientation.ToString();

                string exifDataStr = "<b>Exif Data:</b>" + "<color=white>";
                exifDataStr = exifDataStr + "\n" + "FileName: " + jpi.FileName;
                exifDataStr = exifDataStr + "\n" + "DateTime: " + jpi.DateTime;
                exifDataStr = exifDataStr + "\n" + "GpsLatitude: " + Latitude[0] + "° " + Latitude[1] + "' " + Latitude[2] + '"';
                exifDataStr = exifDataStr + "\n" + "GpsLongitude: " + Longitude[0] + "° " + Longitude[1] + "' " + Longitude[2] + '"';
                exifDataStr = exifDataStr + "\n" + "Description: " + jpi.Description;
                exifDataStr = exifDataStr + "\n" + "Height: " + jpi.Height + " pixels";
                exifDataStr = exifDataStr + "\n" + "Width: " + jpi.Width + " pixels";
                exifDataStr = exifDataStr + "\n" + "ResolutionUnit: " + jpi.ResolutionUnit;
                exifDataStr = exifDataStr + "\n" + "UserComment: " + jpi.UserComment;
                exifDataStr = exifDataStr + "\n" + "Make: " + jpi.Make;
                exifDataStr = exifDataStr + "\n" + "Model: " + jpi.Model;
                exifDataStr = exifDataStr + "\n" + "Software: " + jpi.Software;
                exifDataStr = exifDataStr + "\n" + "Orientation: " + orientationString;
                exifDataStr = exifDataStr + "</color>";

                if (DEBUG_MODE) { Debug.Log(exifDataStr); }

                // Assure the corrent orientation of the provided jpg image
                tex = CorrectRotation(tex, jpi.Orientation.ToString());
                requestContent.data = tex.EncodeToJPG();
                requestContent.filetype = "jpg";
            }
            else
            {
                // Otherwise the image isn't a jpg and we can process it normally
                requestContent.data = tex.EncodeToPNG();
                requestContent.filetype = "png";
            }

            return requestContent;
        }

        /**
         * Call the respective REST endpoint for recognizing the provided data within the RequestContent
         * instance.
         */
        public static void SendTexture(RequestContent requestContent)
        {
            // Send the file data to the superdoku api to recognize and classifiy its digits
            RestRequest.Instance.SendRequest(string.Format(RestRequest.BASE_URL, "recognize"), "PUT", requestContent);
        }

        /**
         * Coroutine to show file loading dialog while waiting for file to be loaded and sent
         * to the Superdoku REST api for a response. Yields so that Game waits for response.
         */
        IEnumerator ShowLoadDialogCoroutine(string path)
        {
            Debug.Log("Processing file: " + path);
            ToggleLoadingModal(true);

            yield return new WaitForEndOfFrame();

            PlayerPrefs.SetString(IMAGE_PATH_KEY, path);
            SceneManager.LoadScene(IMAGE_PROCESSOR_SCENE);
        }
    }
}