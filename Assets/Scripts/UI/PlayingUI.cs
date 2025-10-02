using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayingUI : MonoBehaviour
{
    private void Awake()
    {
        
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
