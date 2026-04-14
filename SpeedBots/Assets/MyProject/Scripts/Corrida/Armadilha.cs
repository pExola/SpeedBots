using UnityEngine; // Importa a biblioteca principal da Unity para acessar as funcionalidades básicas do motor.

public class Armadilha : MonoBehaviour // Cria a classe que define o comportamento da bolinha física (item) jogada no chão.
{
    private void OnTriggerEnter2D(Collider2D collision) // Função ativada automaticamente no momento exato em que algum objeto encosta (pisa) na área da armadilha.
    {
        // 1. O primeiro aviso
        Debug.Log($"[ARMADILHA] Algo pisou em mim! Nome: {collision.gameObject.name}"); // Prática de QA: avisa no console da Unity exatamente o nome do objeto que pisou nela.

        SpeedBotMovment player = collision.GetComponent<SpeedBotMovment>(); // Tenta vasculhar o objeto que pisou nela para ver se ele tem o script de movimento do Jogador.
        SpeedBotIA ia = collision.GetComponent<SpeedBotIA>(); // Tenta vasculhar o objeto que pisou nela para ver se ele tem o script de movimento da Inteligência Artificial.

        if (player != null) // Se conseguiu achar o script do Jogador (ou seja, não é nulo, foi o Player quem pisou)...
        {
            Debug.Log("[ARMADILHA] Acertei o Player! Aplicando Stun e me destruindo..."); // QA: Avisa no console que o Player caiu na armadilha.
            player.TomarStunDeItem(1.5f); // Chama a função no script do Jogador para dar um choque/stun nele com duração de 1.5 segundos.
            Destroy(gameObject); // A armadilha comete suicídio (se destrói) sumindo da pista para não atordoar a pessoa duas vezes.
        }
        else if (ia != null) // Caso contrário, se conseguiu achar o script da IA (ou seja, foi o robô inimigo quem pisou)...
        {
            Debug.Log("[ARMADILHA] Acertei a IA! Aplicando Stun e me destruindo..."); // QA: Avisa no console que a IA caiu na armadilha.
            ia.TomarStunDeItem(1.5f); // Chama a função no script da IA para dar um choque/stun nela com duração de 1.5 segundos.
            Destroy(gameObject); // A armadilha se destrói e some da pista após cumprir seu objetivo.
        }
        else // Se o objeto que bateu na armadilha não tem nenhum dos dois scripts (ex: a parede, ou a própria pista)...
        {
            Debug.Log($"[ARMADILHA] O objeto {collision.gameObject.name} ignorado. Não tem os scripts de movimento dos robôs."); // QA: Avisa que a colisão foi ignorada e a armadilha continua quietinha no chão esperando a vítima certa.
        }
    }
}