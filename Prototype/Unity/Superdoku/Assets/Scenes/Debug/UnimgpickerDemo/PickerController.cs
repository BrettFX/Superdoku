using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Superdoku;

namespace Kakera
{
    public class PickerController : MonoBehaviour
    {
        [SerializeField]
        private Unimgpicker imagePicker;

        [SerializeField]
        private MeshRenderer imageRenderer;

        [SerializeField]
        private Dropdown sizeDropdown;

        private int[] sizes = {1024, 256, 16};

        void Awake()
        {
            // Add the Completed handle function for when the user has selected an image
            imagePicker.Completed += (string path) =>
            {
                StartCoroutine(LoadImage(path, imageRenderer));
            };
        }

        public void OnPressShowPicker()
        {
            imagePicker.Show("Select Image", "unimgpicker", sizes[sizeDropdown.value]);
        }

        private IEnumerator LoadImage(string path, MeshRenderer output)
        {
            var url = "file://" + path;
            var www = new WWW(url);
            yield return www;

            var texture = www.texture;
            if (texture == null)
            {
                Debug.LogError("Failed to load texture url:" + url);
            }

            output.material.mainTexture = texture;

            // Get texture data
            byte[] data = texture.EncodeToPNG();

            // Send the file data to the superdoku api to recognize and classifiy its digits
            RequestContent content = new RequestContent(data, "png");
            RestRequest.Instance.SendRequest(string.Format(RestRequest.BASE_URL, "recognize"), "PUT", content);
        }
    }
}