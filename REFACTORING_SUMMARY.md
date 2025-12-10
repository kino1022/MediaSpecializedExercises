# ステートマシンの循環参照解消 リファクタリング概要

## 問題点

元の設計では以下の循環参照が発生していました：

```
EnemyStatePatternEntry
    ├─> IEnemyBehaviourState
    │       └─> EnemyStatePatternEntry (NextState)  ← 循環参照
    └─> IStateTransitionTrigger
            └─> EnemyStatePatternEntry (NextState)  ← 循環参照
```

### 循環参照による問題

1. **シリアライゼーションの問題**：UnityやOdinSerializerが無限ループに陥る可能性
2. **メモリリーク**：オブジェクトが相互参照し続けるため、GCが回収できない
3. **保守性の低下**：デバッグやエディタでの可視化が困難

## 解決策

**IDベースの参照解決パターン**を採用しました。

```
EnemyStatePatternEntry
    ├─> StateId (string)                    ← 一意識別子
    ├─> IEnemyBehaviourState
    │       └─> NextStateId (string)        ← IDのみを保持
    └─> IStateTransitionTrigger
            └─> NextStateId (string)        ← IDのみを保持
```

### 実装の詳細

#### 1. EnemyStatePatternEntryにStateIdを追加

```csharp
public class EnemyStatePatternEntry {
    [OdinSerialize]
    [LabelText("ステートID（一意）")]
    public string StateId;  // 追加
    
    [OdinSerialize]
    public IEnemyBehaviourState State;
    
    [OdinSerialize]
    public List<IStateTransitionTrigger> TransitionTriggers;
}
```

#### 2. IEnemyBehaviourStateをID参照に変更

```csharp
public interface IEnemyBehaviourState {
    // 変更前: EnemyStatePatternEntry NextState { get; }
    string NextStateId { get; }  // 変更後
    // ...
}

public abstract class AEnemyBehaviourState : IEnemyBehaviourState {
    // 変更前: protected EnemyStatePatternEntry nextState;
    protected string nextStateId;  // 変更後
    
    // 変更前: public EnemyStatePatternEntry NextState => nextState;
    public string NextStateId => nextStateId;  // 変更後
}
```

#### 3. IStateTransitionTriggerをID参照に変更

```csharp
public interface IStateTransitionTrigger {
    // 変更前: EnemyStatePatternEntry NextState { get; }
    string NextStateId { get; }  // 変更後
    // ...
}

public abstract class AStateTransitionTrigger : IStateTransitionTrigger {
    // 変更前: protected EnemyStatePatternEntry nextState;
    protected string nextStateId;  // 変更後
    
    // 変更前: public EnemyStatePatternEntry NextState => nextState;
    public string NextStateId => nextStateId;  // 変更後
}
```

#### 4. EnemyStateMachineにID解決機能を追加

```csharp
public class EnemyStateMachine : SerializedMonoBehaviour {
    // IDから実エントリを検索する辞書
    private Dictionary<string, EnemyStatePatternEntry> _stateEntryMap = new();
    
    private void Start() {
        BuildStateEntryMap();  // 辞書を構築
        // ...
    }
    
    // IDからエントリを検索する辞書を構築
    private void BuildStateEntryMap() {
        _stateEntryMap.Clear();
        
        if (_statePattern is null || _statePattern.StatePatternEntries is null) {
            return;
        }
        
        foreach (var entry in _statePattern.StatePatternEntries) {
            if (entry is null || string.IsNullOrEmpty(entry.StateId)) {
                continue;
            }
            
            if (_stateEntryMap.ContainsKey(entry.StateId)) {
                Debug.LogWarning($"重複したステートID: {entry.StateId}");
                continue;
            }
            
            _stateEntryMap[entry.StateId] = entry;
        }
    }
    
    // IDからエントリを取得
    private EnemyStatePatternEntry GetStateEntryById(string stateId) {
        if (string.IsNullOrEmpty(stateId)) {
            return null;
        }
        
        if (_stateEntryMap.TryGetValue(stateId, out var entry)) {
            return entry;
        }
        
        Debug.LogWarning($"ステートID '{stateId}' が見つかりません");
        return null;
    }
}
```

#### 5. ステート遷移処理をID参照に対応

```csharp
// FixedUpdate
private void FixedUpdate() {
    _currentState?.FixedUpdate(Time.fixedDeltaTime);
    if (CheckStateTransition()) {
        // 変更前: SetNewStateEntry(_currentState?.NextState);
        var nextEntry = GetStateEntryById(_currentState?.NextStateId);  // 変更後
        SetNewStateEntry(nextEntry);
    }
}

// Update
private void Update() {
    _currentState?.Update(Time.fixedDeltaTime);
    if (CheckStateTransition()) {
        var nextEntry = GetStateEntryById(_currentState?.NextStateId);  // 変更後
        SetNewStateEntry(nextEntry);
    }
    var triggerResult = CheckTransitionTriggers();
    if (triggerResult.Triggered && !string.IsNullOrEmpty(triggerResult.NextStateId)) {
        var nextEntry = GetStateEntryById(triggerResult.NextStateId);  // 変更後
        SetNewStateEntry(nextEntry);
    }
}
```

## メリット

1. **循環参照の完全解消**：オブジェクト同士が直接参照し合わない
2. **シリアライゼーション安全**：Unityエディタ上でも安全に保存・読み込み可能
3. **柔軟性の向上**：StateIdによる間接参照により、動的なステート構成が可能
4. **デバッグの容易化**：IDベースなのでログ出力やエディタでの確認が簡単

## 注意点

1. **StateIdの一意性**：各EnemyStatePatternEntryのStateIdは一意である必要があります
2. **存在しないIDの参照**：存在しないStateIdを参照した場合は警告ログを出力しますが、実行時エラーにはなりません
3. **命名規則**：エディタでStateIdを設定する際は、わかりやすい命名規則を使用することを推奨します（例：`Idle`, `Move`, `Attack`など）

## ゴッドクラスについて

`EnemyStateMachine`は現在、約200行程度で以下の責務を持っています：

1. ステートの初期化
2. トリガーの初期化
3. ステート遷移の管理
4. ID解決機能

これらは密接に関連しているため、現時点では**ゴッドクラスとは言えません**。ただし、将来的に以下のような機能追加が予想される場合は分離を検討すべきです：

- ステート履歴の管理
- ステート遷移のアニメーション制御
- 複雑な条件分岐ロジック
- デバッグ用の可視化機能

その場合は以下のような分離が考えられます：

```
EnemyStateMachine (ファサード)
    ├─ StateResolver (ID解決専門)
    ├─ StateTransitionController (遷移制御)
    └─ StateInitializer (初期化処理)
```

## NullReferenceExceptionについて

41行目の`_currentState?.FixedUpdate(Time.fixedDeltaTime);`でNullReferenceExceptionが発生する場合、以下の可能性があります：

1. **`_currentState`がnullでも問題なし**：null条件演算子(`?.`)を使用しているため、nullの場合はメソッドが呼ばれません
2. **`_currentState`の内部で発生している可能性**：`FixedUpdate`メソッドの実装内で他のフィールド（例：`_rigidbody`）がnullの可能性があります

### 対処方法

各具象ステートクラスの`Initialize`メソッドで必要なコンポーネントが正しく取得されているか確認してください：

```csharp
public override void Initialize(IObjectResolver resolver, GameObject enemy) {
    base.Initialize(resolver, enemy);
    _rigidbody = enemy.GetComponent<Rigidbody>();
    
    if (_rigidbody == null) {
        Debug.LogError($"Rigidbody component not found on {enemy.name}");
    }
}
```

