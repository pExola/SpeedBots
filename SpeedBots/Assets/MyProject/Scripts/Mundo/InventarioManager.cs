using System.Collections.Generic;
using UnityEngine;

public class InventarioManager : MonoBehaviour
{
    public static InventarioManager Instance { get; private set; }

    [Header("Mochila")]
    public List<PecaSpeedBot> pecasGuardadas = new List<PecaSpeedBot>();

    [Header("SpeedBot Montado (Loadout)")]
    public PecaSpeedBot chassiEquipado;
    public PecaSpeedBot motorEquipado;
    public PecaSpeedBot moduloEquipado;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Isso garante que o inventário não seja destruído quando você carregar a pista de corrida!
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AdicionarPeca(PecaSpeedBot novaPeca)
    {
        pecasGuardadas.Add(novaPeca);
        Debug.Log($"<color=cyan>[INVENTÁRIO]</color> Você encontrou uma nova peça: {novaPeca.nomeDaPeca} ({novaPeca.tipoPeca})!");
    }

    public void EquiparPeca(PecaSpeedBot peca)
    {
        // Se a peça não está na mochila, cancela
        if (!pecasGuardadas.Contains(peca)) return;

        // 1. Tira a peça nova da mochila
        pecasGuardadas.Remove(peca);

        // 2. Equipa a nova e devolve a antiga para a mochila
        switch (peca.tipoPeca)
        {
            case TipoPeca.Chassi:
                if (chassiEquipado != null) pecasGuardadas.Add(chassiEquipado); // Devolve a velha
                chassiEquipado = peca; // Equipa a nova
                break;

            case TipoPeca.Motor:
                if (motorEquipado != null) pecasGuardadas.Add(motorEquipado);
                motorEquipado = peca;
                break;

            case TipoPeca.Modulo:
                if (moduloEquipado != null) pecasGuardadas.Add(moduloEquipado);
                moduloEquipado = peca;
                break;
        }

        Debug.Log($"<color=yellow>[OFICINA]</color> {peca.nomeDaPeca} foi equipado e a peça antiga voltou pra mochila!");
    }
}
