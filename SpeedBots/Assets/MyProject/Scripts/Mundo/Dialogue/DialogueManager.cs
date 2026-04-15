using UnityEngine; // Importa as ferramentas da engine da Unity.
using TMPro; // Importa a biblioteca TextMeshPro para exibirmos textos com alta qualidade.
using UnityEngine.UI; // Importa os componentes de Interface de Usuário (como Imagens e Botões).
using System.Collections.Generic; // Importa a biblioteca necessária para manipularmos Listas.
using UnityEngine.SceneManagement; // Importa o gerenciador para podermos mudar de cena (teletransporte) ao fim da conversa.

[System.Serializable] // Permite que a Unity exiba essa classe no Inspector para você cadastrar os personagens.
public class PerfilPersonagem // É o "documento" do personagem para o Sistema de Fotos.
{
    public string nome; // O nome do personagem (ex: "Mia").
    public Sprite foto; // A imagem/rosto que aparecerá do lado da fala.
}

public class DialogueManager : MonoBehaviour // É o ator que sobe no palco para ler o roteiro criado pelo LeitorTwine.
{
    // --- INSTÂNCIA BLINDADA (Singleton) ---
    private static DialogueManager _instance; // Variável secreta que guarda o ator único.
    public static DialogueManager Instance // Garante que o jogo inteiro acesse esse gerenciador e que só exista UM no palco.
    {
        get
        {
            if (_instance == null) _instance = FindFirstObjectByType<DialogueManager>();
            return _instance;
        }
    }

    [Header("UI do Diálogo")]
    public GameObject painelDialogo; // O painel principal (a caixa de texto toda) que aparece na tela.
    public TextMeshProUGUI textoNome; // A gaveta onde vai aparecer o nome de quem fala.
    public TextMeshProUGUI textoPrincipal; // A gaveta onde a fala real será exibida.
    public Image fotoPersonagem; // O quadro de imagem onde a foto (Sprite) do personagem vai ser colada.

    [Header("Botões")]
    public Transform areaDosBotoes; // A pasta (grid) onde os botões de escolhas vão ser criados.
    public GameObject prefabBotao; // O molde visual do botão que será clonado.

    [Header("Banco de Personagens")]
    public List<PerfilPersonagem> perfis; // A lista contendo as fotos e nomes de todos os atores cadastrados no jogo.

    private NoTwine noAtualAtivo; // Guarda a "página" ou "nó" do roteiro que está sendo lido agora.
    private int indiceDaFala = 0; // O marcador de linha que diz qual frase da página ele deve ler no momento.
    private string cenaAoEncerrarAtual = ""; // Guarda o nome da fase para qual o jogador deve ser mandado ao fim da conversa (se houver).

    void Awake() // Roda no exato momento em que o jogo inicia.
    {
        // Proteção do Singleton: se já existir outro DialogueManager carregado, este se destrói para não duplicar.
        if (_instance != null && _instance != this)
        {
            Destroy(_instance.gameObject);
        }
        _instance = this; // Se não existir, ele assume o papel principal.

        // Garante que o menu comece desligado (invisível) para não ficar travando a tela.
        if (painelDialogo != null) painelDialogo.SetActive(false);
    }

    public bool isTalking // Propriedade rápida para outros scripts (como o do jogador) saberem se a conversa está rolando.
    {
        get
        {
            if (painelDialogo == null) return false;
            return painelDialogo.activeInHierarchy; // Se a tela de diálogo estiver ligada, é porque estão conversando.
        }
    }

    // Chamado pelo NPC ou qualquer evento que queira abrir uma conversa.
    public void IniciarDialogo(string tituloDoNo, string cenaDestino = "")
    {
        cenaAoEncerrarAtual = cenaDestino; // Salva o nome do teletransporte (se houver).
        ExibirNo(tituloDoNo); // Manda o ator abrir a página certa do roteiro.
    }

