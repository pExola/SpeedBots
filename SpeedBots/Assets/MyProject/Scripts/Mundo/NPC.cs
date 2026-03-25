using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("Conversa")]
    // Isso vai criar uma lista organizadinha no Inspector da Unity!
    public FalaDialogo[] dialogo;

    public void Interagir()
    {
        DialogueManager.Instance.IniciarDialogo(dialogo);
    }
}
