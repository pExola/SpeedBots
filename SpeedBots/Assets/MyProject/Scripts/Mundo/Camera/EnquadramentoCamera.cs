using UnityEngine; // Importa as ferramentas principais do motor da Unity.

[RequireComponent(typeof(BoxCollider2D))] // Exige que o objeto tenha um BoxCollider2D (o retângulo invisível) anexado para não dar erro no jogo.
public class EnquadramentoCamera : MonoBehaviour // Cria a classe que atua como o "Gatilho de Sala" no mapa.
{
    private BoxCollider2D colisor; // Variável para guardar a referência do retângulo invisível que delimita este cômodo.

    private void Awake() // Função chamada no momento exato em que o gatilho nasce na fase.
    {
        colisor = GetComponent<BoxCollider2D>(); // Pega a forma física (o retângulo) do objeto e guarda na memória.
    }

    // Trocamos o OnTriggerEnter pelo OnTriggerStay para verificar a posição continuamente a cada frame da física.
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Confere se quem está pisando na área é realmente o Jogador.
        {
            // O SEGREDO ANTI-BUG: A câmera ficaria louca (indo e voltando) se o jogador ficasse pisando na linha da fronteira com a "ponta do pé".
            // Para resolver isso, usamos o "colisor.bounds.Contains()". 
            // Ele só confirma a entrada quando o CENTRO ABSOLUTO do jogador (collision.transform.position) atravessa a fronteira para dentro da sala.
            if (colisor.bounds.Contains(collision.transform.position))
            {
                // Se o corpo inteiro do robô entrou, ele grita para o Singleton da câmera passar a focar no meio exato deste cômodo (colisor.bounds.center).
                GerenciadorDeCamera.Instance.MudarEnquadramento(colisor.bounds.center);
            }
        }
    }
}