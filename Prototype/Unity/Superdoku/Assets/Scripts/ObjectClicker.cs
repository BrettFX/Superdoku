using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Superdoku
{
    public class ObjectClicker : MonoBehaviour
    {
        [Header("Base Colors")]
        public Color defaultColor = Color.white;
        public Color selectionColor = Color.blue;

        private bool m_selected = false;

        private Color[] m_colorWheel;

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
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, 100.0f))
                {
                    if (hit.transform != null)
                    {
                        //Debug.Log("Clicked: " + hit.transform.gameObject.name);

                        // Change material color on click
                        ToggleSelectionColor(hit.transform.gameObject);

                        // Delegate to the on cell select function defined in game manager
                        GameManager.Instance.OnCellSelected(hit.transform.gameObject);
                    }
                }
            }
        }

        /**
        * Toggle the selection color of a game object based on two colors to cycle between
        * @param GameObject obj the game object to extract the renderer from and alter the material color
        */
        public void ToggleSelectionColor(GameObject obj)
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


