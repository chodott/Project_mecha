using UnityEngine;
using UnityEngine.UI;

public class GameResultUI : MonoBehaviour
{
    [SerializeField] private GameObject _winImage;
    [SerializeField] private GameObject _loseImage;


    private void Start()
    {
        EventBus.OnWin += ShowWinUI;
        EventBus.OnLose += ShowLoseUI;
    }


    private void ShowWinUI()
    {
        _winImage.SetActive(true);
        HideLoseUI();
    }

    private void HideWinUI()
    {
        _winImage.SetActive(false);
    }
    private void ShowLoseUI()
    {
        _loseImage.SetActive(true);
        HideWinUI();
    }

    private void HideLoseUI()
    {
        _loseImage.SetActive(false);
    }


}
