using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class OficinaUIManager : MonoBehaviour
{
    public static OficinaUIManager Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject painelOficina; // O fundo da tela de montagem

    [Header("Textos das Peças Equipadas")]
    public TextMeshProUGUI textoChassi;
    public TextMeshProUGUI textoMotor;
    public TextMeshProUGUI textoModulo;

    // Controladores para saber em qual peça da lista estamos
    private int indexChassi = -1;
    private int indexMotor = -1;
    private int indexModulo = -1;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        painelOficina.SetActive(false);
    }

    public void AbrirOficina()
    {
        painelOficina.SetActive(true);
        AtualizarTextos();
    }

    public void FecharOficina()
    {
        painelOficina.SetActive(false);
    }

    // --- MÉTODOS PARA OS BOTÕES DA UI ---

    public void CiclarChassi() { CiclarPeca(TipoPeca.Chassi, ref indexChassi); }
    public void CiclarMotor() { CiclarPeca(TipoPeca.Motor, ref indexMotor); }
    public void CiclarModulo() { CiclarPeca(TipoPeca.Modulo, ref indexModulo); }

    // --- A LÓGICA DE TROCA ---

    private void CiclarPeca(TipoPeca tipo, ref int indexAtual)
    {
        // 1. Pega o inventário do Player
        List<PecaSpeedBot> mochila = InventarioManager.Instance.pecasGuardadas;

        // 2. Filtra a mochila para achar SÓ as peças do tipo que clicamos (ex: só Motores)
        List<PecaSpeedBot> pecasValidas = mochila.FindAll(p => p.tipoPeca == tipo);

        if (pecasValidas.Count == 0)
        {
            Debug.Log($"[OFICINA] Você não tem nenhuma peça do tipo {tipo} na mochila!");
            return;
        }

        // 3. Avança para a próxima peça da lista
        indexAtual++;

        // Se chegou no final da lista, volta para a primeira peça (Ciclo infinito)
        if (indexAtual >= pecasValidas.Count) indexAtual = 0;

        // 4. Manda o InventarioManager equipar de verdade
        InventarioManager.Instance.EquiparPeca(pecasValidas[indexAtual]);

        // 5. Atualiza o visual da tela
        AtualizarTextos();
    }

    private void AtualizarTextos()
    {
        // Se houver uma peça equipada, mostra o nome. Se não, mostra "Nenhum".
        textoChassi.text = InventarioManager.Instance.chassiEquipado != null ? InventarioManager.Instance.chassiEquipado.nomeDaPeca : "Nenhum";
        textoMotor.text = InventarioManager.Instance.motorEquipado != null ? InventarioManager.Instance.motorEquipado.nomeDaPeca : "Nenhum";
        textoModulo.text = InventarioManager.Instance.moduloEquipado != null ? InventarioManager.Instance.moduloEquipado.nomeDaPeca : "Nenhum";
    }
}
