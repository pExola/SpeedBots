using UnityEngine; // Importa as ferramentas principais da engine da Unity.
using UnityEngine.SceneManagement;

// Cria a classe da porta e assina o contrato da interface IInteractable.
// Isso a torna o seu "segurança" narrativo, garantindo que ela reaja ao clique/interação do jogador.
public class PortaAutrons : MonoBehaviour, IInteractable
{
    // Método obrigatório do IInteractable, executado no exato momento em que o jogador encosta na porta e aperta o botão de ação.
    public void Interagir()
    {
        SceneManager.LoadScene("Escritorio");
    }
}