using UnityEngine;

public class NPC : MonoBehaviour
{
    [TextArea]
    public string[] dialogueLines; // GO TO INSPECTOR & FILL IN DIALOGUE LINES!

    private Animator anim;
    private bool has Spoken = false;

    void Start()
    {
        anim = GetComponent<animator>();
    }

    private void OnTriggerEnter2d(Collider2d collision)//box trigger for dialogue
    {
        if (collision.CompareTag("Player ") && !hasSpoken)
        {
            has spoken = true;//prevent re triggering dialogue
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