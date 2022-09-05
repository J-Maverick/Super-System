
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AvatarToggle : UdonSharpBehaviour
{
    public GameObject targetAvatar;
    public AvatarArray avatarArray;

    public void ToggleAvatar()
    {
        avatarArray.ToggleAvatar(targetAvatar);
    }
}
