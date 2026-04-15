using UnityEngine; // Importa as ferramentas principais do motor da Unity.
using UnityEngine.InputSystem; // Importa o novo sistema de Inputs para ler o teclado com alta precisão.

public class PlayerOverworld : MonoBehaviour // É o cérebro motor do jogador exclusivamente para o modo exploração (movimento top-down).
{
    public float moveSpeed = 5f; // Define a velocidade de caminhada do personagem pelo mapa.
    private Rigidbody2D rb; // Guarda a referência do corpo físico do jogador para podermos empurrá-lo.
    private Vector2 moveInput; // Guarda a direção desejada pelo jogador (eixos X e Y) com base nas teclas apertadas.

    // Lembra para qual lado o boneco olhou por último (o padrão ao nascer é olhar para baixo).
    // O [HideInInspector] esconde isso na Unity para não poluir a tela do desenvolvedor.
    [HideInInspector] public Vector2 lastFacingDirection = Vector2.down;

    // --- NOVA VARIÁVEL DO ANIMATOR ---
    private Animator anim; // Guarda o controlador de animações (Blend Tree) para podermos trocar os sprites de caminhada.

    void Awake() // Roda no exato momento em que o jogador nasce na cena.
    {
        rb = GetComponent<Rigidbody2D>(); // Conecta o motor de física do boneco ao script.
        anim = GetComponent<Animator>(); // Conecta o código ao componente que desenha as animações na tela.
    }

    void Update() // Roda a cada frame. É aqui que o jogo atua como o "ouvido" do seu teclado.
    {
        // 1. A TRAVA GENIAL: Checa se o painel de diálogo do jogo está aberto e você está conversando.
        if (DialogueManager.Instance != null && DialogueManager.Instance.isTalking)
        {
            moveInput = Vector2.zero; // Se estiver conversando, ele zera os controles instantaneamente.
            if (anim != null) anim.SetFloat("Speed", 0f); // Força a animação a parar, deixando o boneco em pose de repouso (Idle).
            return; // Corta a execução do código aqui, impedindo o boneco de sair andando enquanto o painel de texto está aberto.
        }

        moveInput = Vector2.zero; // Zera a intenção de movimento a cada frame para recalcular a direção do zero.

        if (Keyboard.current != null) // Checa se o teclado existe/está conectado.
        {
            // Lê o teclado e monta o vetor de direção (ex: apertar 'W' e 'D' faz o vetor ir para Cima e Direita).
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveInput.y = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveInput.y = -1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput.x = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x = 1f;
        }

        if (moveInput != Vector2.zero) // Se o jogador apertou alguma tecla (o vetor tem alguma direção)...
        {
            // Ele "normaliza" a direção. Isso é crucial em jogos Top-Down para garantir que o jogador não ande mais rápido ao andar na diagonal!
            lastFacingDirection = moveInput.normalized;

            // --- A MÁGICA DA ANIMAÇÃO: Envia a direção exata para a Blend Tree ---
            if (anim != null)
            {
                // Alimenta o Animator com os valores de X e Y, permitindo que ele escolha perfeitamente qual desenho exibir (virado pra cima, baixo, esquerda ou direita).
                anim.SetFloat("Horizontal", lastFacingDirection.x);
                anim.SetFloat("Vertical", lastFacingDirection.y);
            }
        }

        // --- Envia a velocidade para saber se o boneco está parado ou andando ---
        if (anim != null)
        {
            // O sqrMagnitude devolve um número maior que zero se estiver andando, ativando a animação das pernas na Blend Tree.
            anim.SetFloat("Speed", moveInput.sqrMagnitude);
        }
    }

    void FixedUpdate() // Roda no tempo fixo da engine. É aqui que aplicamos a física de verdade para evitar travamentos na tela.
    {
        // Pega a posição atual do corpo (rb.position) e empurra fisicamente somando a direção (moveInput), 
        // a velocidade (moveSpeed) e alinhando com o tempo da física (Time.fixedDeltaTime) para um movimento top-down limpo e suave.
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}