    public void ExibirNo(string tituloDoNo) // Busca a página no LeitorTwine e prepara a leitura.
    {
        if (painelDialogo == null) return; // Trava de segurança.

        painelDialogo.SetActive(true); // Acende as luzes! Mostra a caixa de diálogo para o jogador.

        // Pergunta ao LeitorTwine se essa história/página realmente existe na memória.
        if (!LeitorTwine.Instance.historia.ContainsKey(tituloDoNo)) return;

        // Ele recebe o nó da história atual do LeitorTwine e guarda na memória.
        noAtualAtivo = LeitorTwine.Instance.historia[tituloDoNo];
        indiceDaFala = 0; // Zera o marcador de linha para começar a ler a página desde a primeira frase.

        MostrarFalaNaTela(); // Manda desenhar a fala atual na tela.
    }

    private void MostrarFalaNaTela() // O núcleo principal: atualiza o texto, o Sistema de Fotos e gera as Escolhas.
    {
        if (noAtualAtivo == null || noAtualAtivo.falas.Count == 0) return; // Prevenção contra páginas vazias.

        LinhaDeFala fala = noAtualAtivo.falas[indiceDaFala]; // Pega a frase exata baseada no número do índice atual.
        textoPrincipal.text = fala.texto; // Escreve o texto da frase na tela.

        // --- SISTEMA DE FOTOS ---
        // Checa no roteiro se o texto diz que quem está falando é o "Narrador".
        if (fala.nome == "Narrador")
        {
            if (textoNome != null) textoNome.text = ""; // Apaga o nome de quem fala.
            if (fotoPersonagem != null) fotoPersonagem.gameObject.SetActive(false); // Ele desliga a foto (o narrador não tem rosto).
        }
        else // Se for um personagem real conversando...
        {
            if (textoNome != null) textoNome.text = fala.nome; // Escreve o nome do personagem na tela.
            if (fotoPersonagem != null)
            {
                Sprite fotoEncontrada = null; // Prepara uma gaveta vazia.

                // Ele vasculha o banco de personagens procurando a foto correspondente a esse nome (ex: procura a "Mia").
                foreach (var perfil in perfis)
                {
                    if (fala.nome.Contains(perfil.nome)) // Se achar um perfil com nome igual...
                    {
                        fotoEncontrada = perfil.foto; // Guarda a foto certa.
                        break; // Para de procurar.
                    }
                }

                if (fotoEncontrada != null) // Se achou a imagem da "Mia"...
                {
                    fotoPersonagem.sprite = fotoEncontrada; // Ele liga a foto na tela do lado da fala!
                    fotoPersonagem.gameObject.SetActive(true);
                }
                else fotoPersonagem.gameObject.SetActive(false); // Se não cadastrou foto, deixa desligado.
            }
        }

        // Limpa a área dos botões (destrói as respostas da página anterior para não acumular sujeira na tela).
        foreach (Transform filho in areaDosBotoes) Destroy(filho.gameObject);

        // --- AS ESCOLHAS ---
        // Verifica se a frase atual que acabou de ler é a ÚLTIMA fala deste nó.
        if (indiceDaFala >= noAtualAtivo.falas.Count - 1)
        {
            // Chegou na última fala. Ele olha o roteiro: Tem respostas cadastradas para tomar uma decisão?
            foreach (var resposta in noAtualAtivo.respostas)
            {
                // Se sim, ele "instancia" (clona) um botão visual para cada resposta possível na história.
                GameObject novoBotao = Instantiate(prefabBotao, areaDosBotoes);
                novoBotao.GetComponentInChildren<TextMeshProUGUI>().text = resposta.Key; // O texto que o jogador clica (ex: "Entrar na casa").

                string destino = resposta.Value; // O nome da página que esse botão deve abrir.

                // Já programando o clique do botão para pular para a página certa da história.
                novoBotao.GetComponent<Button>().onClick.AddListener(() => { ExibirNo(destino); });
            }

            // Se NÃO tiver respostas e a página apenas acabar (noAtualAtivo.respostas.Count == 0)...
            if (noAtualAtivo.respostas.Count == 0)
            {
                GameObject botaoSair = Instantiate(prefabBotao, areaDosBotoes); // Cria um botão neutro.
                botaoSair.GetComponentInChildren<TextMeshProUGUI>().text = "Encerrar"; // Ele cria o botão de "Encerrar".

                // Configura o que acontece ao clicar em "Encerrar":
                botaoSair.GetComponent<Button>().onClick.AddListener(() => {

                    painelDialogo.SetActive(false); // Desliga o painel de diálogo, devolvendo o controle pro jogador.

                    // Teletransporte direto! Pode até te teletransportar para outra Cena (SceneManager.LoadScene), se for necessário.
                    if (!string.IsNullOrEmpty(cenaAoEncerrarAtual))
                    {
                        Time.timeScale = 1; // Garante que o jogo não viaje pausado.
                        SceneManager.LoadScene(cenaAoEncerrarAtual); // Carrega a nova fase.
                    }
                });
            }
        }
        else // Se ainda NÃO é a última fala e tem mais texto nessa mesma página...
        {
            GameObject btnContinuar = Instantiate(prefabBotao, areaDosBotoes); // Cria um botão normal.
            btnContinuar.GetComponentInChildren<TextMeshProUGUI>().text = "Continuar ▼"; // Coloca a seta para baixo indicando mais texto.

            // Ao clicar, chama a função de avançar apenas uma frase na mesma página.
            btnContinuar.GetComponent<Button>().onClick.AddListener(() => { AvancarDialogo(); });
        }
    }

