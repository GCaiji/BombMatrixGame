using UnityEngine;

[RequireComponent(typeof(MonsterController))] // 确保组件依赖关系
public class SlimeController : MonoBehaviour
{
    private Animator animator;
    private MonsterController monsterController;

    void Start()
    {
        // 获取Animator组件
        animator = GetComponent<Animator>();
        
        // 获取MonsterController组件
        monsterController = GetComponent<MonsterController>();
        
        // 订阅落地事件
        if (monsterController != null)
        {
            // monsterController.OnMonsterLanded += HandleMonsterLanding;
        }
        else
        {
            Debug.LogError("MonsterController not found on " + gameObject.name);
        }
        
        // 初始禁用Animator
        if (animator != null) 
        {
            animator.enabled = false;
            // 重置所有参数确保初始状态
            animator.Rebind();
            animator.Update(0f);
        }
        else
        {
            Debug.LogError("Animator not found on " + gameObject.name);
        }
    }

    void OnDestroy()
    {
        // 修复空引用问题
        // 仅当对象没有被销毁且还在运行时才取消订阅
        // if (this != null && monsterController != null)
        // {
        //     monsterController.OnMonsterLanded -= HandleMonsterLanding;
        // }
    }

    private void HandleMonsterLanding()
    {
        // 启用动画控制器
        if (animator != null)
        {
            animator.enabled = true;
        }
    }
}