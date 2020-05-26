using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Superdoku
{
    public class ImageProcessor : MonoBehaviour
    {
        public string imagePath;

        // Start is called before the first frame update
        void Start()
        {
            // Load image path from player prefs
            imagePath = PlayerPrefs.GetString(GameManager.IMAGE_PATH_KEY);
            Texture2D pngImage = GameManager.LoadPNG(imagePath);
            if (pngImage != null)
            {
                GetComponent<RawImage>().texture = GameManager.LoadPNG(imagePath);
            }
            else
            {
                if (GameManager.DEBUG_MODE) { Debug.LogWarning("Could not load image to process. Navigating back to home"); }
                SceneManager.LoadScene(GameManager.HOME_SCENE);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnBack()
        {   
            // Change to home scene
            SceneManager.LoadScene(GameManager.HOME_SCENE);
        }
    }
}
