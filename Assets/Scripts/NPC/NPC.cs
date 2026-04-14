using UnityEngine;

public class NPC : MonoBehaviour
{
    [TextArea]
    public string[] dialogueLines; // GO TO INSPECTOR & FILL IN DIALOGUE LINES!

    private Animator anim;
    private bool hasSpoken = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)//box trigger for dialogue
    {
        if (collision.CompareTag("Player") && !hasSpoken)
        {
            hasSpoken = true;//prevent re triggering dialogue
            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        anim.SetBool("IsTalking", true);

        Debug.Log("speaking!!!"); //debug remove
    }

    public void FinishDialogue()
    {
        anim.SetBool("IsTalking", false);

        Debug.Log("going to work"); //debug remove
    }


}