using UnityEngine;

public class BombDestroyCallback : StateMachineBehaviour
{
    public System.Action onDestroyComplete;

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsName("Destroy") && onDestroyComplete != null)
        {
            onDestroyComplete();
        }
    }
}