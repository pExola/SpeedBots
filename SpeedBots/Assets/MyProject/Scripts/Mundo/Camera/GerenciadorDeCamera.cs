using UnityEngine; // Importa a biblioteca principal do motor da Unity.
using System.Collections; // Importa a biblioteca de coleções, essencial para podermos usar Corrotinas (IEnumerator).

public class GerenciadorDeCamera : MonoBehaviour // Cria a classe que atua como o "motorista" da câmera do Overworld.
{
    // Usa o poderoso padrão Singleton. Isso significa que só existe UM gerenciador de câmera no jogo inteiro, 
    // e qualquer outro script pode "ligar" para ele de qualquer lugar chamando GerenciadorDeCamera.Instance.
    public static GerenciadorDeCamera Instance { get; private set; }

    [Header("Configurações da Transição")] // Organiza o menu no Inspector.
    public bool transicaoSuave = true; // Chave para ligar/desligar o deslizamento suave da câmera.
    public float velocidadeTransicao = 30f; // A velocidade com que a câmera viaja de uma sala para a outra.

    private bool iniciou = false; // Flag para saber se o jogo acabou de começar (evita que a câmera deslize sozinha ao dar Play).

    // NOVA VARIÁVEL: Guarda a coordenada da sala para a qual a câmera está olhando no momento.
    private Vector3 alvoAtual;

    private void Awake()
    {
        // Configuração do Singleton: Se não existir nenhum gerenciador, este assume. Se já existir, ele se destrói.
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Inicia uma pequena contagem de atraso assim que a fase carrega.
        StartCoroutine(AtrasarInicio());
    }

    private IEnumerator AtrasarInicio()
    {
        // Espera o exato fim do primeiro frame do jogo para marcar que o jogo começou de verdade.
        // Isso garante que a câmera dê um teletransporte instantâneo para o jogador ao iniciar a fase, em vez de vir deslizando do infinito.
        yield return new WaitForEndOfFrame();
        iniciou = true;
    }

    // Função chamada pelos triggers (EnquadramentoCamera) quando você passa de uma "sala" para outra.
    public void MudarEnquadramento(Vector3 centroDaNovaSala)
    {
        // Pega as coordenadas X e Y da nova sala, mas mantém o Z atual da câmera para ela não bugar/afundar no cenário 2D.
        Vector3 posicaoAlvo = new Vector3(centroDaNovaSala.x, centroDaNovaSala.y, transform.position.z);

        // NOVA PROTEÇÃO (Eficiência e Anti-Bug): Se mandarem a câmera ir para uma sala que ela já está olhando, 
        // ela ignora o comando! Isso poupa processamento e evita loops infinitos.
        if (alvoAtual == posicaoAlvo) return;

        // Se for uma sala nova e válida, registra o novo foco no cérebro da câmera.
        alvoAtual = posicaoAlvo;

        // Se o jogo está no frame de carregamento (não iniciou completamente ainda)...
        if (!iniciou)
        {
            transform.position = posicaoAlvo; // ...teletransporta a câmera direto, sem fazer efeito suave.
            return; // Corta o resto do código.
        }

        if (transicaoSuave) // Se o efeito de deslizamento estiver ativado no Inspector...
        {
            StopAllCoroutines(); // Para qualquer viagem que a câmera estivesse fazendo antes...
            StartCoroutine(MoverCameraCoroutine(posicaoAlvo)); // ...e engata a marcha para a nova sala chamando a Corrotina.
        }
        else // Se a suavidade estiver desativada, simplesmente teletransporta a câmera de forma seca.
        {
            transform.position = posicaoAlvo;
        }
    }

    // A Corrotina: é aqui que a mágica do deslizamento temporal acontece.
    private IEnumerator MoverCameraCoroutine(Vector3 alvo)
    {
        // Enquanto a distância entre a câmera atual e o alvo for maior que "quase zero" (0.01f)...
        while (Vector3.Distance(transform.position, alvo) > 0.01f)
        {
            // O MoveTowards move a câmera só um pouquinho a cada frame, criando a transição contínua e suave.
            transform.position = Vector3.MoveTowards(transform.position, alvo, velocidadeTransicao * Time.deltaTime);

            // Pausa a função e manda a tela desenhar o frame. No frame seguinte, o "while" repete o ciclo!
            yield return null;
        }

        // Quando ela chegar extremamente perto do centro (distância menor que 0.01f), o while acaba,
        // e nós forçamos o teletransporte final para cravar o estacionamento milimetricamente perfeito.
        transform.position = alvo;
    }
}