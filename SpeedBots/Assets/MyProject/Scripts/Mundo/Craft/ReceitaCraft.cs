using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Ingrediente
{
    public PecaSpeedBot recursoNecessario; // A sucata, engrenagem, etc.
    public int quantidadeNecessaria;
}

[CreateAssetMenu(fileName = "NovaReceita", menuName = "SpeedBot/Receita de Craft")]
public class ReceitaCraft : ScriptableObject
{
    [Header("O que esta receita fabrica?")]
    public PecaSpeedBot pecaResultado;

    [Header("O que ela exige?")]
    public List<Ingrediente> ingredientes;
}
