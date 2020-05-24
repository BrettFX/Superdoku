using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Superdoku
{
    public class ImageProcessor : MonoBehaviour
    {
        public string imagePath;

        // Start is called before the first frame update
        void Start()
        {
            // TODO load image path from player prefs
            //imagePath = PlayerPrefs.GetString(GameManager.IMAGE_PATH_KEY);
            imagePath = "C:/Users/Brett/AppData/Local/Superdoku/test.png";
            GetComponent<RawImage>().texture = GameManager.LoadPNG(imagePath);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
