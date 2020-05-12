using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Superdoku
{
    public class GameManager : MonoBehaviour
    {
        private InputField m_currActiveCell;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnCellSelected(InputField cell)
        {
            Debug.Log("Selected cell: " + cell);
            m_currActiveCell = cell;
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
