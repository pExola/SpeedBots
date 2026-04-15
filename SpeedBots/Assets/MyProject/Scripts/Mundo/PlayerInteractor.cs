using UnityEngine; // Importa as ferramentas principais da engine da Unity.
using UnityEngine.InputSystem; // Importa o novo sistema de Inputs para ler o teclado com precisão.

public class PlayerInteractor : MonoBehaviour // É a "mão" e a "boca" do jogador no modo exploração do Overworld.
{
    // Define o comprimento do "laser invisível". Diferente da IA que atira longe, este atira um laser bem curtinho.
    public float interactRange = 1.0f;

    // Um filtro da Unity para o laser ignorar chão e paredes, batendo apenas em objetos da camada "Interactable".
    public LayerMask interactableLayer;

    private PlayerOverworld movement; // Guarda a referência do script de movimento do jogador.

    void Awake() // Chamado assim que o jogador nasce no mapa.
    {
        movement = GetComponent<PlayerOverworld>(); // Conecta o script de interação ao script de movimento para saber para onde o jogador está olhando.
    }

    void Update() // Roda a cada frame, atuando como os "ouvidos" do jogo para o seu teclado.
    {
        // Checa se o teclado existe E se as teclas "E" ou "Espaço" foram apertadas exatamente neste frame.
        if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
        {
            // O menu de diálogo já está aberto e o jogador está conversando agora?
            if (DialogueManager.Instance != null && DialogueManager.Instance.isTalking)
            {
                // Se sim, o botão serve para mandar a conversa avançar para a próxima fala.
                DialogueManager.Instance.AvancarDialogo();
            }
            else
            {
                // Se não estiver conversando com ninguém, ele tenta usar a "mão" para interagir com o mundo.
                TentarInteragir();
            }
        }
    }

    void TentarInteragir() // A função que dispara o laser para checar o que está na frente do jogador.
    {
        // 1. Prática de QA: Avisa no console que o botão funcionou e mostra para qual direção o jogador está olhando.
        Debug.Log($"[INTERAÇÃO] Botão apertado! Atirando raio na direção: {movement.lastFacingDirection}");

        // 2. MÁGICA VISUAL DE TESTES: Define de onde o raio sai (centro do jogador) e para onde vai (direção * tamanho do braço).
        Vector2 origem = transform.position;
        Vector2 direcao = movement.lastFacingDirection * interactRange;
        // Desenha uma linha vermelha na aba SCENE por 2 segundos para o desenvolvedor conseguir ver o raio invisível.
        Debug.DrawRay(origem, direcao, Color.red, 2f);

        // 3. Atira o raio (Raycast2D) de verdade na física do jogo, filtrando apenas pela camada configurada.
        RaycastHit2D hit = Physics2D.Raycast(origem, movement.lastFacingDirection, interactRange, interactableLayer);

        if (hit.collider != null) // Se o laser bater em algum objeto físico...
        {
            // Avisa no console o nome do objeto que tomou a "dedada".
            Debug.Log($"[INTERAÇÃO] O raio bateu em: {hit.collider.gameObject.name}");

            // Ele faz a pergunta crucial: "Você tem o selo IInteractable?" (Tenta buscar a interface no objeto).
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null) // Se a resposta for sim (o objeto assinou o contrato da interface)...
            {
                // Avisa que deu tudo certo.
                Debug.Log("[INTERAÇÃO] Sucesso! O objeto tem a interface. Iniciando diálogo...");

                // Aperta o "botão virtual" desse objeto, acionando o comportamento específico dele (abrir porta, falar, abrir loja).
                interactable.Interagir();
            }
            else // Se o objeto não tiver a interface...
            {
                // Avisa que você bateu em algo inútil que não sabe como reagir a interações.
                Debug.Log($"[INTERAÇÃO] Falha: O objeto {hit.collider.gameObject.name} não possui o script NPC/IInteractable.");
            }
        }
        else // Se o laser for atirado no vazio e não encostar em nada...
        {
            // Avisa que o golpe pegou no vento.
            Debug.Log("[INTERAÇÃO] O raio não bateu em nada. Verifique a distância ou a Layer no Inspector.");
        }
    }
}