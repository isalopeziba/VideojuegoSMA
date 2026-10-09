using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text continueIndicator;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button skipButton;

    [Header("Configuración")]
    [SerializeField] private float textSpeed = 0.03f;

    [Header("Diálogo de prueba")]
    [SerializeField] private string characterName = "DUENDE";

    [TextArea(2, 5)]
    [SerializeField]
    private string[] dialogueLines =
    {
        "Ten cuidado... este bosque no es como los demás.",
        "La Madre Monte protege este lugar.",
        "Debemos demostrar que comprendemos lo que protege."
    };

    private int currentLine = 0;
    private bool isTyping = false;
    private bool dialogueFinished = false;

    private Coroutine typingCoroutine;

    private void Start()
    {
        characterNameText.text = characterName;

        continueButton.onClick.AddListener(ContinueDialogue);
        skipButton.onClick.AddListener(SkipDialogue);

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (currentLine >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(dialogueLines[currentLine]));
    }

    private IEnumerator TypeText(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        continueIndicator.gameObject.SetActive(false);

        foreach (char letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;

        continueIndicator.gameObject.SetActive(true);
    }

    public void ContinueDialogue()
    {
        if (dialogueFinished)
            return;

        // Si el texto todavía se está escribiendo,
        // mostrarlo completo inmediatamente.
        if (isTyping)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            dialogueText.text = dialogueLines[currentLine];
            isTyping = false;

            continueIndicator.gameObject.SetActive(true);

            return;
        }

        currentLine++;

        ShowCurrentLine();
    }

    public void SkipDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        EndDialogue();
    }

    private void EndDialogue()
    {
        dialogueFinished = true;

        dialogueText.text = "";

        continueButton.gameObject.SetActive(false);
        skipButton.gameObject.SetActive(false);
        continueIndicator.gameObject.SetActive(false);

        // Ocultar todo el cuadro de diálogo
        gameObject.SetActive(false);
    }
}