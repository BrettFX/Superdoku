using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Superdoku
{
    public class GameManager : MonoBehaviour
    {
        // Static Refs //
        private static GameManager instance;
        public static GameManager Instance { get { return instance; } }

        [Header("Cell Management")]
        public Color defaultColor = Color.white;
        public Color selectionColor = Color.blue;

        private bool m_selected = false;

        private Color[] m_colorWheel;

        private static GameObject m_currentActiveCell;

        void Awake()
        {
            // If the instance variable is already assigned...
            if (instance != null)
            {
                // If the instance is currently active...
                if (instance.gameObject.activeInHierarchy == true)
                {
                    // Warn the user that there are multiple Game Managers within the scene and destroy the old manager.
                    Debug.LogWarning("There are multiple instances of the Game Manager script. Removing the old manager from the scene.");
                    Destroy(instance.gameObject);
                }

                // Remove the old manager.
                instance = null;
            }

            // Assign the instance variable as the Game Manager script on this object.
            instance = GetComponent<GameManager>();
        }

        // Start is called before the first frame update
        void Start()
        {
            // Selection color wheel for toggling between selected and deselected
            m_colorWheel = new Color[2]
            {
                defaultColor,
                selectionColor
            };
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnCellSelected(GameObject cell)
        {
            Debug.Log("Selected cell: " + cell.name);

            Button button = cell.GetComponent<Button>();
            m_currentActiveCell = cell;
        }

        public void OnNumberClicked(GameObject button)
        {
            // Grab the respective number value of the clicked button
            Text buttonText = button.GetComponentInChildren<Text>();

            // Set the text of the currently selected input field based on the number that has been clicked
            if (m_currentActiveCell != null)
            {
                Button targetCell = m_currentActiveCell.GetComponent<Button>();
                Debug.Log(targetCell);

                targetCell.GetComponentInChildren<Text>().text = buttonText.text;

                // Highlight the target cell since it lost focus
                targetCell.Select();
            }
            else
            {
                Debug.Log("Must select a cell first!");
            }
        }

        // See: https://forum.unity.com/threads/unity-4-6-ui-how-to-prevent-deselect-losing-focus-in-inputfield.272387/
        void ReactivateInputField(InputField inputField)
        {
            if (inputField != null)
            {
                StartCoroutine(ActivateInputFieldWithoutSelection(inputField));
            }
        }

        IEnumerator ActivateInputFieldWithoutSelection(InputField inputField)
        {
            inputField.ActivateInputField();
            // wait for the activation to occur in a lateupdate
            yield return new WaitForEndOfFrame();
            // make sure we're still the active ui
            if (EventSystem.current.currentSelectedGameObject == inputField.gameObject)
            {
                // To remove hilight we'll just show the caret at the end of the line
                inputField.MoveTextEnd(false);
            }
        }

        /**
        * Toggle the selection color of a game object based on two colors to cycle between
        * @param GameObject obj the game object to extract the renderer from and alter the material color
        */
        private void ToggleSelectionColor(GameObject obj)
        {
            // Toggle selection state (change before processing for onClick change)
            m_selected = !m_selected;

            // Choose the respective color from the color wheel based on the selection state
            Color color = m_colorWheel[m_selected ? 1 : 0];

            // Obtain the renderer from the game object and set its material color
            Renderer rend = obj.GetComponent<Renderer>();
            rend.material.color = color;
        }
    }
}
