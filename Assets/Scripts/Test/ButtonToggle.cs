using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class ButtonToggle : UdonSharpBehaviour
{
    private Toggle _toggle;
    [UdonSynced] private bool toggleState;
    public GameObject[] targetObjectArray = null;

    void Start()
    {
        _toggle = transform.GetComponent<Toggle>();
        toggleState = _toggle.isOn;
    }

    public override void OnPreSerialization()
    {
        toggleState = _toggle.isOn;
    }

    public override void OnDeserialization()
    {
        _toggle.isOn = toggleState;
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        RequestSerialization();
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player)
    {
        if (player == Networking.LocalPlayer)
        {
            RequestSerialization();
        }
    }

    public override void Interact()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    public void ObjectToggle()
    {
        foreach (GameObject toggleObject in targetObjectArray)
        {
            toggleObject.SetActive(!toggleObject.activeSelf);
        }

        RequestSerialization();
    }
}