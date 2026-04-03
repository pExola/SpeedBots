using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// 1. A menor estrutura: Guarda quem está falando e o que ele disse naquela hora
[System.Serializable]
public class LinhaDeFala
{
    public string nome;
    public string texto;
}

// 2. A "Caixinha" do Nó do Twine agora guarda uma LISTA de falas, e não mais um texto gigante
[System.Serializable]
public class NoTwine
{
    public string titulo;
    public List<LinhaDeFala> falas = new List<LinhaDeFala>();
    public Dictionary<string, string> respostas = new Dictionary<string, string>();
}

public class LeitorTwine : MonoBehaviour
{
    public static LeitorTwine Instance { get; private set; }
    public Dictionary<string, NoTwine> historia = new Dictionary<string, NoTwine>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CarregarTwee(string nomeArquivo)
    {
        TextAsset arquivo = Resources.Load<TextAsset>(nomeArquivo);
        if (arquivo == null) return;

        historia.Clear();
        string[] blocos = arquivo.text.Split(new string[] { ":: " }, System.StringSplitOptions.RemoveEmptyEntries);

        foreach (string bloco in blocos)
        {
            if (bloco.StartsWith("StoryTitle") || bloco.StartsWith("StoryData")) continue;

            NoTwine novoNo = new NoTwine();
            int fimDaPrimeiraLinha = bloco.IndexOf('\n');
            if (fimDaPrimeiraLinha == -1) continue;

            string cabecalho = bloco.Substring(0, fimDaPrimeiraLinha).Trim();
            novoNo.titulo = Regex.Replace(cabecalho, @"\s*\{.*?\}\s*", "").Trim();

            string corpoTexto = bloco.Substring(fimDaPrimeiraLinha).Trim();

            // Mapeia links de opções
            MatchCollection links = Regex.Matches(corpoTexto, @"\[\[(.*?)\]\]");
            foreach (Match match in links)
            {
                string conteudoLink = match.Groups[1].Value;
                if (conteudoLink.Contains("->"))
                {
                    string[] partes = conteudoLink.Split(new string[] { "->" }, System.StringSplitOptions.None);
                    novoNo.respostas.Add(partes[0].Trim(), partes[1].Trim());
                }
                else
                {
                    novoNo.respostas.Add(conteudoLink.Trim(), conteudoLink.Trim());
                }
            }

            // Traduz as cores do Harlowe
            string textoColorido = Regex.Replace(corpoTexto, @"\(text-colour:(.*?)\)\[(.*?)\]", "<color=$1>$2</color>");
            string textoLimpo = Regex.Replace(textoColorido, @"\[\[(.*?)\]\]", "").Trim();

            // =========================================================
            // O FATIADOR DE DIÁLOGOS UNIVERSAL
            // =========================================================
            string[] linhasDoTexto = textoLimpo.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (string linha in linhasDoTexto)
            {
                string linhaTrim = linha.Trim();
                int indexDoisPontos = linhaTrim.IndexOf(':');

                LinhaDeFala novaFala = new LinhaDeFala();

                // Se achar os ':' no começo (Ex: "Sam:", "Tom:")
                if (indexDoisPontos > 0 && indexDoisPontos < 20)
                {
                    novaFala.nome = linhaTrim.Substring(0, indexDoisPontos).Trim();
                    novaFala.texto = linhaTrim.Substring(indexDoisPontos + 1).Trim(); // Pega tudo DEPOIS dos ":"
                }
                else
                {
                    // Regra de Ouro: Se não tem dois pontos, é Narração pura.
                    novaFala.nome = "Narrador";
                    novaFala.texto = linhaTrim;
                }

                // Adiciona a linha imediatamente na fila do nó
                novoNo.falas.Add(novaFala);
            }
            // =========================================================

            historia.Add(novoNo.titulo, novoNo);
        }
    }
}