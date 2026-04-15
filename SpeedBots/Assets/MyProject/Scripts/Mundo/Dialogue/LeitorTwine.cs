using UnityEngine; // Importa as ferramentas principais da engine da Unity.
using System.Collections.Generic; // Importa as ferramentas para criarmos Listas e Dicionários.
using System.Text.RegularExpressions; // Importa a biblioteca de Expressões Regulares (Regex), o motor que vai procurar padrões no texto.

[System.Serializable] // Permite que a Unity enxergue essa classe customizada no Inspector.
public class LinhaDeFala // Molde simples que separa e guarda os dados de uma única frase.
{
    public string nome; // Guarda quem está falando (ex: "Sam").
    public string texto; // Guarda o que a pessoa disse (ex: "Olá!").
}

[System.Serializable]
public class NoTwine // Molde que representa um "Nó" (ou uma "Página" de cena) inteira lá do Twine.
{
    public string titulo; // O nome invisível dessa cena (ex: "Cena_Garagem_01").
    public List<LinhaDeFala> falas = new List<LinhaDeFala>(); // A lista de todas as frases que acontecem nesta cena.
    public Dictionary<string, string> respostas = new Dictionary<string, string>(); // Guarda as escolhas do jogador (o texto do botão e para onde ele leva).
}

public class LeitorTwine : MonoBehaviour // O "tradutor de roteiros" do jogo.
{
    // --- INSTÂNCIA BLINDADA (Singleton) ---
    // Garante que só exista um único leitor de história ativo no jogo inteiro.
    private static LeitorTwine _instance;
    public static LeitorTwine Instance
    {
        get
        {
            if (_instance == null) _instance = FindFirstObjectByType<LeitorTwine>();
            return _instance;
        }
    }

    // O grande Dicionário na memória: Guarda a história inteira do jogo. 
    // Você dá o nome da cena (string), e ele te devolve a página completa com falas e botões (NoTwine).
    public Dictionary<string, NoTwine> historia = new Dictionary<string, NoTwine>();

    void Awake()
    {
        // Proteção do Singleton: destrói cópias se houver mais de um LeitorTwine na cena.
        if (_instance != null && _instance != this)
        {
            Destroy(_instance.gameObject);
        }
        _instance = this;
    }

    // Função principal que recebe o nome do arquivo de texto para traduzir.
    public void CarregarTwee(string nomeArquivo)
    {
        // Vai na pasta oculta "Resources" da Unity e tenta puxar o arquivo de texto puro (.twee).
        TextAsset arquivo = Resources.Load<TextAsset>(nomeArquivo);
        if (arquivo == null)
        {
            // Se o arquivo não existir, grita um erro no console.
            Debug.LogError($"[TWINE] Arquivo {nomeArquivo} não encontrado em Resources!");
            return;
        }

        historia.Clear(); // Limpa memórias de histórias antigas antes de carregar a nova.

        // Corta o texto gigante do Twine em vários pedaços usando o separador ":: " (que é como o Twine separa as cenas no arquivo).
        string[] blocos = arquivo.text.Split(new string[] { ":: " }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string bloco in blocos) // Pega cada cena separada...
        {
            // Ignora os blocos de configuração internos que o programa Twine gera sozinho.
            if (bloco.StartsWith("StoryTitle") || bloco.StartsWith("StoryData")) continue;

            NoTwine novoNo = new NoTwine(); // Cria uma nova "página" em branco na memória.

            // A primeira linha do bloco sempre é o Título da cena. Ele descobre onde a linha acaba ('\n').
            int fimDaPrimeiraLinha = bloco.IndexOf('\n');
            if (fimDaPrimeiraLinha == -1) continue;

            // Extrai o título e limpa qualquer sujeira/espaço extra (Trim).
            string cabecalho = bloco.Substring(0, fimDaPrimeiraLinha).Trim();
            novoNo.titulo = Regex.Replace(cabecalho, @"\s*\{.*?\}\s*", "").Trim(); // Remove tags invisíveis de posição do Twine.

            // O resto do texto (depois da primeira linha) é o corpo real da história.
            string corpoTexto = bloco.Substring(fimDaPrimeiraLinha).Trim();

            // --- A MÁGICA DOS BOTÕES ---
            // Usa o Regex para procurar qualquer coisa escrita entre colchetes duplos [[texto]].
            MatchCollection links = Regex.Matches(corpoTexto, @"\[\[(.*?)\]\]");
            foreach (Match match in links)
            {
                string conteudoLink = match.Groups[1].Value; // Pega o que está dentro do colchete.
                if (conteudoLink.Contains("->")) // Se ele achar a setinha "->" (padrão do Twine para apontar destino)...
                {
                    // Corta o texto no meio: a parte esquerda é o botão, a parte direita é a próxima cena!
                    string[] partes = conteudoLink.Split(new string[] { "->" }, System.StringSplitOptions.None);
                    novoNo.respostas.Add(partes[0].Trim(), partes[1].Trim()); // Salva a opção no dicionário de respostas.
                }
                else
                {
                    // Se não tiver setinha, significa que o texto do botão é igual ao nome da próxima cena.
                    novoNo.respostas.Add(conteudoLink.Trim(), conteudoLink.Trim());
                }
            }

            // --- LIMPEZA DE CÓDIGO ---
            // Substitui a formatação de cores padrão do Twine (Harlowe) para o padrão que a Unity/TextMeshPro entende (<color>).
            string textoColorido = Regex.Replace(corpoTexto, @"\(text-colour:(.*?)\)\[(.*?)\]", "<color=$1>$2</color>");
            // Apaga as opções de botões [[ ]] do corpo do texto, já que eles já foram salvos lá em cima e vão virar botões de verdade na tela.
            string textoLimpo = Regex.Replace(textoColorido, @"\[\[(.*?)\]\]", "").Trim();

            // --- FATIADOR DE DIÁLOGOS ---
            // Pega o texto limpo e quebra ele em linhas individuais.
            string[] linhasDoTexto = textoLimpo.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (string linha in linhasDoTexto) // Analisa linha por linha da história.
            {
                string linhaTrim = linha.Trim();
                int indexDoisPontos = linhaTrim.IndexOf(':'); // Procura onde está o dois-pontos (:) na frase.

                LinhaDeFala novaFala = new LinhaDeFala();

                // Se achar o ':' bem no comecinho da frase (menos de 20 caracteres para ignorar ':' perdidos em falas grandes)...
                if (indexDoisPontos > 0 && indexDoisPontos < 20)
                {
                    // O nome do personagem é tudo que está ANTES do dois-pontos.
                    novaFala.nome = linhaTrim.Substring(0, indexDoisPontos).Trim();
                    // A fala real é tudo que está DEPOIS do dois-pontos.
                    novaFala.texto = linhaTrim.Substring(indexDoisPontos + 1).Trim();
                }
                else
                {
                    // Se não achar dois-pontos, deduz que é uma narração do ambiente.
                    novaFala.nome = "Narrador";
                    novaFala.texto = linhaTrim; // A linha inteira vira a fala.
                }

                novoNo.falas.Add(novaFala); // Adiciona a fala fatiada dentro da "página" atual.
            }

            // No fim da tradução desse bloco inteiro, guarda a página montada no grande Dicionário da história!
            historia.Add(novoNo.titulo, novoNo);
        }
    }
}