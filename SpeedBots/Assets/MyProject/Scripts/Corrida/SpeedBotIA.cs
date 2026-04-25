using UnityEngine; // Importa a biblioteca principal da Unity.

public class SpeedBotIA : MonoBehaviour // Cria a classe que atua como o cérebro autônomo dos corredores inimigos.
{
    public enum TipoBot { Crawler, Slider, Aerial } // Define os 3 tipos de chassi possíveis.

    [Header("Identidade do Chassi")]
    public TipoBot tipoBot; // Guarda qual é o chassi desta IA.

    [Tooltip("Crawler = 0.2 | Aerial = 0.5 | Slider = 0.9")]
    [Range(0f, 1f)] public float arrancadaBase = 0.5f; // Status de arrancada da IA.

    [Tooltip("Crawler = 0.9 | Aerial = 0.5 | Slider = 0.2")]
    [Range(0f, 1f)] public float durabilidadeBase = 0.5f; // Status de durabilidade/resistência da IA.

    [Header("Atributos Base do Motor (Controlados pela Progressão)")]
    [HideInInspector] public float velocidadeMaximaBase = 15f; // Velocidade base escondida no Inspector.
    [HideInInspector] public float aceleracaoBase = 30f; // Aceleração base escondida no Inspector.

    [Header("Parkour")]
    public float forcaPulo = 12f; // Força do pulo normal.
    public float forcaWallJumpY = 14f; // Força vertical ao pular da parede.
    public float forcaWallJumpX = 4f; // Força horizontal ao pular da parede.

    [Header("Sensores Frontais e Buraco")]
    public float distanciaSensorFrente = 0.5f; // O quão longe a IA enxerga paredes.
    public Vector2 tamanhoCaixaSensor = new Vector2(0.1f, 0.8f); // O tamanho do sensor de paredes.
    public float distanciaOlhoBuraco = 1.0f; // Tamanho do laser que olha para o chão.
    public float avancoOlhoBuraco = 0.8f; // Distância que o laser do buraco fica à frente do robô.

    private Rigidbody2D rb; // Guarda o corpo físico do robô.
    private CapsuleCollider2D col; // Guarda o colisor do robô.
    private bool isGrounded; // Sabe se a IA está pisando no chão.
    private bool isTouchingWall; // Sabe se a IA bateu de cara numa parede.
    private float moveDirection = 1f; // Direção que a IA está indo (1 = direita, -1 = esquerda).

    // --- VARIÁVEIS DE ESTADO ---
    private string terrenoAtual = "Normal"; // Guarda qual terreno a IA está pisando agora.
    private float stunTimer = 0f; // Cronômetro de atordoamento.
    private float debuffFogoTimer = 0f; // Cronômetro de penalidade por pisar no fogo.
    private float multiplicadorNitro = 1f; // Força extra do nitro.
    private float nitroTimer = 0f; // Cronômetro do nitro.
    private float debuffGanchoTimer = 0f; // Cronômetro de lentidão ao ser puxada pelo gancho.
    private float tempoAcelerando = 0f; // Tempo usado para calcular o efeito "estilingue" da arrancada.
    private Animator anim; // Guarda o controlador de animações.
    private float direcaoPistaAtual = 1f; // A corrida começa indo para a direita por padrão.

    void Awake() // Executa ao nascer na fase.
    {
        rb = GetComponent<Rigidbody2D>(); // Captura o motor físico.
        col = GetComponent<CapsuleCollider2D>(); // Captura a forma física.
        anim = GetComponent<Animator>(); // Captura o animador.
    }

    void Update() // Os "Olhos" e a Tomada de Decisão da IA (Roda a cada frame)
    {
        if (stunTimer > 0) return; // Se a IA tomou choque/stun, ela paralisa a mente e não toma decisões.

        VerificarAmbiente(); // Usa o sensor (OverlapBoxAll) para ver se há paredes na frente.

        if (isGrounded) // Se a IA está no chão...
        {
            // Ela decide pular SE: tem parede na frente OU o laser não achou chão à frente OU o terreno atual é ruim para a classe dela.
            if (isTouchingWall || DetectarBuraco() || DevePularDoTerreno())
            {
                PuloNormal(); // Executa o pulo.
            }
        }
        else if (isTouchingWall) // Se não está no chão, mas está tocando na parede (caindo no penhasco)...
        {
            WallJump(); // Executa o pulo na parede para se salvar.
        }

        AtualizarAnimacoes(); // Atualiza os sprites baseado na física.
    }

