using UnityEngine; // Importa as ferramentas da engine da Unity.
using UnityEngine.UI; // Importa as ferramentas básicas de Interface de Usuário (Imagens, Botões).
using TMPro; // Importa as ferramentas de Texto nítido (TextMeshPro).
using System.Collections.Generic; // Importa o sistema de Listas (List<>).

public class CraftingUIManager : MonoBehaviour // É o cérebro complexo e visual da oficina.
{
    // Singleton: Garante que só exista UM gerenciador de Crafting no jogo e permite que outros scripts falem com ele facilmente.
    public static CraftingUIManager Instance { get; private set; }

    [Header("Configuração Geral")]
    public GameObject painelCrafting; // O painel principal (a tela inteira) do menu de montagem.
    public List<ReceitaCraft> bancoDeReceitas; // A biblioteca completa que lê os arquivos de receita do jogo.

    [Header("Área Esquerda (Lista de Peças)")]
    public TextMeshProUGUI tituloCategoriaEsquerda; // O Texto do Título da Categoria selecionada.
    public Transform gridListaReceitas; // A pasta (grid) onde os botões das peças serão criados.
    public GameObject prefabBotaoReceita; // O botão "molde" que será clonado na tela.

    [Header("Área Direita (Painel Inteiro)")]
    public GameObject painelDetalhesDireita; // O painel que exibe os detalhes da peça selecionada.

    [Header("Área Direita (Detalhes)")]
    public Image iconeDetalhe; // O ícone da peça selecionada na direita.
    public TextMeshProUGUI textoNomeDetalhe; // O nome da peça selecionada.
    public TextMeshProUGUI textoDescricaoDetalhe; // A descrição da peça selecionada.
    public Button botaoFabricar; // O botão que o jogador clica para criar o item.

    [Header("Área Direita (Grid Dinâmico)")]
    public Transform gridIngredientesDinamico; // A pasta (grid) onde aparecerão os ingredientes necessários.
    public GameObject prefabSlotIngrediente; // O molde visual do ingrediente exigido.

    private ReceitaCraft receitaSelecionada; // Variável invisível que guarda qual receita o jogador clicou por último.

    // Guarda o estado atual dos filtros para podermos atualizar a tela automaticamente depois de fabricar sem o jogador se perder.
    private TipoPeca filtroAtual;
    private string tituloAtual;

    void Awake() // Roda ao iniciar a fase.
    {
        // Configuração do Singleton.
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        painelCrafting.SetActive(false); // Garante que a oficina comece desligada/fechada.
    }

    public void AbrirBancada() // Chamado pelo objeto físico (BancadaDeMontagem) no mundo 2D.
    {
        painelCrafting.SetActive(true); // Liga a tela da oficina.
        LimparDetalhes(); // Esconde as informações antigas da direita.
        BotaoFiltroChassi(); // Por padrão, sempre abre exibindo a aba de "Chassis".
    }

    public void FecharBancada() { painelCrafting.SetActive(false); } // Desliga a tela da oficina.

    // --- OS BOTÕES DO TOPO (Filtros) ---
    // Quando você clica num filtro, ele chama a Listagem passando o tipo de peça e o nome da aba.
    public void BotaoFiltroChassi() { FiltrarReceitas(TipoPeca.Chassi, "Chassis"); }
    public void BotaoFiltroMotor() { FiltrarReceitas(TipoPeca.Motor, "Motores"); }
    public void BotaoFiltroModulo() { FiltrarReceitas(TipoPeca.Modulo, "Módulos"); }

