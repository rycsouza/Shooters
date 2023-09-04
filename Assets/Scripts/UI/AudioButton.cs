using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class AudioButton : MonoBehaviour
{
    [SerializeField] private List<Sprite> _imageList = new List<Sprite>();

    public void AudioController()
    {
        if (AudioListener.pause)
        {
            AudioListener.pause = false;
            gameObject.GetComponent<Image>().sprite = _imageList[0];
        }
        else
        {
            AudioListener.pause = true;
            gameObject.GetComponent<Image>().sprite = _imageList[1];
        }
    }
}
