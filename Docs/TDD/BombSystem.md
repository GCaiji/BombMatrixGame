# 炸弹系统技术方案

## 爆炸算法实现
```csharp
// Bomb.cs
IEnumerator ExplodeCoroutine() {
    yield return new WaitForSeconds(explodeDelay);
    
    // 使用圆形检测爆炸范围
    Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, blastRadius);
    
    foreach (var hit in hits) {
        if (hit.TryGetComponent<DestructibleTile>(out var tile)) {
            tile.Destroy(); // 触发图块销毁
        }
    }
    
    // 连锁爆炸逻辑
    if (chainExplode) {
        foreach (var bomb in FindChainableBombs()) {
            bomb.TriggerChain();
        }
    }
}