using UnityEngine; // Importa as ferramentas principais do motor da Unity.
using UnityEngine.EventSystems; // Importa ferramentas de eventos da interface.
using UnityEngine.InputSystem; // Importa o novo Input System da Unity para ler teclado/controles com precisão.

public class SpeedBotMovment : MonoBehaviour // É o controle principal do Jogador, traduzindo o teclado em movimento físico e "Game Feel".
{
    public enum TipoBot { Crawler, Slider, Aerial } // Define as categorias que mudam a sensação de peso do robô.

    [Header("Identidade do Chassi")]
    public TipoBot tipoBot; // Guarda o chassi selecionado para este robô.

    [Tooltip("Crawler = 0.2 | Aerial = 0.5 | Slider = 0.9")]
    [Range(0f, 1f)] public float arrancadaBase = 0.5f; // Status que define o "boost" massivo ao sair do zero (motores explosivos).

    [Tooltip("Crawler = 0.9 | Aerial = 0.5 | Slider = 0.2")]
    [Range(0f, 1f)] public float durabilidadeBase = 0.5f; // Status de armadura que define a resistência a rampas e stuns de fogo/armadilhas.

    [Header("Atributos do Motor (Controlados pela Progressão)")]
    // Estes valores agora começam invisíveis no Inspector para não confundir, 
    // pois o script SpeedBotProgression que vai preenchê-los.
    [HideInInspector] public float velocidadeMaximaBase = 15f;
    [HideInInspector] public float aceleracaoBase = 30f;

    [Header("Parkour")] // Configurações para pulos e rebatidas nas paredes.
    public float forcaPulo = 12f;
    public float forcaWallJumpY = 14f;
    public float forcaWallJumpX = 4f;
    public float distanciaSensor = 0.1f;
    public Vector2 tamanhoCaixaSensor = new Vector2(0.1f, 0.8f);

    private Rigidbody2D rb; // Guarda o corpo físico real do jogador.
    private CapsuleCollider2D col; // Guarda o colisor (forma) do jogador.
    private bool isGrounded; // Sensor que diz se o jogador está pisando no chão.
    private bool isTouchingWall; // Sensor que diz se o jogador está de cara numa parede.
    private float lastMoveDirection = 1f; // Lembra para qual lado o jogador andou por último (1=direita, -1=esquerda).

    // --- VARIÁVEIS DE ESTADO ---
    private string terrenoAtual = "Normal"; // Diz se está no gelo, lama, normal, etc.
    private float stunTimer = 0f; // Cronômetro de choque/paralisia.
    private float debuffFogoTimer = 0f; // Cronômetro de penalidade ao se queimar.
    private float debuffGanchoTimer = 0f; // Cronômetro de lentidão ao ser puxado.
    private float multiplicadorNitro = 1f; // Força do item Nitro.
    private float nitroTimer = 0f; // Cronômetro do item Nitro.

    // --- NOVO: Controle de Arrancada ---
    private float tempoAcelerando = 0f; // Cronômetro que mede os 2 primeiros segundos para dar o efeito de "Estilingue".

    private Animator anim; // O gerente das animações (pernas, braços, etc).

    void Awake() // Função chamada assim que o jogo começa.
    {
        rb = GetComponent<Rigidbody2D>(); // Conecta o motor físico da Unity ao script.
        col = GetComponent<CapsuleCollider2D>(); // Conecta o colisor ao script.
        anim = GetComponent<Animator>(); // Conecta o animador ao script.
    }

    void Update() // Roda a cada frame para ler o teclado imediatamente.
    {
        if (Keyboard.current == null) return; // Trava de segurança caso não tenha teclado.

        float moveInput = 0f; // Variável temporária para saber para onde o jogador quer ir.

        // Lê DIRETAMENTE o novo Input System da Unity:
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1f;

        // Se o jogador apertou algo, guarda essa direção para saber para onde ele está olhando.
        if (moveInput != 0) lastMoveDirection = moveInput;

        VerificarParede(); // Liga o sensor de parkour que checa se há parede na frente.

        // Se o jogador não estiver paralisado e apertar Espaço...
        if (stunTimer <= 0 && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isGrounded) PuloNormal(); // Se estiver no chão, dá um pulo normal.
            else if (isTouchingWall) WallJump(); // Se estiver no ar tocando na parede, rebate nela (Parkour).
        }

