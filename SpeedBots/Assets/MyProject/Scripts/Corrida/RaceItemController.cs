using UnityEngine; // Importa as ferramentas essenciais da Unity.
using UnityEngine.InputSystem; // Importa o novo sistema de Inputs da Unity para ler o teclado.

public class RaceItemController : MonoBehaviour // Cria a classe que gerencia o inventário e atua como o "dedo no gatilho" do arsenal.
{
    public enum TipoItem { Nenhum, Nitro, Armadilha, Gancho } // Cria uma lista que diz quais itens (armas) existem no jogo.

    [Header("Inventário")] // Organiza a visualização do painel no Inspector.
    public TipoItem itemGuardado = TipoItem.Nenhum; // A gaveta do inventário: guarda o item que foi pego. Começa vazia.
    public GameObject prefabArmadilha; // Gaveta onde você arrasta o objeto físico da bolinha de Stun lá na Unity.

    [Header("Configuração")] // Organiza as opções do script.
    public bool ehJogador = true; // Chave mestre que diz se quem está usando é o Player (true) ou a Inteligência Artificial (false).

    private SpeedBotMovment motorPlayer; // Espaço reservado para o "motor" do Jogador.
    private SpeedBotIA motorIA; // Espaço reservado para o "motor" da IA.
    private float tempoParaIAUsarItem = 0f; // Cronômetro regressivo usado pela IA para não usar os itens rápido demais.

    void Awake() // Função acionada assim que o robô nasce no jogo.
    {
        motorPlayer = GetComponent<SpeedBotMovment>(); // Tenta capturar o motor de movimento do Jogador.
        motorIA = GetComponent<SpeedBotIA>(); // Tenta capturar o motor de movimento da IA.
    }

    void Update() // Função "Tomada de Decisão" que roda a cada frame do jogo.
    {
        if (ehJogador) // Se o dono desse inventário for o Jogador...
        {
            // Fica escutando o teclado para ver se a tecla Shift foi apertada neste exato momento.
            if (Keyboard.current != null && Keyboard.current.shiftKey.wasPressedThisFrame)
            {
                UsarItem(); // Se o botão foi apertado, chama a função de puxar o gatilho!
            }
        }
        else // Se o dono desse inventário for a Inteligência Artificial...
        {
            if (itemGuardado != TipoItem.Nenhum) // Confere se ela não está de mãos vazias.
            {
                // INTELIGÊNCIA ARTIFICIAL TRAPACEIRA: Se a arma for o Gancho, ela age como um franco-atirador.
                if (itemGuardado == TipoItem.Gancho)
                {
                    float direcao = motorIA.GetDirecaoOlhar(); // Pega a direção que a IA está olhando.
                    CapsuleCollider2D col = GetComponent<CapsuleCollider2D>(); // Acha o corpo dela para não atirar em si mesma.
                    float offset = (col != null) ? col.bounds.extents.x + 0.5f : 1.0f; // Dá uma distância de segurança.
                    Vector2 origem = new Vector2(transform.position.x + (direcao * offset), transform.position.y); // Ponto de onde o laser vai sair.

                    // A IA liga o "Raycast2D", um laser invisível atirando para frente a todo instante procurando uma vítima.
                    RaycastHit2D hit = Physics2D.Raycast(origem, new Vector2(direcao, 0), 15f);

                    // Se esse laser invisível encostar no Player...
                    if (hit.collider != null && hit.collider.CompareTag("Player"))
                    {
                        UsarItem(); // ...Ela puxa o gatilho do Gancho instantaneamente!
                    }
                }
                else // Mas se for um item comum (Nitro ou Armadilha)...
                {
                    // Ela não é vidente, então ela liga o cronômetro para parecer mais natural.
                    tempoParaIAUsarItem -= Time.deltaTime;
                    if (tempoParaIAUsarItem <= 0) UsarItem(); // Quando a contagem regressiva atinge zero, ela usa o item!
                }
            }
        }
    }

    public void PegarItem(TipoItem novoItem) // Função ativada quando o robô bate na Caixa Misteriosa na pista.
    {
        if (itemGuardado == TipoItem.Nenhum) // Checa se o inventário está vazio (não subscreve um item que já está lá).
        {
            itemGuardado = novoItem; // Guarda o item recém-sorteado.
            // Se for a IA pegando o item, ela já rola os dados (entre 1 a 3 segundos) para decidir quando vai usar.
            if (!ehJogador) tempoParaIAUsarItem = Random.Range(1f, 3f);
        }
    }

