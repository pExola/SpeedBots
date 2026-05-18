using System.Collections.Generic; // Necessário para usarmos Dictionary
using UnityEngine;

public class BauDePecas : MonoBehaviour, IInteractable
{
    [Header("O que tem dentro? (Pode colocar várias!)")]
    public PecaSpeedBot[] pecasEscondidas;

    private bool jaAberto = false;

    public void Interagir()
    {
        if (jaAberto)
        {
            Debug.Log("[BAÚ] Este baú já está vazio.");
            return;
        }

        int itensPegos = 0;

        // Dicionário para agrupar as peças antes de mandar para a tela
        // Ele vai guardar o "Nome do Item" e a "Quantidade"
        Dictionary<string, int> itensAgrupados = new Dictionary<string, int>();

        foreach (PecaSpeedBot peca in pecasEscondidas)
        {
            if (peca != null)
            {
                InventarioManager.Instance.AdicionarPeca(peca);
                itensPegos++;

                // Pega o nome do item. Se o seu PecaSpeedBot tiver uma variável específica (ex: peca.nomeDaPeca), mude aqui!
                string nome = peca.name;

                // Se o item já está no dicionário, soma +1. Se não, cria o registro valendo 1.
                if (itensAgrupados.ContainsKey(nome))
                    itensAgrupados[nome]++;
                else
                    itensAgrupados[nome] = 1;
            }
        }

        // Agora sim, enviamos a lista agrupada e limpa para a UI Lateral
        foreach (var grupo in itensAgrupados)
        {
            if (NotificacaoLateral.Instance != null)
            {
                // grupo.Key é o Nome, grupo.Value é a Quantidade
                NotificacaoLateral.Instance.MostrarLoot(grupo.Key, grupo.Value);
            }
        }

        jaAberto = true;
        Debug.Log($"[BAÚ] Baú aberto! Você pegou {itensPegos} itens.");
    }
}