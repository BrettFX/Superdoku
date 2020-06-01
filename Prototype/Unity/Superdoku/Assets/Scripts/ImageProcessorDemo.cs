using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Superdoku
{
    public class ImageProcessorDemo : MonoBehaviour
    {
        public Texture2D testDigitTexture;

        // Start is called before the first frame update
        void Start()
        {
            // Make a copy of the input texture so it doesn't get overwritten during testing
            //Texture2D photo = new Texture2D(testDigitTexture.width, testDigitTexture.height);
            //photo.SetPixels(testDigitTexture.GetPixels());
            //photo.Apply();

            //NormalizeTexture(photo);
        }

        // Update is called once per frame
        void Update()
        {

        }

        private void NormalizeTexture(Texture2D texture)
        {
            /*
                # load the image
                img = load_img(filename, color_mode="grayscale", target_size=(28, 28))
                # convert to array
                img = img_to_array(img)
                # reshape into a single sample with 1 channel
                img = img.reshape(1, 28, 28, 1)
                # prepare pixel data
                img = img.astype('float32')
                img = img / 255.0
             */

            // Resize to target 28x28 pixels for MNIST dataset
            TextureScale.Bilinear(texture, 28, 28);

            // Convert to grayscale (e.g., single channel 8 bits)
            GameManager.ConvertToGrayscale(texture);

            // Convert texture to array

        }
    }
}