    public void UsarItem() // A execução do Ataque/Efeito de fato.
    {
        if (itemGuardado == TipoItem.Nenhum) return; // Trava de segurança: não faz nada se o bolso estiver vazio.

        // Pergunta para qual lado o atirador está olhando (lê o motor do player ou da IA).
        float direcao = ehJogador ? motorPlayer.GetDirecaoOlhar() : motorIA.GetDirecaoOlhar();

        switch (itemGuardado) // O "switch" vai ler qual é a arma guardada e decidir o que fazer.
        {
            case TipoItem.Nitro: // Se ativou um Nitro...
                if (ehJogador) motorPlayer.AtivarNitro(1.8f, 1.5f); // Chama o motor do jogador para acelerar.
                else motorIA.AtivarNitro(1.8f, 1.5f); // Chama o motor da IA para acelerar.
                break; // Finaliza o fluxo do Nitro.

            case TipoItem.Armadilha: // Se usou uma Armadilha...
                // Calcula a posição (posArmadilha) atrás das costas de quem jogou usando matemática vetorial.
                Vector2 posArmadilha = new Vector2(transform.position.x - (direcao * 1.5f), transform.position.y);
                // Usa Instantiate para "dar à luz" à bolinha de choque (prefab) lá na pista, e ela fica quieta lá.
                Instantiate(prefabArmadilha, posArmadilha, Quaternion.identity);
                break; // Finaliza o fluxo da Armadilha.

            case TipoItem.Gancho: // Se disparou o Gancho...
                AtirarGancho(direcao); // Chama a rotina dedicada de tiro e mira logo abaixo.
                break; // Finaliza o fluxo do Gancho.
        }

        itemGuardado = TipoItem.Nenhum; // Por fim, a arma some e o inventário volta a ficar vazio.
    }

    private void AtirarGancho(float direcao) // A mecânica real do disparo do Gancho.
    {
        CapsuleCollider2D col = GetComponent<CapsuleCollider2D>(); // Acha o próprio corpo físico.
        float offset = (col != null) ? col.bounds.extents.x + 0.5f : 1.0f; // Compensa a largura do corpo para não atirar em si.

        Vector2 origem = new Vector2(transform.position.x + (direcao * offset), transform.position.y); // O ponto de onde o gancho sai.
        float alcance = 15f; // O cabo do gancho tem um alcance máximo de 15 metros na Unity.

        // Atira outro Raycast (laser) da ponta da arma para ver se acerta alguém.
        RaycastHit2D hit = Physics2D.Raycast(origem, new Vector2(direcao, 0), alcance);
        // Desenha uma linha de teste na aba Scene (só para você ver se o código está calculando certo).
        Debug.DrawRay(origem, new Vector2(direcao * alcance, 0), Color.magenta, 2f);

        if (hit.collider != null) // Se a corda bater em alguém...
        {
            SpeedBotMovment vitimaPlayer = hit.collider.GetComponent<SpeedBotMovment>(); // Pega o motor do jogador caso ele seja a vítima.
            SpeedBotIA vitimaIA = hit.collider.GetComponent<SpeedBotIA>(); // Pega o motor da IA caso ela seja a vítima.

            // Se o Player foi atingido e o atirador NÃO for o jogador (ou seja, foi a IA que te pescou)...
            if (vitimaPlayer != null && !ehJogador)
            {
                // Manda o Player sofrer um tranco (direção invertida para ser puxado para trás) e um Stun de 1.5s no motor dele.
                vitimaPlayer.SofrerPuxao(20f, -direcao, 1.5f);
            }
            // Se a Inteligência Artificial foi atingida e o atirador for o jogador...
            else if (vitimaIA != null && ehJogador)
            {
                // A IA sofre o Puxão para trás e o Stun temporário.
                vitimaIA.SofrerPuxao(20f, -direcao, 1.5f);
                // Um detalhe de game design: o jogador ganha um leve "Nitro" para chegar perto de quem ele pescou.
                if (motorPlayer != null) motorPlayer.AtivarNitro(1.3f, 1f);
            }
        }
    }
}