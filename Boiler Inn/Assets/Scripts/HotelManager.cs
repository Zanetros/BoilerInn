using System.Collections.Generic;
using UnityEngine;

public class HotelManager : MonoBehaviour
{
    public static HotelManager instance;

    [Header("Hotel Settings")]
    public int maxSlots = 5; // Número máximo de hóspedes que o hotel suporta
    
    [Header("Current Guests")]
    public List<string> guestList = new List<string>(); // Lista de IDs dos hóspedes atuais

    private void Awake()
    {
        // Padrão Singleton para acesso global e fácil
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // Verifica se ainda há quartos disponíveis
    public bool HasAvailableRoom()
    {
        return guestList.Count < maxSlots;
    }

    // Tenta adicionar um hóspede. Retorna true se houver sucesso.
    public bool AddGuest(string guestID)
    {
        if (HasAvailableRoom())
        {
            guestList.Add(guestID);
            Debug.Log($"Guest '{guestID}' checked in. Occupancy: {guestList.Count}/{maxSlots}");
            return true;
        }
        
        Debug.LogWarning("Cannot add guest. The hotel is full!");
        return false;
    }

    // Remove um hóspede (útil para quando eles fizerem check-out no futuro)
    public void RemoveGuest(string guestID)
    {
        if (guestList.Contains(guestID))
        {
            guestList.Remove(guestID);
            Debug.Log($"Guest '{guestID}' checked out.");
        }
    }
}