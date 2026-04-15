using System.Collections; // Importa a biblioteca para podermos usar Corrotinas (atrasos controlados).
using System.Collections.Generic; // Importa a biblioteca essencial para usarmos Listas e o poderoso Dictionary.
using TMPro; // Importa a biblioteca para desenhar textos com alta qualidade na UI.
using UnityEngine; // Importa as ferramentas fundamentais do motor da Unity.
using UnityEngine.UI; // Importa os componentes de interface como Imagens e Botões.

public class TabletUIManager : MonoBehaviour // É o seu "Menu de Pause" estilo Pip-Boy, mostrando tudo o que você tem na mochila.
{
    // Padrão Singleton: Garante que exista apenas um TabletUIManager no jogo para fácil acesso.
    public static TabletUIManager Instance { get; private set; }

    [Header("Painéis Principais")]
    public GameObject painelTablet; // A gaveta que liga/desliga o painel mestre inteiro do tablet na tela.
    public Transform gridDeItens;   // A pasta (grid) onde os botões dos itens vão ser criados visualmente.
    public GameObject prefabSlotItem; // O botão de item modelo (Prefab) que será clonado para cada peça.

    [Header("Área de Detalhes (Direita)")]
    public Image iconeDetalhe; // Gaveta para a imagem grande da peça selecionada.
    public TextMeshProUGUI textoNomeDetalhe; // Gaveta para o nome da peça selecionada.
    public TextMeshProUGUI textoDescricaoDetalhe; // Gaveta para a ficha técnica/descrição da peça.

    private void Awake() // Roda no exato momento em que o jogo inicia.
    {
        // Configuração básica do Singleton.
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Garante que o jogo comece com o tablet desligado/invisível.
        painelTablet.SetActive(false);
        LimparDetalhes(); // Zera as informações da tela da direita.
    }

