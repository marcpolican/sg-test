using UnityEngine;
using TMPro;

public class UIAceOfShadows : UIBase
{
    [SerializeField] private TextMeshProUGUI textButtonSpeed;
    [SerializeField] private TextMeshProUGUI textButtonPlay;

    [SerializeField] private CardTable cardTable;

    public void OnButtonBack()
    {
        cardTable.CleanUpAndExit();
        OnButtonSceneChange("0-Menu");
    }

    public void OnButtonReset()
    {
        cardTable.Initialize();
    }

    public void OnButtonPlay()
    {
        cardTable.TogglePlay();
    }

    public void OnButtonSpeed()
    {
        cardTable.ToggleSpeed();
    }

    private void OnEnable()
    {
        cardTable.IsPlayingChanged += IsPlayingChanged;
        cardTable.OnSpeedChanged += OnSpeedChanged;
    }

    private void OnDisable()
    {
        cardTable.IsPlayingChanged -= IsPlayingChanged;
        cardTable.OnSpeedChanged -= OnSpeedChanged;
    }

    private void IsPlayingChanged(bool isPlaying)
    {
        textButtonPlay.text = isPlaying ? "Pause" : "Play";
    }

    private void OnSpeedChanged(int speed)
    {
        textButtonSpeed.text = $"Speed x{cardTable.CurrentSpeed}";
    }
}