    void FixedUpdate() // A Física e Sinergias (Roda no tempo da física da Unity)
    {
        if (TelaResultados.Instance != null && !TelaResultados.Instance.corridaLiberada)
        {
            // Trava a IA no lugar, mantendo a gravidade funcionando caso ela nasça caindo, mas zerando a corrida
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (!isGrounded && isTouchingWall && rb.linearVelocity.y > 0) { } // Ajuste fino de inércia em pulos na parede.
        else if (isGrounded) { moveDirection = direcaoPistaAtual; } // Garante que a IA sempre siga o fluxo da pista ditado pelo level design

        if (stunTimer > 0) // Se estiver atordoada...
        {
            stunTimer -= Time.fixedDeltaTime; // Diminui o tempo de atordoamento.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.8f, rb.linearVelocity.y); // Freia a IA perdendo inércia aos poucos.
            tempoAcelerando = 0f; // Zera a arrancada se tomar stun.
            return; // Interrompe o resto da física.
        }

        float velMaxAtual = velocidadeMaximaBase; // Puxa a velocidade base para calcular.
        float acelAtual = aceleracaoBase; // Puxa a aceleração base para calcular.
        float friccao = 0.9f; // Fricção padrão da pista.

        // 1. ARRANCADA DA IA (O Efeito Estilingue simulando o jogador)
        if (isGrounded)
        {
            tempoAcelerando += Time.fixedDeltaTime; // Soma o tempo que ela está acelerando do zero.
            if (tempoAcelerando <= 2.0f) // Nos primeiros 2 segundos...
            {
                float bonusAcel = Mathf.Lerp(1.2f, 3.0f, arrancadaBase); // Transforma o status de arrancada num bônus de aceleração.
                float bonusVel = Mathf.Lerp(1.0f, 1.3f, arrancadaBase); // Transforma o status de arrancada num limite extra de velocidade.
                acelAtual *= bonusAcel; // Aplica o multiplicador de arrancada.
                velMaxAtual *= bonusVel; // Ultrapassa o limite de velocidade provisoriamente.
            }
        }

        // 2. SINERGIAS DE TERRENO E DURABILIDADE EXTREMA
        if (terrenoAtual == "Lama") // A mágica da Lama.
        {
            if (tipoBot == TipoBot.Crawler) // Se for o trator (Crawler)...
            {
                velMaxAtual *= 1.3f; acelAtual *= 1.3f; // Ele ganha muito status na lama!
            }
            else if (tipoBot == TipoBot.Slider || tipoBot == TipoBot.Aerial) // Se forem ágeis...
            {
                // A durabilidade base amortece a perda: status 0.0 cai pra 20% da velocidade, 1.0 segura 85%.
                float retencaoStatus = Mathf.Lerp(0.2f, 0.85f, durabilidadeBase);
                velMaxAtual *= retencaoStatus; // Aplica a penalidade.
                acelAtual *= (retencaoStatus - 0.1f); // Aplica a penalidade de aceleração um pouco mais forte.
            }
        }
        else if (terrenoAtual == "Gelo") // A mágica do Gelo.
        {
            friccao = 0.99f; // Fica super escorregadio.
            if (tipoBot == TipoBot.Slider) // Se for Slider (patinador)...
            {
                velMaxAtual *= 1.4f; acelAtual *= 1.4f; // Ele adora gelo e ganha status.
            }
            else if (tipoBot == TipoBot.Crawler || tipoBot == TipoBot.Aerial) // Se forem os outros...
            {
                float retencaoStatus = Mathf.Lerp(0.2f, 0.85f, durabilidadeBase); // Durabilidade tenta salvá-los.
                velMaxAtual *= retencaoStatus;
                acelAtual *= (retencaoStatus - 0.1f);
            }
        }

        // DURABILIDADE NAS RAMPAS
        // Se a velocidade em Y for positiva (subindo ladeira) e estiver no chão...
        if (rb.linearVelocity.y > 0.5f && isGrounded)
        {
            // O robô perde aceleração devido à gravidade. Um robô com alta durabilidadeBase sofre menos com isso.
            float penalidadeRampa = Mathf.Lerp(0.3f, 1.0f, durabilidadeBase);
            acelAtual *= penalidadeRampa; // Aplica o peso da rampa no motor.
        }

        // Passiva do chassi aéreo
        if (tipoBot == TipoBot.Aerial)
        {
            velMaxAtual *= 1.15f; acelAtual *= 1.10f; // Voadores são naturalmente um pouco mais rápidos.
        }

        // 3. DEBUFFS DE COMBATE
        if (debuffFogoTimer > 0) // Se passou no fogo recentemente...
        {
            debuffFogoTimer -= Time.fixedDeltaTime; // Abate o tempo.
            velMaxAtual *= 0.5f; acelAtual *= 0.5f; // Corta o motor pela metade enquanto estiver queimado.
        }

        if (debuffGanchoTimer > 0) // Se o player acertou um gancho nela...
        {
            debuffGanchoTimer -= Time.fixedDeltaTime; // Abate o tempo.
            velMaxAtual *= 0.4f; acelAtual *= 0.4f; // A IA fica extremamente mancada.
        }

        if (nitroTimer > 0) // Se a IA ativou nitro...
        {
            nitroTimer -= Time.fixedDeltaTime; // Abate o tempo.
            velMaxAtual *= multiplicadorNitro; // Multiplica a velocidade de acordo com a força da arma.
            acelAtual *= multiplicadorNitro;
        }
        else { multiplicadorNitro = 1f; } // Sem nitro, multiplicador volta a ser neutro (1).

        // --- APLICAÇÃO FÍSICA FINAL ---

        // Depois de todas essas contas malucas de terreno, fogo e rampa, se a IA ainda não chegou no limite...
        if (Mathf.Abs(rb.linearVelocity.x) < velMaxAtual)
        {
            // O script empurra o robô usando a função AddForce (Força física real).
            rb.AddForce(new Vector2(moveDirection * acelAtual, 0), ForceMode2D.Force);
        }

        // Se por algum motivo (nitro, explosão ou descida) ela passar da velocidade máxima permitida...
        if (Mathf.Abs(rb.linearVelocity.x) > velMaxAtual)
        {
            // Trava a velocidade máxima usando a suavização do Mathf.Lerp para ela não "voar" quebrando a física do jogo.
            float velXSuave = Mathf.Lerp(rb.linearVelocity.x, velMaxAtual * Mathf.Sign(rb.linearVelocity.x), 0.1f);
            rb.linearVelocity = new Vector2(velXSuave, rb.linearVelocity.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) // Avisa quando entra em zonas de terreno
    {
        if (collision.CompareTag("Lama")) terrenoAtual = "Lama";
        if (collision.CompareTag("Gelo")) terrenoAtual = "Gelo";
        if (collision.CompareTag("Fogo"))
        {
            stunTimer = Mathf.Lerp(1.2f, 0.1f, durabilidadeBase); // Quanto maior a armadura, menos tempo a IA passa atordoada no fogo.
            debuffFogoTimer = 3.0f; // Fica queimada por 3s.
        }

        DiretorDePista diretor = collision.GetComponent<DiretorDePista>();
        if (diretor != null)
        {
            // A IA memoriza que o fluxo da pista mudou. O FixedUpdate vai cuidar de virar o boneco!
            direcaoPistaAtual = diretor.novaDirecao;
            Debug.Log($"[IA] Mudando fluxo da corrida para: {direcaoPistaAtual}");
        }

    }

    private void OnTriggerExit2D(Collider2D collision) // Limpa o terreno ao sair da zona
    {
        if (collision.CompareTag("Lama") || collision.CompareTag("Gelo")) terrenoAtual = "Normal";
    }

    private void PuloNormal() // Executa o salto.
    {
        float impulsoFinal = (tipoBot == TipoBot.Aerial) ? forcaPulo * 1.3f : forcaPulo; // Aerial pula mais alto.
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Zera a inércia de queda antes de pular.
        rb.AddForce(Vector2.up * impulsoFinal, ForceMode2D.Impulse); // Chuta a IA para cima com Impulse.
    }

    private void AtualizarAnimacoes() // Manda os números matemáticos para o boneco desenhado.
    {
        if (anim == null || rb == null) return;
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x)); // Anima pernas correndo dependendo da inércia em X.
        anim.SetFloat("yVelocity", rb.linearVelocity.y); // Anima pose de pulo/queda dependendo da inércia em Y.
        anim.SetBool("isGrounded", isGrounded); // Diz ao animador se os pés estão tocando a terra.

        if (moveDirection > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveDirection < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private bool DevePularDoTerreno() // Inteligência extra: A IA tenta fugir de buracos ou terrenos ruins pra ela.
    {
        if (terrenoAtual == "Fogo") return true; // Todo mundo pula pra fugir do fogo.
        if (tipoBot == TipoBot.Slider && terrenoAtual == "Lama") return true; // Slider tenta pular a lama.
        if (tipoBot == TipoBot.Crawler && terrenoAtual == "Gelo") return true; // Crawler tenta pular o gelo.
        if (tipoBot == TipoBot.Aerial && (terrenoAtual == "Lama" || terrenoAtual == "Gelo")) return true; // Aerial odeia os dois.
        return false;
    }

    private void WallJump() // Executa o pulo de parede.
    {
        moveDirection = moveDirection * -1; // Inverte o lado que ela está olhando.
        float puloYFinal = (tipoBot == TipoBot.Aerial) ? forcaWallJumpY * 1.2f : forcaWallJumpY;
        float puloXFinal = (tipoBot == TipoBot.Aerial) ? forcaWallJumpX * 1.2f : forcaWallJumpX;

        rb.linearVelocity = Vector2.zero; // Anula o peso da queda.
        rb.AddForce(new Vector2(moveDirection * puloXFinal, puloYFinal), ForceMode2D.Impulse); // Chuta na diagonal.
    }

    private void VerificarAmbiente() // O "Sensor de Parede"
    {
        Vector2 centro = col.bounds.center;
        Vector2 destinoCaixa = centro + (new Vector2(moveDirection, 0) * (col.bounds.extents.x + distanciaSensorFrente)); // Posiciona o sensor na frente do rosto.

        // Desenha uma caixa invisível (OverlapBoxAll) e vê o que tem dentro.
        Collider2D[] hits = Physics2D.OverlapBoxAll(destinoCaixa, tamanhoCaixaSensor, 0f);
        isTouchingWall = false;
        // Se uma parede entrou nessa caixa secreta, ele avisa a mente: bateu parede!
        foreach (Collider2D hit in hits) { if (hit.CompareTag("Parede")) { isTouchingWall = true; break; } }
    }

    private bool DetectarBuraco() // O "Sensor de Abismo"
    {
        float centroX = col.bounds.center.x;
        float centroY = col.bounds.center.y;
        Vector2 origem = new Vector2(centroX + (avancoOlhoBuraco * moveDirection), centroY); // Posiciona o olho na frente e abaixo.
        float distanciaRaio = col.bounds.extents.y + 0.8f;

        // Dispara um laser (Raycast2D) para baixo e para frente.
        RaycastHit2D hit = Physics2D.Raycast(origem, Vector2.down, distanciaRaio);

        // Se o laser atirar pro chão e não bater em NADA, a IA sabe que ali tem um abismo.
        if (hit.collider == null) return true;

        string tag = hit.collider.tag;
        // Se o laser bateu em algo, mas não é uma pista ou terreno pisável (ex: bateu numa armadilha no ar), ela também considera abismo.
        if (tag != "Pista" && tag != "Lama" && tag != "Gelo" && tag != "Fogo") return true;
        return false; // Tem chão, é seguro correr.
    }

    // --- FUNÇÕES CHAMADAS PELO SISTEMA DE ARSENAL ---
    public void AtivarNitro(float forca, float duracao) { multiplicadorNitro = forca; nitroTimer = duracao; }

    public void TomarStunDeItem(float tempoBase)
    {
        // Usa a armadura de RPG (durabilidade) para diminuir o tempo de atordoamento dos choques!
        stunTimer = Mathf.Lerp(tempoBase, tempoBase * 0.2f, durabilidadeBase);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.3f, rb.linearVelocity.y); // Toma um tranco na hora perdendo 70% da inércia.
    }

    public void SofrerPuxao(float forcaPuxao, float direcaoX, float tempoDebuff)
    {
        rb.linearVelocity = Vector2.zero; // Anula a velocidade atual.
        rb.AddForce(new Vector2(direcaoX * forcaPuxao, 4f), ForceMode2D.Impulse); // Sofre um puxão invertido e uma pequena jogada pra cima no eixo Y.
        debuffGanchoTimer = tempoDebuff; // Aplica a lentidão para simular atordoamento na corrente.
    }

    public float GetDirecaoOlhar() { return moveDirection; } // Devolve para a arma pra qual lado atirar.

    private void OnCollisionStay2D(Collision2D collision) { if (collision.contacts[0].normal.y > 0.5f) isGrounded = true; } // Garante que piso inclinado muito alto não dê pulo infinito.
    private void OnCollisionExit2D(Collision2D collision) { isGrounded = false; } // Avisa a IA que ela está caindo ou no ar.
}