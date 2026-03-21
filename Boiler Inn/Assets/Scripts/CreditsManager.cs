using UnityEngine;
using TMPro;

public class CreditsManager : MonoBehaviour
{
    public static CreditsManager instance;

    [Header("UI References")]
    public GameObject creditsPanel; // O painel preto do fundo
    public RectTransform creditsRectTransform; // O componente que move o texto
    public TextMeshProUGUI creditsText; // O texto em si

    [Header("Scroll Settings")]
    public float scrollSpeed = 50f;
    public bool scrollDown = false; // Marque na Unity se quiser que o texto vá para baixo em vez de subir

    private bool isScrolling = false; 

    private void Awake()
    {
        // Configura o Singleton
        if (instance == null) instance = this;
        else Destroy(gameObject);
        
        // Garante que os créditos comecem parados e invisíveis
        isScrolling = false; 
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    private void Update()
    {
        // Se não estiver na hora dos créditos, ignora o Update
        if (!isScrolling) return;

        // A MÁGICA: Move o texto constantemente na direção escolhida
        Vector2 direction = scrollDown ? Vector2.down : Vector2.up;
        creditsRectTransform.anchoredPosition += direction * scrollSpeed * Time.deltaTime;
    }

    // =========================================================
    // FUNÇÃO CHAMADA PELO DIALOGUE MANAGER (Via CreditsNode)
    // =========================================================
    public void StartCredits()
    {
        // 1. Liga a tela de créditos e libera a animação
        if (creditsPanel != null) creditsPanel.SetActive(true);
        isScrolling = true;
        
        // 2. Procura a palavra {DIAS} e troca pelo número real de dias jogados!
        if (DayManager.instance != null && creditsText != null)
        {
            int totalDays = DayManager.instance.currentDay;
            creditsText.text = creditsText.text.Replace("{DIAS}", totalDays.ToString());
        }
    }
}