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
        if (!pecasGuardadas.Contains(peca)) return;

        switch (peca.tipoPeca)
        {
            case TipoPeca.Chassi:
                chassiEquipado = peca;
                break;
            case TipoPeca.Motor:
                motorEquipado = peca;
                break;
            case TipoPeca.Modulo:
                moduloEquipado = peca;
                break;
        }
        Debug.Log($"<color=yellow>[OFICINA]</color> {peca.nomeDaPeca} foi equipado no SpeedBot!");
    }
}
