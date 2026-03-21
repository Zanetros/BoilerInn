using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager instance;

    [Header("Player Wallet")]
    public int cybercurrency = 0;
    public int implants = 0;
    public int chips = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public void AddCybercurrency(int amount)
    {
        cybercurrency += amount;
        UpdateUI();
    }

    public void AddImplants(int amount)
    {
        implants += amount;
        UpdateUI();
    }

    public void AddChips(int amount)
    {
        chips += amount;
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        // Avisa o UIManager da cena atual para atualizar os números na tela
        if (UIManager.instance != null)
        {
            UIManager.instance.UpdateCurrencyTexts();
        }
    }
}