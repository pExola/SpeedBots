using UnityEngine;
using UnityEngine.SceneManagement;

public class PortaOficina : MonoBehaviour, IInteractable
{
    [Header("Configuração")]
    [Tooltip("O nome exato da cena do interior da oficina")]
    public string nomeCenaInterior = "Oficina_Selecao";

    public void Interagir()
    {
        Debug.Log($"<color=cyan>[SISTEMA]</color> Sam abriu a porta. Carregando {nomeCenaInterior}...");

        // Transporta o jogador para a cena da Oficina Interna
        SceneManager.LoadScene(nomeCenaInterior);
    }
}
