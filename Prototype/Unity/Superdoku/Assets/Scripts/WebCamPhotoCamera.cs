using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Environment;

namespace Superdoku
{
    public class WebCamPhotoCamera : MonoBehaviour
    {
        public static SpecialFolder BASE_OUT_DIR = SpecialFolder.LocalApplicationData;
        public string OUTPUT_DIR = GetFolderPath(BASE_OUT_DIR) + "/Superdoku";
        WebCamTexture webCamTexture;
        Quaternion baseRotation;

        void Start()
        {
            // Get the name of the front-facing camera
            //string frontCamName = null;
            //WebCamDevice[] webCamDevices = WebCamTexture.devices;
            //foreach (WebCamDevice camDevice in webCamDevices)
            //{
            //    if (camDevice.isFrontFacing)
            //    {
            //        frontCamName = camDevice.name;
            //        break;
            //    }
            //}

            //if (GameManager.DEBUG_MODE) { Debug.Log("Front Facing Camera: " + frontCamName); }

            //webCamTexture = new WebCamTexture(frontCamName);

            webCamTexture = new WebCamTexture();
            baseRotation = transform.rotation;
            webCamTexture.Play();
        }

        
        private void Update()
        {
            // @see https://answers.unity.com/questions/773464/webcamtexture-correct-resolution-and-ratio.html
            RawImage rawImage = GetComponent<RawImage>();

            if (webCamTexture.width < 100)
            {
                if (GameManager.DEBUG_MODE) { Debug.Log("Still waiting another frame for correct info..."); }
                return;
            }

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

            // Finally, set the raw image texture to the modified webCamTexture to render each frame
            rawImage.texture = webCamTexture;
        }

        public void OnSnap()
        {
            // Call coroutine to take a photo
            StartCoroutine("TakePhoto");
        }

        public void OnBack()
        {
            // Important so that the webcam doesn't keep running until Unity is restarted
            webCamTexture.Stop();

            // Change to home scene
            SceneManager.LoadScene(GameManager.HOME_SCENE);
        }

        /**
         * Capture the current texture frame and convert it to a png file to be
         * written to the file system.
         * 
         * @see https://stackoverflow.com/questions/24496438/can-i-take-a-photo-in-unity-using-the-devices-camera
         * @see http://docs.unity3d.com/ScriptReference/WaitForEndOfFrame.html
         */
        public IEnumerator TakePhoto()  // Start this Coroutine on some button click
        {
            // Required
            yield return new WaitForEndOfFrame();

            Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
            photo.SetPixels(webCamTexture.GetPixels());
            photo.Apply();

            // Encode to a PNG
            byte[] bytes = photo.EncodeToPNG();

            // Write out the PNG.
            string outFileName = "/superdoku_snap.png";
            if (GameManager.WriteFile(OUTPUT_DIR, outFileName, bytes, FileMode.Create))
            {
                // Save output file to player prefs so it can be referenced in image processor scene
                PlayerPrefs.SetString(GameManager.IMAGE_PATH_KEY, OUTPUT_DIR + outFileName);

                if (GameManager.DEBUG_MODE)
                {
                    Debug.Log("Successfully wrote to " + OUTPUT_DIR + outFileName);
                }

                // Important so that the webcam doesn't keep running until Unity is restarted
                webCamTexture.Stop();

                // Navigate to the Image Processor scene
                SceneManager.LoadScene(GameManager.IMAGE_PROCESSOR_SCENE);
            }
        }
    }
}
