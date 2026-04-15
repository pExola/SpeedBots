using UnityEngine; // Importa as ferramentas principais da engine da Unity para o script funcionar.

public class BancadaDeMontagem : MonoBehaviour, IInteractable // Cria a classe que atua como a âncora física (mesa/oficina) no mundo 2D e implementa a interface IInteractable (avisando o sistema do jogador que este objeto pode ser "clicado/acionado").
{
    public void Interagir() // Método exigido pela interface. É acionado no exato momento em que o jogador encosta na mesa e aperta o botão de ação.
    {
        if (CraftingUIManager.Instance != null) // Trava de segurança: verifica se o "cérebro" (Singleton) do menu de crafting realmente existe na cena antes de tentar chamá-lo.
        {
            CraftingUIManager.Instance.AbrirBancada(); // Funciona como um interruptor muito simples: grita para o gerenciador dizendo "O jogador clicou em mim, abra a tela de montagem!".
        }
    }
}