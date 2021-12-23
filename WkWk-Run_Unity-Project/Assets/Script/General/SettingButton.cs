using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingButton : MonoBehaviour
{
    private float openPos = 0;
    private float closePos = 300;
    private float slideSpeed = .7f;

    private bool opening;
    private bool closing;
    private bool isOpen;

    [SerializeField] RectTransform slideObject;

    public void SettingButtonMethod()
    {
        FindObjectOfType<AudioManager>().CheckUI();

        if (isOpen)
        {
            closing = true;
            isOpen = false;
        }
        else
        {
            opening = true;
            isOpen = true;
        }
    }

    public void Update()
    {
        if (opening == true)
        {
            slideObject.localPosition = new Vector3(slideObject.localPosition.x, Mathf.Lerp(slideObject.localPosition.y, openPos, slideSpeed), slideObject.localPosition.z);
            if (slideObject.localPosition.y - 15 <= openPos)
            {
                slideObject.localPosition = new Vector3(slideObject.localPosition.x, openPos, slideObject.localPosition.z);
                opening = false;
            }
        }
        else if (closing == true)
        {
            slideObject.localPosition = new Vector3(slideObject.localPosition.x, Mathf.Lerp(slideObject.localPosition.y, closePos, slideSpeed), slideObject.localPosition.z);
            if (slideObject.localPosition.y + 15 >= closePos)
            {
                slideObject.localPosition = new Vector3(slideObject.localPosition.x, closePos, slideObject.localPosition.z);
                closing = false;
            }
        }
    }

    public void TurnOffSound()
    {
        FindObjectOfType<AudioManager>().TurnOffSound();
    }
    public void TurnOnSound(int scene)
    {
        FindObjectOfType<AudioManager>().TurnOnSound(scene);
    }
}
