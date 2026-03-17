using UnityEngine;
using TMPro;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager instance;

    [Header("Saldos Atuais")]
    public int cybercurrency;
    public int implants;
    public int chips;

    //[Header("Referências de UI (Opcional)")]
    //public TextMeshProUGUI cyberText;
    //public TextMeshProUGUI implantsText;
    //public TextMeshProUGUI chipsText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // A "Mágica" acontece aqui
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        UpdateUI();
    }

    // Métodos para Adicionar Moedas
    public void AddCybercurrency(int amount) { cybercurrency += amount; UpdateUI(); }
    public void AddImplants(int amount) { implants += amount; UpdateUI(); }
    public void AddChips(int amount) { chips += amount; UpdateUI(); }

    // Métodos para Remover Moedas (Gastar)
    public bool SpendCybercurrency(int amount)
    {
        if (cybercurrency >= amount) {
            cybercurrency -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    private void UpdateUI()
    {
        //if (cyberText != null) cyberText.text = cybercurrency.ToString();
        //if (implantsText != null) implantsText.text = implants.ToString();
        //if (chipsText != null) chipsText.text = chips.ToString();
    }
}