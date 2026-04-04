using UnityEngine;

public class PortaOficina : MonoBehaviour, IInteractable
{
    public void Interagir()
    {
        // Acesso sempre travado na primeira cena
        Debug.Log("<color=red>[PORTA]</color> Travada.");
        DialogueManager.Instance.ExibirMensagemRapida("A porta está trancada... Preciso resolver minhas pendências com o Tom antes.");
    }
}