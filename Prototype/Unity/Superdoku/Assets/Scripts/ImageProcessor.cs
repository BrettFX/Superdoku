using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    /**
     * A Canvas extension to be applied to RawImage components to ensure that the
     * texture associated with the respective RawImage is scaled appropriately to conform
     * to the size of its parent.
     */
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
    public class ImageProcessor : MonoBehaviour
    {
        public GameObject btnBack;
        public GameObject scanner;

        void Start()
        {
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

        private void ToggleAnimation(bool b)
        {
            // Handle scanner animation
            scanner.SetActive(b);
            //ParticleSystem scannerParticles = scanner.GetComponentInChildren<ParticleSystem>();
            //scannerParticles.Pause(!b); // Inverse logic
            Animator scannerAnimator = scanner.GetComponent<Animator>();
            scannerAnimator.SetTrigger(b ? "ScanStart" : "ScanFinish");
        }

        public void OnCancel()
        {
            // Change to home scene (scene management cancels the active rest request
            SceneManager.LoadScene(GameManager.HOME_SCENE);
        }
    }
}
