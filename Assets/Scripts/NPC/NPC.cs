using UnityEngine;

public class NPC : MonoBehaviour
{
    public enum NPCType {Looping, Reaction}//One is animation loops then stops when talking to player, other stays still then shifts animation when talking then goes back to idle
    public NPCType npcType;// drop down in inspector to organize better

    [TextArea]
    public string[] dialogueLines;

    private Animator anim;
    public bool hasSpoken = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void StartTalkingAnimation()
    {
        if (anim == null) return;
        if (npcType == NPCType.Looping)
            anim.SetBool("IsTalking", true);
        else if (npcType == NPCType.Reaction)
            anim.SetTrigger("PlayReaction");
    }

    public void StopTalkingAnimation()
    {
        if (anim==null) return;
        if (npcType == NPCType.Looping)
            anim.SetBool("IsTalking", false);
    }


}