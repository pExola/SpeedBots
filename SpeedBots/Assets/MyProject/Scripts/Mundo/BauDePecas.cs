using UnityEngine;

public class BauDePecas : MonoBehaviour, IInteractable
{
    [Header("O que tem dentro? (Pode colocar várias!)")]
    // O segredo está nestes colchetes []. Eles transformam o slot num array!
    public PecaSpeedBot[] pecasEscondidas;

    private bool jaAberto = false;

    public void Interagir()
    {
        if (jaAberto)
        {
            Debug.Log("[BAÚ] Este baú já está vazio.");
            return;
        }

        // Passa por cada peça da lista e coloca na mochila
        int itensPegos = 0;
        foreach (PecaSpeedBot peca in pecasEscondidas)
        {
            if (peca != null)
            {
                InventarioManager.Instance.AdicionarPeca(peca);
                itensPegos++;
            }
        }

        jaAberto = true;
        Debug.Log($"[BAÚ] Baú aberto! Você pegou {itensPegos} itens.");
    }
}
