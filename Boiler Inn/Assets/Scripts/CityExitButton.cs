using UnityEngine;
using UnityEngine.UI;

public class CityExitButton : MonoBehaviour
{
    private void Start()
    {
        // Pega o botão onde este script está colado
        Button exitButton = GetComponent<Button>();

        // Adiciona a função invisível do clique
        exitButton.onClick.AddListener(() => 
        {
            if (DayManager.instance != null)
            {
                DayManager.instance.EndCityExploration();
            }
            else
            {
                Debug.LogWarning("DayManager missing in this scene!");
            }
        });
    }
}