    // --- LISTAGEM DE ITENS ---
    private void FiltrarReceitas(TipoPeca tipoFiltro, string tituloFiltro)
    {
        filtroAtual = tipoFiltro; // Memoriza qual categoria o jogador está olhando.
        tituloAtual = tituloFiltro; // Memoriza o texto do título.

        // Atualiza o texto do título acima da lista da esquerda.
        if (tituloCategoriaEsquerda != null) tituloCategoriaEsquerda.text = tituloFiltro;

        // Apaga todos os botões antigos da tela para não misturar com os novos.
        foreach (Transform filho in gridListaReceitas) Destroy(filho.gameObject);
        if (bancoDeReceitas == null) return;

        bool selecionouPrimeiro = false;

        // Varre o banco de receitas procurando apenas as peças da aba certa (ex: "Motores").
        foreach (ReceitaCraft receita in bancoDeReceitas)
        {
            if (receita == null || receita.pecaResultado == null) continue;

            if (receita.pecaResultado.tipoPeca == tipoFiltro) // Se for o tipo que o jogador pediu...
            {
                // "Instancia" (cria um clone) do prefabBotaoReceita lá dentro do grid da tela.
                GameObject novoBotao = Instantiate(prefabBotaoReceita, gridListaReceitas);

                // Procura as gavetinhas de texto, imagem e quantidade dentro desse clone recém-criado.
                Transform objTexto = novoBotao.transform.Find("Text_Nome");
                Transform objIcone = novoBotao.transform.Find("Image_Icone");
                Transform objQtd = novoBotao.transform.Find("Image_Icone/Qtd"); // O número de quantidade fica grudado no ícone.

                // Preenche o texto com o nome da peça e o ícone com a imagem da peça.
                if (objTexto != null) objTexto.GetComponent<TextMeshProUGUI>().text = receita.pecaResultado.nomeDaPeca;
                if (objIcone != null && receita.pecaResultado.icone != null) objIcone.GetComponent<Image>().sprite = receita.pecaResultado.icone;

                // Preenche o número no cantinho do botão.
                if (objQtd != null)
                {
                    // Chama "A Matemática Inteligente" para descobrir quantos desse item eu posso fazer agora.
                    int maxCraftavel = CalcularQuantidadeCraftavel(receita);
                    TextMeshProUGUI textoQtd = objQtd.GetComponent<TextMeshProUGUI>();

                    textoQtd.text = maxCraftavel.ToString(); // Escreve o número na tela.

                    // Feedback visual: Fica branco normal se der pra fazer, mas fica vermelho/cinza se não der pra fazer nenhum.
                    textoQtd.color = maxCraftavel > 0 ? Color.white : new Color(1f, 0.5f, 0.5f);
                }

                // Programa o botão: quando ele for clicado, vai chamar a função de exibir os detalhes daquela receita específica na direita.
                ReceitaCraft receitaAtual = receita;
                novoBotao.GetComponent<Button>().onClick.AddListener(() => SelecionarReceita(receitaAtual));
            }
        }

        LimparDetalhes(); // Esconde o painel da direita até o jogador clicar em um dos botões recém-criados.
    }

    // --- A MATEMÁTICA INTELIGENTE ---
    private int CalcularQuantidadeCraftavel(ReceitaCraft receita)
    {
        if (receita.ingredientes.Count == 0) return 0; // Prevenção de erro: receita vazia não fabrica nada.

        int maxCraftavel = 999; // Começa absurdamente alto e vai abaixando conforme acha o gargalo.

        // Compara a exigência da receita com a mochila do jogador.
        foreach (Ingrediente ing in receita.ingredientes)
        {
            int tenho = ContarNoInventario(ing.recursoNecessario); // Conta quanto material desse eu tenho.

            // Divisão inteira. Ex: Tenho 5 sucatas na mochila. A receita pede 2. 5 / 2 = Posso fazer 2.
            int possoFazerDesteIngrediente = tenho / ing.quantidadeNecessaria;

            // Se esse ingrediente render menos construções que o anterior, ele se torna o novo gargalo de produção.
            // Ex: Tenho pão pra 10 hambúrgueres, mas só carne pra 3. O limite (gargalo) é 3!
            if (possoFazerDesteIngrediente < maxCraftavel)
            {
                maxCraftavel = possoFazerDesteIngrediente;
            }
        }

        return maxCraftavel; // Devolve o número máximo absoluto que você pode construir dessa peça.
    }