        AtualizarAnimacoes(); // Despacha todos os cálculos de estado para o Animator.
    }

    void FixedUpdate() // Roda no tempo fixo da engine. É aqui que o "Game Feel" físico acontece.
    {
        if (stunTimer > 0) // Se o robô tomou um Stun (Fogo/Armadilha)...
        {
            stunTimer -= Time.fixedDeltaTime; // Abate o tempo do stun.
            // Congela a inércia, cortando a velocidade horizontal para dar impacto ao choque.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.8f, rb.linearVelocity.y);
            tempoAcelerando = 0f; // Zera a arrancada se tomar stun (perde o embalo).
            return; // Impede que o jogador se mova enquanto estiver atordoado.
        }

        float moveInput = 0f;
        if (Keyboard.current != null) // Lê a intenção de movimento do jogador de novo para aplicar a força.
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput = 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveInput = -1f;
        }

        // --- INÍCIO DO CÁLCULO DE RPG E GAME FEEL ---
        float velMaxAtual = velocidadeMaximaBase; // Puxa os limites do robô.
        float acelAtual = aceleracaoBase; // Puxa a potência do robô.
        float friccao = 0.9f; // Fricção natural do chão.

        // A ARRANCADA (O ESTILINGUE):
        if (moveInput != 0 && isGrounded) // Se o jogador está tentando andar no chão...
        {
            tempoAcelerando += Time.fixedDeltaTime; // Conta o tempo em que ele está apertando o botão.
            if (tempoAcelerando <= 2.0f) // Nos primeiros 2 segundos de corrida...
            {
                // MÁGICA DO GAME FEEL: Robôs com status alto de arrancadaBase ganham um "boost" massivo ao sair do zero!
                // Arrancada 0.0 (Slider Nível 1) = Aceleração x1.2 e Velocidade x1.0
                // Arrancada 1.0 (Slider Nível 20) = Aceleração x3.0 e Velocidade x1.3 (Ele ultrapassa o limite físico!)
                float bonusAcel = Mathf.Lerp(1.2f, 3.0f, arrancadaBase);
                float bonusVel = Mathf.Lerp(1.0f, 1.3f, arrancadaBase);

                acelAtual *= bonusAcel; // Multiplica a aceleração pelo bônus.
                velMaxAtual *= bonusVel; // Multiplica a velocidade máxima pelo bônus.
            }
        }
        else if (moveInput == 0) // Se ele soltar o botão de andar...
        {
            tempoAcelerando = 0f; // Reseta a arrancada para ele poder dar o "estilingue" de novo depois.
        }

        // 2. SISTEMA DE PESO, INTERAÇÕES E TERRENOS:
        if (terrenoAtual == "Lama") // Se entrar no Trigger de Lama...
        {
            if (tipoBot == TipoBot.Crawler) // Robô pesado (trator) adora lama.
            {
                velMaxAtual *= 1.3f; acelAtual *= 1.3f; // Fica mais rápido.
            }
            else if (tipoBot == TipoBot.Slider || tipoBot == TipoBot.Aerial) // Robôs leves odeiam lama.
            {
                // Usa a Durabilidade como Armadura contra a lentidão:
                // Durabilidade 0.0 = Cai para 20% da velocidade (quase atola)
                // Durabilidade 1.0 = Segura 85% da velocidade (passa rasgando)
                float retencaoStatus = Mathf.Lerp(0.2f, 0.85f, durabilidadeBase);
                velMaxAtual *= retencaoStatus;
                acelAtual *= (retencaoStatus - 0.1f);
            }
        }
        else if (terrenoAtual == "Gelo") // Se entrar no Trigger de Gelo...
        {
            friccao = 0.99f; // O chão fica super escorregadio.
            if (tipoBot == TipoBot.Slider) // Robô patinador adora gelo.
            {
                velMaxAtual *= 1.4f; acelAtual *= 1.4f;
            }
            else if (tipoBot == TipoBot.Crawler || tipoBot == TipoBot.Aerial)
            {
                float retencaoStatus = Mathf.Lerp(0.2f, 0.85f, durabilidadeBase);
                velMaxAtual *= retencaoStatus;
                acelAtual *= (retencaoStatus - 0.1f);
            }
        }

        // --- APLICAÇÃO FÍSICA E RAMPAS ---

        // SISTEMA DE PESO E RAMPA: Igual à IA, pune subidas pesadas.
        if (rb.linearVelocity.y > 0.5f && isGrounded) // Se a velocidade vertical for positiva (está subindo ladeira)...
        {
            // Durabilidade 0.0 = Perde 70% da força do motor na subida.
            // Durabilidade 1.0 = Ignora a subida e mantém 100% de força.
            float penalidadeRampa = Mathf.Lerp(0.3f, 1.0f, durabilidadeBase);
            acelAtual *= penalidadeRampa; // Pune a aceleração.
        }

        // Passiva natural do chassi Aerial
        if (tipoBot == TipoBot.Aerial)
        {
            velMaxAtual *= 1.15f; // Voadores têm velocidade máxima um pouco maior.
            acelAtual *= 1.10f;
        }

        // 3. Debuffs de Combate e Nitro (Lidos a partir do inventário/itens)
        if (debuffFogoTimer > 0)
        {
            debuffFogoTimer -= Time.fixedDeltaTime;
            velMaxAtual *= 0.5f; acelAtual *= 0.5f; // Corta a velocidade pela metade se queimado.
        }

        if (debuffGanchoTimer > 0)
        {
            debuffGanchoTimer -= Time.fixedDeltaTime;
            velMaxAtual *= 0.4f; acelAtual *= 0.4f; // Lentidão extrema após ser pescado.
        }

        if (nitroTimer > 0) // Se usou o Nitro...
        {
            nitroTimer -= Time.fixedDeltaTime;
            velMaxAtual *= multiplicadorNitro; // O limite máximo explode positivamente.
            acelAtual *= multiplicadorNitro;
        }
        else { multiplicadorNitro = 1f; } // Sem nitro, multiplicador volta a neutro.

        // --- APLICAÇÃO FÍSICA FINAL ---
        // NOVA DURABILIDADE EM RAMPAS (Reforço):
        if (rb.linearVelocity.y > 0.5f && isGrounded)
        {
            float penalidadeRampa = Mathf.Lerp(0.7f, 1.0f, durabilidadeBase);
            acelAtual *= penalidadeRampa;
        }

        // Se ainda não bateu na velocidade limite, empurra o robô fisicamente (AddForce) na direção desejada.
        if (Mathf.Abs(rb.linearVelocity.x) < velMaxAtual && moveInput != 0)
        {
            rb.AddForce(new Vector2(moveInput * acelAtual, 0), ForceMode2D.Force);
        }

        // Se ele não estiver apertando nada e estiver no chão, aplica a fricção para frear naturalmente.
        if (moveInput == 0 && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * friccao, rb.linearVelocity.y);
        }

        // Se algo (explosão, descida, nitro) fez ele passar do limite natural...
        if (Mathf.Abs(rb.linearVelocity.x) > velMaxAtual)
        {
            // Puxa a velocidade dele suavemente de volta para o limite para ele não "quebrar" o jogo voando pra fora.
            float velXSuave = Mathf.Lerp(rb.linearVelocity.x, velMaxAtual * Mathf.Sign(rb.linearVelocity.x), 0.1f);
            rb.linearVelocity = new Vector2(velXSuave, rb.linearVelocity.y);
        }
    }

    private void VerificarParede() // Sensor que checa paredes para o Wall Jump (Parkour).
    {
        Vector2 centro = col.bounds.center; // Pega o umbigo do jogador.
        Vector2 direcao = lastMoveDirection > 0 ? Vector2.right : Vector2.left; // Vê para onde ele tá olhando.

        // Coloca a caixa de verificação EXATAMENTE onde o Gizmo desenha na tela (na frente do rosto).
        Vector2 destino = centro + (direcao * (col.bounds.extents.x + distanciaSensor));

        // Pega todos os colisores que estão tocando na caixa invisível.
        Collider2D[] hits = Physics2D.OverlapBoxAll(destino, tamanhoCaixaSensor, 0f);

        isTouchingWall = false; // Começa assumindo que não tem parede.

        foreach (Collider2D hit in hits)
        {
            // Se qualquer coisa dentro da caixa tiver a tag Parede, ele avisa que dá pra fazer parkour!
            if (hit.CompareTag("Parede"))
            {
                isTouchingWall = true;
                break;
            }
        }
    }

    private void AtualizarAnimacoes() // Despacha todos os cálculos matemáticos para o Animator desenhar.
    {
        if (anim == null || rb == null) return;

        // 1. Envia a Velocidade Horizontal para tocar a animação de corrida (usando o linearVelocity atualizado).
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));

        // 2. Envia a Velocidade Vertical para decidir entre animação de pulo ou queda.
        anim.SetFloat("yVelocity", rb.linearVelocity.y);

        // 3. Envia o sensor de Chão direto para o Animator saber se o robô tá pisando firme.
        anim.SetBool("isGrounded", isGrounded);

        // --- NOVA LÓGICA DE ESPELHAMENTO (VIRAR PARA A ESQUERDA/DIREITA) ---
        // Se a intenção for ir para a direita, a escala X é 1 (normal).
        if (lastMoveDirection > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
            // NOTA 2.5D: Se os seus modelos forem 3D e amassar a escala bugar a iluminação, 
            // use a rotação em vez do localScale: transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        // Se a intenção for ir para a esquerda, a escala X vira -1 (espelhado).
        else if (lastMoveDirection < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
            // NOTA 2.5D: Alternativa com rotação: transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
    }

    private void PuloNormal() // Ação de Parkour: Pulo do chão.
    {
        float impulsoFinal = (tipoBot == TipoBot.Aerial) ? forcaPulo * 1.3f : forcaPulo; // Voadores pulam 30% mais alto.
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Zera o peso da queda antes de pular.
        rb.AddForce(Vector2.up * impulsoFinal, ForceMode2D.Impulse); // Chuta o robô para cima com a Unity Physics.
    }

    private void WallJump() // Ação de Parkour: Rebatida na parede.
    {
        float puloYFinal = (tipoBot == TipoBot.Aerial) ? forcaWallJumpY * 1.2f : forcaWallJumpY; // Voadores rebatem mais alto.
        float puloXFinal = (tipoBot == TipoBot.Aerial) ? forcaWallJumpX * 1.2f : forcaWallJumpX; // Voadores rebatem mais longe.

        rb.linearVelocity = Vector2.zero; // Zera a inércia inteira para não deslizar errado.
        rb.AddForce(new Vector2(lastMoveDirection * puloXFinal, puloYFinal), ForceMode2D.Impulse); // Chuta na diagonal.
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Garante que só considera chão se a superfície for plana ou rampa (impede pulo infinito em tetos ou paredes perfeitas).
        if (collision.contacts[0].normal.y > 0.5f) isGrounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false; // Se saiu da colisão, avisa o sistema que o jogador está voando/caindo.
    }

    // --- TRIGGERS DOS TERRENOS E INTERAÇÕES ---
    // Detecta quando o robô ENTRA em uma zona especial
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Lama")) terrenoAtual = "Lama"; // Entrou na lama.
        if (collision.CompareTag("Gelo")) terrenoAtual = "Gelo"; // Entrou no gelo.

        if (collision.CompareTag("Fogo"))
        {
            // O Fogo congela a inércia (linearVelocity.x * 0.8f lá em cima) e aplica um Stun que varia conforme a armadura!
            // Durabilidade 1.0 (Crawler) toma só 0.1s de stun. Durabilidade 0.0 toma 1.2s.
            stunTimer = Mathf.Lerp(1.2f, 0.1f, durabilidadeBase);
            debuffFogoTimer = 3.0f; // O robô fica manco/queimado por 3 segundos depois do stun.
            Debug.Log($"FOGO! Stun de {stunTimer}s aplicado.");
        }
    }

    // --- MÉTODOS PÚBLICOS DE COMBATE (Chamados pelos Itens) ---
    public void AtivarNitro(float forca, float duracao)
    {
        multiplicadorNitro = forca; // Define a força do tiro de nitro.
        nitroTimer = duracao; // Define quantos segundos o nitro dura.
    }

    public void TomarStunDeItem(float tempoBase)
    {
        // A durabilidadeBase do seu RPG reduz o tempo do choque da armadilha inimiga!
        stunTimer = Mathf.Lerp(tempoBase, tempoBase * 0.2f, durabilidadeBase);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.3f, rb.linearVelocity.y); // Freia violentamente 70% na hora do choque.
    }

    public void SofrerPuxao(float forcaPuxao, float direcaoX, float tempoDebuff)
    {
        rb.linearVelocity = Vector2.zero; // Zera a inércia.
        rb.AddForce(new Vector2(direcaoX * forcaPuxao, 4f), ForceMode2D.Impulse); // Dá um puxão reverso com um pequeno solavanco pro alto (4f).

        // Aplica o tempo de lentidão no motor após ser pescado!
        debuffGanchoTimer = tempoDebuff;
    }

    // Retorna a direção para o script da Arma/Gancho saber para onde atirar o raio.
    public float GetDirecaoOlhar()
    {
        return lastMoveDirection;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Se saiu do trigger da Lama ou do Gelo, o robô percebe que voltou para o chão limpo.
        if (collision.CompareTag("Lama") || collision.CompareTag("Gelo"))
        {
            terrenoAtual = "Normal";
        }
    }

    private void OnDrawGizmos() // Função de desenvolvedor: desenha o sensor visual na cena da Unity.
    {
        if (col == null) col = GetComponent<CapsuleCollider2D>();
        if (col == null) return;

        Vector2 centro = col.bounds.center;
        Vector2 direcao = lastMoveDirection > 0 ? Vector2.right : Vector2.left;
        Vector2 destino = centro + (direcao * (col.bounds.extents.x + distanciaSensor));

        // A caixa vai ficar verde quando detectar uma parede (Ground), e vermelha se for ar livre.
        Gizmos.color = isTouchingWall ? Color.green : Color.red;
        Gizmos.DrawWireCube(destino, tamanhoCaixaSensor);
    }
}