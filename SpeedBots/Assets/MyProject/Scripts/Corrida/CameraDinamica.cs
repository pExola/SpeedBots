using UnityEngine; // Importa a biblioteca básica da Unity para o script funcionar.

public class CameraDinamica : MonoBehaviour // Cria a classe do nosso "câmera-man" inteligente.
{
    [Header("Alvo e Configurações")] // Cria um cabeçalho bonito no Inspector da Unity para organização.
    public Transform alvo; // Guarda a referência física do jogador (o ponto que a câmera vai seguir).

    public float suavidade = 0.15f; // O "peso" do elástico. Quanto maior, mais a câmera demora para alcançar o alvo.

    [Header("Posicionamento (Offset)")] // Novo cabeçalho para as distâncias de avanço.
    public Vector3 avancoDireita = new Vector3(6f, 2f, -10f); // Quanto a câmera olha à frente (+6 em X) quando o robô vai para a direita. O Z=-10 mantém a câmera fora da tela.
    public Vector3 avancoEsquerda = new Vector3(-6f, 2f, -10f); // Quanto a câmera olha à frente (-6 em X) quando o robô vai para a esquerda.

    private Vector3 velocidadeRef = Vector3.zero; // Uma variável "invisível" que a matemática da Unity exige para calcular a aceleração do elástico nos bastidores.
    private SpeedBotMovment motorPlayer; // Espaço reservado para guardar o "cérebro" (script de movimento) do jogador.

    void Start() // Função ativada apenas uma vez, no exato frame em que a fase carrega.
    {
        // 1. Verifica se você arrastou o objeto do jogador para a gaveta "Alvo" lá na Unity.
        if (alvo == null)
        {
            return; // Se não arrastou, ele cancela o Start aqui para o jogo não travar com erro.
        }

        // Vai até o objeto do jogador (alvo) e rouba a referência do script de movimento dele.
        motorPlayer = alvo.GetComponent<SpeedBotMovment>();
    }

    void LateUpdate() // Roda a cada frame, mas SÓ DEPOIS que todo mundo (incluindo o jogador) já se moveu. Isso evita a "tremedeira" de câmera.
    {
        // Trava de segurança: se o jogador morrer ou perder o motor, a câmera para de calcular.
        if (alvo == null || motorPlayer == null) return;

        // O câmera-man pergunta ao motor: "Para qual lado o jogador está olhando?" (Ex: 1 para direita, -1 para esquerda).
        float direcao = motorPlayer.GetDirecaoOlhar();

        // Toma a decisão: Se a direção for maior que zero (direita), usa o avanço da direita. Caso contrário, usa o da esquerda.
        Vector3 offsetDesejado = (direcao > 0) ? avancoDireita : avancoEsquerda;

        // Calcula exatamente onde a câmera DEVERIA estar (posição atual do jogador + o avanço escolhido).
        Vector3 posicaoAlvo = alvo.position + offsetDesejado;

        // A mágica visual do elástico: Em vez de teletransportar a câmera, o SmoothDamp desliza ela da posição atual até a "posicaoAlvo" com o atraso da "suavidade", matando movimentos bruscos.
        transform.position = Vector3.SmoothDamp(transform.position, posicaoAlvo, ref velocidadeRef, suavidade);
    }
}