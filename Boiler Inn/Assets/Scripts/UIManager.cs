using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header("Currency UI Texts")]
    public TextMeshProUGUI cyberText;
    public TextMeshProUGUI implantsText;
    public TextMeshProUGUI chipsText;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Assim que a cena carrega, ele puxa os valores do CurrencyManager imortal
        UpdateCurrencyTexts();
    }

    public void UpdateCurrencyTexts()
    {
        // Pega os valores reais e joga nos textos da tela
        if (CurrencyManager.instance != null)
        {
            if (cyberText != null) cyberText.text = CurrencyManager.instance.cybercurrency.ToString();
            if (implantsText != null) implantsText.text = CurrencyManager.instance.implants.ToString();
            if (chipsText != null) chipsText.text = CurrencyManager.instance.chips.ToString();
        }
    }
}