    private void Update() // Roda a cada frame (atuando como os "ouvidos" do jogo para o teclado).
    {
        // Quando você aperta TAB...
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.tabKey.wasPressedThisFrame)
        {
            // ...ele funciona como um interruptor: se o tablet já estiver aberto, ele fecha. Senão, ele abre.
            if (painelTablet.activeSelf) FecharTablet();
            else AbrirTablet();
        }
    }

    public void AbrirTablet()
    {
        painelTablet.SetActive(true); // Ativa o Canvas do Tablet na tela.
        ConstruirGridDeItens(); // Chama a função que lê a mochila e cria os botões.
        LimparDetalhes(); // Começa com o lado direito vazio para o jogador escolher em que clicar.

        // Pausa o motor de física do jogo mudando o Time.timeScale para 0f (típico Menu de Pause de RPGs).
        Time.timeScale = 0f;
    }

    public void FecharTablet()
    {
        painelTablet.SetActive(false); // Desativa o Canvas do Tablet na tela.
        Time.timeScale = 1f; // Despausa o jogo, devolvendo a física ao tempo normal (1x).
    }

    private void ConstruirGridDeItens() // Onde a mágica visual e matemática de listagem acontece.
    {
        // 1. Destrói os botões antigos da tela para que a lista seja gerada limpa e atualizada.
        foreach (Transform filho in gridDeItens)
        {
            Destroy(filho.gameObject);
        }

        // Puxa a lista bruta com TODAS as peças que estão salvas no seu Inventário (Singleton).
        List<PecaSpeedBot> mochila = InventarioManager.Instance.pecasGuardadas;

        // 2. O AGRUPAMENTO INTELIGENTE: Impede que a tela fique poluída com vários ícones idênticos.
        // Ele usa um Dictionary que liga o Item (PecaSpeedBot) a um número inteiro (Quantidade).
        Dictionary<PecaSpeedBot, int> contagemPecas = new Dictionary<PecaSpeedBot, int>();

        foreach (PecaSpeedBot peca in mochila) // Ele olha a sua mochila, peça por peça...
        {
            // Se a peça já existir no dicionário, ele apenas soma +1 na quantidade.
            if (contagemPecas.ContainsKey(peca)) contagemPecas[peca]++;
            // Se for uma peça nova que ele ainda não contou, ele adiciona no dicionário com a quantidade 1.
            else contagemPecas[peca] = 1;
        }

        // 3. Criação Visual: Agora, em vez de criar um botão por peça, cria um botão para cada TIPO de peça no Dicionário!
        foreach (var kvp in contagemPecas)
        {
            PecaSpeedBot peca = kvp.Key; // Puxa a peça do Dicionário.
            int quantidade = kvp.Value; // Puxa o total acumulado daquela peça.

            // "Instancia" (cria um clone) do Prefab do botão lá dentro da pasta do Grid na tela.
            GameObject novoSlot = Instantiate(prefabSlotItem, gridDeItens);

            // Procura a imagem dentro do botão criado e coloca o Ícone correto da peça.
            Image iconeSlot = novoSlot.transform.Find("Image").GetComponent<Image>();
            if (iconeSlot != null && peca.icone != null)
            {
                iconeSlot.sprite = peca.icone;
            }

            // Coloca a Quantidade (Procura a gavetinha de texto "Qtd" no Prefab).
            Transform objQtd = novoSlot.transform.Find("Qtd");
            if (objQtd != null)
            {
                TextMeshProUGUI textoQtd = objQtd.GetComponent<TextMeshProUGUI>();
                if (textoQtd != null)
                {
                    // Estética refinada: Se tiver só 1 item, o texto fica vazio. Se for 2 ou mais, mostra o número (ex: "3").
                    textoQtd.text = quantidade > 1 ? quantidade.ToString() : "";
                }
            }

            // Diz pro botão: "Quando você for clicado, chame a função de Mostrar Detalhes passando esta peça como parâmetro!"
            Button botaoSlot = novoSlot.GetComponent<Button>();
            botaoSlot.onClick.AddListener(() => MostrarDetalhesDoItem(peca));
        }

        // Dá um "empurrão" na Unity para organizar o grid direitinho (para os ícones não nascerem encavalados).
        StartCoroutine(ArrumarGridDelay());
    }

    private IEnumerator ArrumarGridDelay()
    {
        // Espera o exato fim do frame atual para dar tempo da Unity terminar de instanciar todos os botões fisicamente.
        yield return new WaitForEndOfFrame();

        // Força a atualização do layout/grid de forma bruta, alinhando todos os ícones perfeitamente.
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridDeItens.GetComponent<RectTransform>());
    }

    // Método chamado quando você clica em um item da mochila.
    public void MostrarDetalhesDoItem(PecaSpeedBot peca)
    {
        iconeDetalhe.gameObject.SetActive(true); // Acende o quadro da foto à direita.
        iconeDetalhe.sprite = peca.icone; // Joga a foto do item no quadro.
        textoNomeDetalhe.text = peca.nomeDaPeca; // Escreve o nome da peça.

        // Usa formatação de texto rico (Rich Text) para criar uma ficha técnica bonita na direita, pulando duas linhas (\n\n) após a descrição base.
        string descricaoFormatada = peca.descricao + "\n\n";

        // Puxa os atributos originais do seu RPG (Velocidade, Durabilidade, etc.) dependendo de qual categoria de peça foi clicada:
        if (peca.tipoPeca == TipoPeca.Chassi) // Se for um corpo (Crawler, Slider, Aerial)...
        {
            // Injeta a Classe com cor Azul, depois a Arrancada e a Durabilidade base convertidas para porcentagem (* 100).
            descricaoFormatada += $"<color=#AADDFF>Classe:</color> {peca.classe}\n";
            descricaoFormatada += $"<color=#AADDFF>Arrancada:</color> {peca.arrancadaBase * 100}%\n";
            descricaoFormatada += $"<color=#AADDFF>Durabilidade:</color> {peca.durabilidadeBase * 100}%";
        }
        else if (peca.tipoPeca == TipoPeca.Motor) // Se for um motor...
        {
            // Injeta a Velocidade e Aceleração com cor Vermelha.
            descricaoFormatada += $"<color=#FFAAAA>Velocidade Máx:</color> {peca.velocidadeMaxima}\n";
            descricaoFormatada += $"<color=#FFAAAA>Aceleração:</color> {peca.aceleracao}";
        }
        else if (peca.tipoPeca == TipoPeca.Modulo) // Se for um item especial (Armadilha, Gancho, Nitro)...
        {
            // Injeta o nome da Habilidade com cor Verde.
            descricaoFormatada += $"<color=#AAFFAA>Habilidade:</color> {peca.habilidadeEspecial}";
        }

        // Despacha toda essa frase maluca (com cores e atributos formatados) para o texto final da tela.
        textoDescricaoDetalhe.text = descricaoFormatada;
    }

    private void LimparDetalhes() // Desliga a interface da direita para quando o menu é aberto vazio.
    {
        iconeDetalhe.gameObject.SetActive(false); // Apaga a imagem grande.
        textoNomeDetalhe.text = "Selecione um item"; // Texto placeholder.
        textoDescricaoDetalhe.text = ""; // Zera a ficha técnica.
    }
}