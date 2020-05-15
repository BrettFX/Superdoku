using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Superdoku
{
    public class ObjectClicker : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            
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
                        // Delegate to the on cell select function defined in game manager
                        GameManager.Instance.OnCellSelected(hit.transform.gameObject);
                    }
                }
            }
        }
    }
}