    public void AvancarDialogo() // Chamado quando o botão "Continuar" é apertado ou quando o jogador usa a tecla "E" (via PlayerInteractor).
    {
        if (noAtualAtivo == null) return; // Trava de segurança.

        // Ele vai avançando o indiceDaFala a cada clique.
        if (indiceDaFala < noAtualAtivo.falas.Count - 1)
        {
            indiceDaFala++; // Pula para a próxima linha da página atual (+1).
            MostrarFalaNaTela(); // Refaz toda a lógica de atualizar texto e foto.
        }
        else // Se a página já acabou (mas o jogador apertou o teclado em cima dos botões de escolha)...
        {
            if (areaDosBotoes.childCount == 1) // Confere se tem só UM botão na tela.
            {
                Button botaoUnico = areaDosBotoes.GetChild(0).GetComponent<Button>();
                if (botaoUnico != null)
                {
                    // --- TRAVA ANTI-STACKOVERFLOW: O sistema não clica sozinho se o botão for "Continuar" ---
                    string textoBotao = botaoUnico.GetComponentInChildren<TextMeshProUGUI>().text;
                    if (!textoBotao.Contains("Continuar"))
                    {
                        botaoUnico.onClick.Invoke(); // Força um clique via código (útil para encerrar diálogo pelo teclado sem usar o mouse).
                    }
                }
            }
        }
    }

    // Função auxiliar simples para o sistema jogar mensagens na tela que não dependem do Twine (Ex: Porta trancada).
    public void ExibirMensagemRapida(string mensagem)
    {
        if (painelDialogo == null) return;
        painelDialogo.SetActive(true); // Exibe o painel.
        if (textoNome != null) textoNome.text = ""; // Zera a caixa de nome do personagem.
        textoPrincipal.text = mensagem; // Escreve o texto livre.
        if (fotoPersonagem != null) fotoPersonagem.gameObject.SetActive(false); // Desliga foto se houver.

        foreach (Transform filho in areaDosBotoes) Destroy(filho.gameObject); // Apaga as escolhas velhas.
        GameObject btn = Instantiate(prefabBotao, areaDosBotoes); // Gera um botão.
        btn.GetComponentInChildren<TextMeshProUGUI>().text = "Fechar"; // Chama de Fechar.
        btn.GetComponent<Button>().onClick.AddListener(() => { painelDialogo.SetActive(false); }); // Desliga tudo ao clicar.
    }
}