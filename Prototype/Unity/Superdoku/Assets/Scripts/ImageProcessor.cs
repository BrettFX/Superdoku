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
        WebCamTexture webCamTexture;

        // Start is called before the first frame update
        void Start()
        {
            // Load image path from player prefs
            imagePath = PlayerPrefs.GetString(GameManager.IMAGE_PATH_KEY);
            Texture2D pngImage = GameManager.LoadPNG(imagePath);
            if (pngImage != null)
            {
                // TODO get texture and manipulate it the same way that the webcam photo was manipulated

                // Create the webcam texture just for the resources to detect if the image should be manipulated
                // for displaying properly (don't actually play the webcam)
                //webCamTexture = new WebCamTexture();
                //Texture2D rawTexture = GameManager.LoadPNG(imagePath);
                //while (!NormalizeTexture(rawTexture)) ;
                //NormalizeTexture(rawTexture);

                GetComponent<RawImage>().texture = pngImage;
            }
            else
            {
                if (GameManager.DEBUG_MODE) { Debug.LogWarning("Could not load image to process. Navigating back to home"); }
                SceneManager.LoadScene(GameManager.HOME_SCENE);
            }
        }

        private bool NormalizeTexture(Texture2D sourceTexture)
        {
            
            // @see https://answers.unity.com/questions/773464/webcamtexture-correct-resolution-and-ratio.html
            RawImage rawImage = GetComponent<RawImage>();

            //if (webCamTexture.width < 100)
            //{
            //    if (GameManager.DEBUG_MODE) { Debug.Log("Waiting for correct webcam texture info..."); }
            //    return false;
            //}

            // change as user rotates iPhone or Android:

            int cwNeeded = webCamTexture.videoRotationAngle;
            // Unity helpfully returns the _clockwise_ twist needed
            // guess nobody at Unity noticed their product works in counterclockwise:
            int ccwNeeded = -cwNeeded;

            // IF the image needs to be mirrored, it seems that it
            // ALSO needs to be spun. Strange: but true.
            if (webCamTexture.videoVerticallyMirrored) ccwNeeded += 180;

            // you'll be using a UI RawImage, so simply spin the RectTransform
            RectTransform rawImageRT = rawImage.GetComponent<RectTransform>();
            rawImageRT.localEulerAngles = new Vector3(0f, 0f, ccwNeeded);

            float videoRatio = (float)webCamTexture.width / (float)webCamTexture.height;

            // you'll be using an AspectRatioFitter on the Image, so simply set it
            AspectRatioFitter rawImageARF = rawImage.GetComponent<AspectRatioFitter>();
            rawImageARF.aspectRatio = videoRatio;

            // alert, the ONLY way to mirror a RAW image, is, the uvRect.
            // changing the scale is completely broken.
            if (webCamTexture.videoVerticallyMirrored)
                rawImage.uvRect = new Rect(1, 0, -1, 1);  // means flip on vertical axis
            else
                rawImage.uvRect = new Rect(0, 0, 1, 1);  // means no flip

            // Now the raw image texture can be set to the source texture
            rawImage.texture = sourceTexture;
            return true;
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
