using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

namespace Superdoku
{
    public class PermissionsManager : MonoBehaviour
    {
        GameObject dialog = null;

        // Start is called before the first frame update
        void Start()
        {
            #if PLATFORM_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Permission.RequestUserPermission(Permission.Camera);
                dialog = new GameObject();
            }
            #endif
        }

        void OnGUI()
        {
            #if PLATFORM_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                // The user denied permission to use the Camera.
                // Display a message explaining why you need it with Yes/No buttons.
                // If the user says yes then present the request again
                // Display a dialog here.
                //dialog.AddComponent<PermissionsRationaleDialog>();
                Application.Quit();
                return;
            }
            else if (dialog != null)
            {
                Destroy(dialog);
            }
            #endif

            // Now you can do things with the microphone
        }
    }
}
