using UnityEngine;
using UnityEngine.UI; // Necessário para usar a Image do retrato
using TMPro;
using System.Collections.Generic;

// Esta é a nova "caixinha" que guarda as informações de cada linha de fala
[System.Serializable]
public class FalaDialogo
{
    public string nomeDoFalante;
    public Sprite retrato;
    [TextArea(3, 10)]
    public string texto;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI Elements")]
    public GameObject painelDialogo;
    public TextMeshProUGUI textoNome;
    public TextMeshProUGUI textoDialogo;
    public Image imagemRetrato; // Nova gaveta para o retrato

    [HideInInspector] public bool isTalking = false;

    // A fila agora guarda a estrutura completa, não apenas o texto
    private Queue<FalaDialogo> frasesFila = new Queue<FalaDialogo>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        painelDialogo.SetActive(false);
    }

    public void IniciarDialogo(FalaDialogo[] falas)
    {
        isTalking = true;
        painelDialogo.SetActive(true);

        frasesFila.Clear();
        foreach (FalaDialogo fala in falas)
        {
            frasesFila.Enqueue(fala);
        }

        AvancarDialogo();
    }

    public void AvancarDialogo()
    {
        if (frasesFila.Count == 0)
        {
            EncerrarDialogo();
            return;
        }

        // Tira a próxima fala da fila e atualiza toda a UI
        FalaDialogo falaAtual = frasesFila.Dequeue();

        textoNome.text = falaAtual.nomeDoFalante;
        textoDialogo.text = falaAtual.texto;

        // Troca a foto se houver uma, ou esconde a imagem se não houver
        if (falaAtual.retrato != null)
        {
            imagemRetrato.sprite = falaAtual.retrato;
            imagemRetrato.gameObject.SetActive(true);
        }
        else
        {
            imagemRetrato.gameObject.SetActive(false);
        }
    }

    private void EncerrarDialogo()
    {
        isTalking = false;
        painelDialogo.SetActive(false);
    }
}