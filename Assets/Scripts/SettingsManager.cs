using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [Header("Draw Data")]
    public FrameRates FPS = FrameRates._120;

    [Space(20)]
    public VSyncs vSync = VSyncs.Off;

    private void Awake()
    {
        ApplySettings();
    }

    public void ApplySettings()
    {
        QualitySettings.vSyncCount = (int)vSync;

        Application.targetFrameRate = (int)FPS;
    }

    
    public enum FrameRates
    {
         _30 = 30,
         _60 = 60,
         _90 = 90,
         _120 = 120,
         _180 = 180,
         _Unlimited = 300
    }
    public enum VSyncs
    {
        Off = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }
}
