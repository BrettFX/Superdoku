using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Superdoku
{
    public class PuzzleGrid : MonoBehaviour
    {
        [Header("Cells")]
        public GameObject[] cells;

        private GameObject m_currActiveCell;
        private GameObject m_prevActiveCell;

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
            Debug.Log("Selected cell: " + cell);
        }
    }
}
