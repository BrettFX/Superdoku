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

        private string m_currActiveCellName;

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

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnCellSelected(GameObject cell)
        {
            Debug.Log("Selected cell: " + cell.name);
            this.m_currActiveCellName = cell.name;
        }

        public void OnNumberClicked(GameObject button)
        {
            // Grab the respective number value of the clicked button
            NumberButton numberButton = button.GetComponent<NumberButton>();
            Debug.Log("Button clicked: " + numberButton.value);

            // Set the text of the currently selected input field based on the number that has been clicked
            if (this.m_currActiveCellName != null)
            {
                // Lookup by name
                GameObject gameObject = GameObject.Find(this.m_currActiveCellName);
                InputField targetInputField = gameObject.GetComponentInChildren<InputField>();

                Debug.Log(gameObject);
                Debug.Log(targetInputField);

                targetInputField.text = numberButton.value.ToString();

                // Highlight the input field again since it lost focus
                targetInputField.ActivateInputField();
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
    }
}
