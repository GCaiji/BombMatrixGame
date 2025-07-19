using UnityEngine;
using System.Collections;

public class DestructibleTile : MonoBehaviour
{
    private bool _isDestroyed = false;
    
    public bool IsDestroyed => _isDestroyed;
    
    // 错误1修复：移除了错误的接口声明（第20行）
    public void DestroyTile()
    {
        if (_isDestroyed) return;
        
        _isDestroyed = true;
        StartCoroutine(PlayDestructionAnimation());
    }
    
    // 错误2修复：移除了泛型 <T> 并且返回 null 是合法的
    private IEnumerator PlayDestructionAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * 0.5f;
        float duration = 0.5f;
        float elapsed = 0;
        
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null; // 正确返回 null
        }
        
        gameObject.SetActive(false);
    }
}