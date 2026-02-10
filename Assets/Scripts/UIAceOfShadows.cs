using UnityEngine;
using TMPro;

public class UIAceOfShadows : UIBase
{
    [SerializeField] private TextMeshProUGUI textButtonSpeed;
    [SerializeField] private TextMeshProUGUI textButtonPlay;
    [SerializeField] private TextMeshProUGUI textLeftCount;
    [SerializeField] private TextMeshProUGUI textRightCount;

    [SerializeField] private CardTable cardTable;

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
        cardTable.OnCountChanged += OnCountChanged;
    }

    private void OnDisable()
    {
        cardTable.IsPlayingChanged -= IsPlayingChanged;
        cardTable.OnSpeedChanged -= OnSpeedChanged;
        cardTable.OnCountChanged -= OnCountChanged;
    }

    private void IsPlayingChanged(bool isPlaying)
    {
        textButtonPlay.text = isPlaying ? "Pause" : "Play";
    }

    private void OnSpeedChanged(int speed)
    {
        textButtonSpeed.text = $"Speed x{cardTable.CurrentSpeed}";
    }

    private void OnCountChanged(int left, int right)
    {
        textLeftCount.text = left.ToString();
        textRightCount.text = right.ToString();
    }
}
