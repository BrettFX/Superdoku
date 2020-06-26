﻿using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Environment;

// Create task extension to use async Task in Coroutine
// @see http://www.stevevermeulen.com/index.php/2017/09/using-async-await-in-unity3d-2017/
public static class TaskExtensions
{
    public static IEnumerator AsIEnumerator(this Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }

        if (task.IsFaulted)
        {
            throw task.Exception;
        }
    }
}

public static class CanvasExtensions
{
    public static Vector2 SizeToParent(this RawImage image, float padding = 0)
    {
        var parent = image.transform.parent.GetComponentInParent<RectTransform>();
        var imageTransform = image.GetComponent<RectTransform>();
        if (!parent) { return imageTransform.sizeDelta; } //if we don't have a parent, just return our current width;
        padding = 1 - padding;
        float w = 0, h = 0;
        float ratio = image.texture.width / (float)image.texture.height;
        var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);
        if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90)
        {
            //Invert the bounds if the image is rotated
            bounds.size = new Vector2(bounds.height, bounds.width);
        }
        //Size by height first
        h = bounds.height * padding;
        w = h * ratio;
        if (w > bounds.width * padding)
        { //If it doesn't fit, fallback to width;
            w = bounds.width * padding;
            h = w / ratio;
        }
        imageTransform.sizeDelta = new Vector2(w, h);
        return imageTransform.sizeDelta;
    }
}

namespace Superdoku
{
    public class WebCamPhotoCamera : MonoBehaviour
    {
        public GameObject btnBack;
        public GameObject btnSnap;
        public GameObject scanner;
        
        //public static SpecialFolder BASE_OUT_DIR = SpecialFolder.LocalApplicationData;
        //public string OUTPUT_DIR = BASE_OUT_DIR + "/Superdoku";
        private WebCamTexture webCamTexture;

        private bool m_snapped = false;

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
            //webCamTexture.Play();

            // TEST
            //GetComponent<RawImage>().SizeToParent();

            // Load image path from player prefs
            string imagePath = PlayerPrefs.GetString(GameManager.IMAGE_PATH_KEY);

            if (imagePath != null && imagePath != "")
            {
                PlayerPrefs.DeleteKey(GameManager.IMAGE_PATH_KEY);
                byte[] imageData = GameManager.GetImageData(imagePath);

                // Preprocesses the image and build REST request object
                RequestContent requestContent = GameManager.ProcessAndBuildRequest(imageData);

                // Create texture object based on new preprocessed image data and display the texture to the RawImage
                Texture2D texture = new Texture2D(2, 2, TextureFormat.BGRA32, false);
                texture.LoadImage(requestContent.data);
                RawImage rawImage = GetComponent<RawImage>();
                rawImage.texture = texture;
                rawImage.SizeToParent();

                // Start the scanning animation once the scene starts
                ToggleAnimation(true);

                // Send off to Superdoku api for image recogonition
                GameManager.SendTexture(requestContent);
            }
            else
            {
                if (GameManager.DEBUG_MODE) { Debug.LogWarning("Could not load image to process. Navigating back to home"); }
                SceneManager.LoadScene(GameManager.HOME_SCENE);
            }
        }
        
        private void Update()
        {
            // UpdateWebcamTexture();
        }

        private void UpdateWebcamTexture()
        {
            // Don't proceed if the cam is in an image snapped state
            if (m_snapped)
            {
                return;
            }

            // Don't proceed if the webcam texture hasn't finished getting metadata
            if (webCamTexture.width < 100)
            {
                if (GameManager.DEBUG_MODE) { Debug.Log("Waiting for correct webcam texture info..."); }
                return;
            }

            // @see https://answers.unity.com/questions/773464/webcamtexture-correct-resolution-and-ratio.html
            RawImage rawImage = GetComponent<RawImage>();

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
            m_snapped = true;
            ToggleAnimation(m_snapped);
            if (webCamTexture.isPlaying)
            {
                webCamTexture.Pause();
            }

            // Call coroutine to process image
            StartCoroutine(ProcessImageCoroutine());

            // Call coroutine to take a photo
            //StartCoroutine("TakePhoto");
        }

        private void ToggleAnimation(bool b)
        {
            // Handle back button animation
            Animator btnBackAnimator = btnBack.GetComponent<Animator>();
            btnBackAnimator.SetTrigger(b ? "ScanStart" : "ScanFinish");

            // Handle snap button animation
            Animator btnSnapAnimator = btnSnap.GetComponent<Animator>();
            btnSnapAnimator.SetTrigger(b ? "ScanStart" : "ScanFinish");

            // Handle scanner animation
            scanner.SetActive(b);
            //ParticleSystem scannerParticles = scanner.GetComponentInChildren<ParticleSystem>();
            //scannerParticles.Pause(!b); // Inverse logic
            Animator scannerAnimator = scanner.GetComponent<Animator>();
            scannerAnimator.SetTrigger(b ? "ScanStart" : "ScanFinish");
        }

        public void OnBack()
        {
            // Important so that the webcam doesn't keep running until Unity is restarted
            if (webCamTexture.isPlaying)
            {
                webCamTexture.Stop();
            }

            // Change to home scene
            SceneManager.LoadScene(GameManager.HOME_SCENE);
        }

        private async Task ProcessImageAsync(WebCamTexture wct)
        {
            //await Task.Delay(5000);
            await Task.Run(() =>
            {
                // Get the texture2d representation from the webcam snap
                Texture2D photo = new Texture2D(wct.width, wct.height);
                photo.SetPixels(wct.GetPixels());
                photo.Apply();

                // Encode webcam texture to png to get the byte array data
                byte[] data = photo.EncodeToPNG();

                // Invoke RestRequest PUT request to recognize snapped image of Sudoku puzzle
                RequestContent content = new RequestContent(data, "png");
                RestRequest.Instance.SendRequest(string.Format(RestRequest.BASE_URL, "recognize"), "PUT", content);
            });
        }
        
        public IEnumerator ProcessImageCoroutine()
        {
            // Keep track of processing time
            Debug.Log("Began processing image at timestamp: " + Time.time);

            //yield return ProcessImageAsync(webCamTexture).AsIEnumerator();

            //yield return new WaitForEndOfImageProcessing(webCamTexture);

            yield return new WaitForEndOfFrame();

            // yield on a new YieldInstruction that waits for n seconds (Debugging).
            //yield return new WaitForSeconds(5);

            // Get the texture2d representation from the webcam snap
            Texture2D photo = new Texture2D(webCamTexture.width, webCamTexture.height);
            photo.SetPixels(webCamTexture.GetPixels());
            photo.Apply();

            // Encode webcam texture to png to get the byte array data
            byte[] data = photo.EncodeToPNG();

            // Invoke RestRequest PUT request to recognize snapped image of Sudoku puzzle
            RequestContent content = new RequestContent(data, "png");
            RestRequest.Instance.SendRequest(string.Format(RestRequest.BASE_URL, "recognize"), "PUT", content);

            // Return back to original state to test animations
            //m_snapped = false;
            //ToggleAnimation(m_snapped);

            //// Play the webcam again
            //if (!webCamTexture.isPlaying)
            //{
            //    webCamTexture.Play();
            //}

            // After we have waited 5 seconds print the time again.
            Debug.Log("Finished processing at timestamp: " + Time.time);
        }

        private void OnDestroy()
        {
            // Stop the webcam so it can be used again after recycling the scene associated with this instance
            webCamTexture.Stop();
        }
    }
}
