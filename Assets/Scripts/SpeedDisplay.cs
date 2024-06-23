using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SpeedDisplay : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TMP_Text speedDisplay;

    void Update()
    {
        speedDisplay.text = ((int)(playerController.GetFlatVelocity() * 100)/100f).ToString();
    }
}
