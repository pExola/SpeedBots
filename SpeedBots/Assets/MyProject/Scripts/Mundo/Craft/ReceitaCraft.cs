using System.Collections.Generic; // Importa a biblioteca essencial para podermos usar Listas (List<>).
using UnityEngine; // Importa as ferramentas principais da engine da Unity.

[System.Serializable] // Permite que esta classe "invisível" apareça e seja editável na tela do Inspector da Unity.
public class Ingrediente // Cria uma classe personalizada que amarra perfeitamente duas informações inseparáveis:
{
    public PecaSpeedBot recursoNecessario; // 1. Qual é a peça ou recurso exigido (ex: uma sucata, uma engrenagem).
    public int quantidadeNecessaria; // 2. Qual é a quantidade matemática exata que o jogador precisa ter desse recurso.
}

// A grande mágica: Isso cria um botão no menu da Unity. Permite clicar com o botão direito e criar arquivos "NovaReceita" 
// infinitas vezes, como se fossem documentos de texto, sem precisar programar absolutamente nada novo!
[CreateAssetMenu(fileName = "NovaReceita", menuName = "SpeedBot/Receita de Craft")]
public class ReceitaCraft : ScriptableObject // É o "caderno de receitas". Sendo um ScriptableObject, ele não fica em um objeto 3D/2D na tela, é apenas um arquivo leve de dados (Data Container).
{
    [Header("O que esta receita fabrica?")] // Cria um cabeçalho para organizar a visualização no Inspector.
    public PecaSpeedBot pecaResultado; // Aponta qual será o "prêmio final". É a peça que o jogador vai ganhar quando terminar o Crafting.

    [Header("O que ela exige?")] // Cria outro cabeçalho organizador.
    public List<Ingrediente> ingredientes; // Simplesmente guarda uma Lista usando a nossa classe lá de cima, contendo todos os itens exigidos para a fabricação.
}