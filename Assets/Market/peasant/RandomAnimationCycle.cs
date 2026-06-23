using UnityEngine;

public class RandomAnimationCycle : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private int animationCount = 4;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetInteger("current", Random.Range(0, animationCount));
    }

    void Update()
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (state.normalizedTime >= 1f && !animator.IsInTransition(0))
        {
            animator.SetInteger("current", Random.Range(0, animationCount));
        }
    }
}
