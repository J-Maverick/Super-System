
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AvatarArray : UdonSharpBehaviour
{
    public GameObject[] avatars = null;

    public void ToggleAvatar(GameObject gameObject)
    {
        foreach (GameObject avatar in avatars)
        {
            if (avatar == gameObject)
            {
                avatar.SetActive(!avatar.activeSelf);
            }
            else avatar.SetActive(false);
        }
    }
}
