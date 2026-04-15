using UnityEngine; // Importa as ferramentas principais da engine da Unity.

// Cria a classe da porta e assina o contrato da interface IInteractable.
// Isso a torna o seu "segurança" narrativo, garantindo que ela reaja ao clique/interação do jogador.
public class PortaOficina : MonoBehaviour, IInteractable
{
    // Método obrigatório do IInteractable, executado no exato momento em que o jogador encosta na porta e aperta o botão de ação.
    public void Interagir()
    {
        // Prática para testes: Dispara um aviso no console do desenvolvedor alertando que a interação ocorreu.
        // Usa formatação de texto rico (Rich Text) para pintar a tag "[PORTA]" de vermelho, facilitando a visualização no meio de outros logs.
        Debug.Log("<color=red>[PORTA]</color> Travada.");

        // A parede invisível elegante: Em vez de usar o SceneManager para carregar a próxima fase, a porta barra o jogador.
        // Ela aciona o Singleton de diálogo (DialogueManager.Instance) e usa a função de mensagem rápida para jogar um aviso na tela, 
        // explicando o motivo da tranca e guiando o jogador a completar o objetivo atual da história.
        DialogueManager.Instance.ExibirMensagemRapida("A porta está trancada... Preciso resolver minhas pendências com o Tom antes.");
    }
}