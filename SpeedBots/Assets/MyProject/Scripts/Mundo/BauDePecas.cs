using UnityEngine;

public class BauDePecas : MonoBehaviour, IInteractable
{
    [Header("O que tem dentro?")]
    public PecaSpeedBot pecaEscondida; // Arraste a ficha da peça aqui no Inspector

    private bool jaAberto = false;

    public void Interagir()
    {
        if (jaAberto)
        {
            Debug.Log("[BAÚ] Este baú já está vazio.");
            // Opcional: Aqui você pode chamar o DialogueManager para dizer "Apenas poeira aqui."
            return;
        }

        if (pecaEscondida != null)
        {
            InventarioManager.Instance.AdicionarPeca(pecaEscondida);
            jaAberto = true;
            // Opcional: Mudar o sprite do baú para um baú aberto aqui
        }
    }
}
