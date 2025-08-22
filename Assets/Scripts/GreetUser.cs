using TMPro;
using UnityEngine;
using System;

public class GreetUser : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text greetText;

    void Start()
    {
        string userName = PlayerPrefs.GetString("UserName", "Guest");
        nameText.text = userName;

        int hour = DateTime.Now.Hour;
        string greeting;

        if (hour >= 5 && hour < 12)
            greeting = "Selamat Pagi";
        else if (hour >= 12 && hour < 15)
            greeting = "Selamat Siang";
        else if (hour >= 15 && hour < 18)
            greeting = "Selamat Sore";
        else
            greeting = "Selamat Malam";

        greetText.text = greeting;
    }
}