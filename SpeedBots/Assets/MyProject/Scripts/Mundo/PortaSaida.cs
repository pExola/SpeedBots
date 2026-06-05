using UnityEngine;
using UnityEngine.SceneManagement;

public class PortaSaida : MonoBehaviour, IInteractable
{
    // Método obrigatório do IInteractable, executado no exato momento em que o jogador encosta na porta e aperta o botão de ação.
    public void Interagir()
    {
        SceneManager.LoadScene("Caminho_Autrons");
    }
}