    // --- EXIBIÇÃO DE DETALHES ---
    private void SelecionarReceita(ReceitaCraft receita) // Executado ao clicar no botão na lista.
    {
        receitaSelecionada = receita; // Salva a receita como a atual.
        if (painelDetalhesDireita != null) painelDetalhesDireita.SetActive(true); // Liga o painel da direita.

        // Preenche o ícone, nome e descrição principais.
        if (receita.pecaResultado.icone != null) iconeDetalhe.sprite = receita.pecaResultado.icone;
        textoNomeDetalhe.text = receita.pecaResultado.nomeDaPeca;
        textoDescricaoDetalhe.text = receita.pecaResultado.descricao;

        // Apaga os ingredientes antigos do painel de detalhes.
        foreach (Transform filho in gridIngredientesDinamico) Destroy(filho.gameObject);

        bool podeFabricar = true; // Assume que dá pra fazer, até descobrir que falta algo.

        // Monta a lista visual de ingredientes necessários para essa receita.
        foreach (Ingrediente ing in receita.ingredientes)
        {
            if (ing == null || ing.recursoNecessario == null) continue;

            // Cria um bloquinho visual novo pra cada exigência.
            GameObject novoSlot = Instantiate(prefabSlotIngrediente, gridIngredientesDinamico);
            int tenhoNaMochila = ContarNoInventario(ing.recursoNecessario); // Olha a mochila de novo.

            // Acha as referências de texto/imagem dentro do bloquinho visual recém-criado.
            Transform objIcone = novoSlot.transform.Find("Image_Icone");
            Transform objNome = novoSlot.transform.Find("Text_Nome");
            Transform objDesc = novoSlot.transform.Find("Text_Descricao");
            Transform objQtd = novoSlot.transform.Find("Text_Qtd");

            // Preenche os dados visuais do bloquinho.
            if (ing.recursoNecessario.icone != null) objIcone.GetComponent<Image>().sprite = ing.recursoNecessario.icone;
            objNome.GetComponent<TextMeshProUGUI>().text = ing.recursoNecessario.nomeDaPeca;
            objDesc.GetComponent<TextMeshProUGUI>().text = ing.recursoNecessario.descricao;

            // Escreve a quantidade no formato (Tenho / Exigido). Ex: "15 / 5".
            TextMeshProUGUI textoQtd = objQtd.GetComponent<TextMeshProUGUI>();
            textoQtd.text = $"{tenhoNaMochila} / {ing.quantidadeNecessaria}";

            // Checagem visual final: Se eu tiver menos do que a receita pede...
            if (tenhoNaMochila < ing.quantidadeNecessaria)
            {
                textoQtd.color = Color.red; // Pinta o número de vermelho para eu saber o que tá faltando.
                podeFabricar = false; // Trava o botão de fabricar!
            }
            else { textoQtd.color = Color.black; } // Se tiver o suficiente, pinta de preto (ou a cor padrão).
        }

        // Liga ou desliga o botão de fabricar dependendo se o jogador tem ou não todos os recursos.
        botaoFabricar.interactable = podeFabricar;
    }

    // Função de auxílio que varre a mochila e soma os itens repetidos.
    private int ContarNoInventario(PecaSpeedBot pecaBuscada)
    {
        int total = 0;
        foreach (PecaSpeedBot p in InventarioManager.Instance.pecasGuardadas) { if (p == pecaBuscada) total++; }
        return total;
    }

    // --- FABRICAÇÃO ---
    public void FabricarItem() // Ligado ao botão final de construir.
    {
        if (receitaSelecionada == null) return; // Segurança contra cliques fantasmas.

        // 1. A SACADA DE MESTRE (SALVAMENTO DE EMERGÊNCIA): 
        // Guarda a receita atual numa variável local antes que o comando de atualizar apague ela da memória!
        ReceitaCraft receitaSalva = receitaSelecionada;

        // 2. Realiza a troca matemática: Consome os materiais exigidos.
        // Usa laços de repetição (foreach e for) para deletar da sua mochila as peças usadas, uma por uma, até quitar o custo.
        foreach (Ingrediente ing in receitaSalva.ingredientes)
        {
            for (int i = 0; i < ing.quantidadeNecessaria; i++)
            {
                InventarioManager.Instance.pecasGuardadas.Remove(ing.recursoNecessario);
            }
        }

        // 3. Entrega o prêmio (a peça final montada) para o Inventário.
        InventarioManager.Instance.AdicionarPeca(receitaSalva.pecaResultado);

        // 4. Recarrega a tela inteira (isso apaga tudo e recalcula os números nos botõezinhos para mostrar que você gastou recursos).
        // Cuidado: Isso vai zerar a 'receitaSelecionada' global.
        FiltrarReceitas(filtroAtual, tituloAtual);

        // 5. Continuação da SACADA DE MESTRE: Usa a cópia salva para "re-injetar" os detalhes na visualização da direita sem bugar.
        SelecionarReceita(receitaSalva);
    }

    private void LimparDetalhes() // Desliga a interface da direita.
    {
        if (painelDetalhesDireita != null) painelDetalhesDireita.SetActive(false);
        receitaSelecionada = null; // Zera a memória de cliques.
    }
}