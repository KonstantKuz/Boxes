# Physics Joints Requirements

## Boxes Attachment System

### CRITICAL REQUIREMENTS:
- Ящики ДОЛЖНЫ быть прикреплены через ДЖОИНТЫ (НЕ kinematic, НЕ parent)
- ДОЛЖНА быть НЕБОЛЬШАЯ свобода движения (не как на клей)
- НЕ должны выпадать при вращении платформы
- НЕ должны дергаться при движении/вращении платформы

### TESTED AND FAILED:
- ❌ SpringJoint - дергается при вращении платформы
- ❌ ConfigurableJoint с Limited motion (первая попытка, большие лимиты) - выпадают при вращении
- ❌ ConfigurableJoint с Drive моторами (первая попытка) - зависают в воздухе, дергаются
- ❌ ConfigurableJoint Locked с soft springs (первая попытка) - дергается
- ❌ CharacterJoint - очень сильно дергается
- ❌ ConfigurableJoint Locked linear + Limited angular + Projection (первая попытка) - залезают внутрь платформы (но дергаются меньше!)
- ❌ ConfigurableJoint Limited на всех осях без springs - дергается
- ❌ ConfigurableJoint Limited + Soft springs (вторая попытка, средние лимиты) - очень сильно дергается
- ⚠️ ConfigurableJoint Free + Drive (XYAndZ) - ПРОГРЕСС! Хорошо держатся, но крутятся на месте и при поворотах вылетают
- ❌ ConfigurableJoint Free + очень жесткие Drive (Slerp) - крутятся на месте, дергаются, сильно разлетаются при поворотах
- ❌ ConfigurableJoint Locked + tight projection - приклеены намертво, висят в воздухе неестественно, зависают при поворотах
- ❌ ConfigurableJoint Limited с минимальными лимитами + жесткие springs - дергаются, вылетают, свисают с края
- ❌ FixedJoint после исправления движения платформы - выпадают при старте движения
- ✅ ConfigurableJoint Locked linear + Free angular + Slerp Drive (после исправления движения) - РАБОТАЕТ ОЧЕНЬ ХОРОШО!
  - **БАЗОВАЯ РАБОЧАЯ КОНФИГУРАЦИЯ ДЛЯ ДАЛЬНЕЙШИХ ЭКСПЕРИМЕНТОВ**
  - Locked все линейные оси (жесткое крепление по позиции)
  - Free все угловые оси (свободное вращение)
  - Slerp Drive: spring=100000, damper=10000, maxForce=50000
  - Solver iterations = 100
  - Drag: linear=2, angular=5

### ✅ SOLUTION FOUND:

**ROOT CAUSE**: Проблема была НЕ в настройках джоинтов, а в том КАК двигались платформы!

**Ошибка**: Платформы (CarAgent) двигались через прямое изменение `transform.position` и `transform.rotation` в методе Update()

**Почему это вызывало дергание**:
- Прямое изменение Transform обходит внутренние расчеты скорости PhysX
- Unity PhysX не может корректно вычислить velocity/angularVelocity для джоинтов
- Джоинты теряют информацию о движении платформы и начинают дергаться

**Решение**: Использовать `Rigidbody.MovePosition()` и `Rigidbody.MoveRotation()` в FixedUpdate()

**Источники**:
- [Unity Manual: Joint and Ragdoll stability](https://docs.unity3d.com/Manual//RagdollStability.html)
  > "Avoid direct Transform manipulation on kinematic rigidbodies connected via joints. This bypasses PhysX's internal velocity calculations."
- [Unity Discussions: Joints jittering](https://discussions.unity.com/t/i-figured-out-why-joints-are-jittering/699018)

**Реализация**:
- Добавлено опциональное поле `[SerializeField] private Rigidbody carRigidbody` в CarAgent.cs
- Если carRigidbody назначен в инспекторе - движение происходит через MovePosition/MoveRotation в FixedUpdate
- Если carRigidbody не назначен - работает старый метод через transform в Update (для обратной совместимости)
- Изменены места: CarAgent.cs строки 220-242 (UpdateDriving) и 292-299 (ShouldStopOnEdge)

### ✅✅✅ CURRENT WORKING IMPLEMENTATION - BASELINE ✅✅✅

**ConfigurableJoint Locked linear + Free angular + Slerp Drive**
**СТАТУС: РАБОТАЕТ ОЧЕНЬ ХОРОШО - это базовая конфигурация для дальнейших улучшений**

#### Полная конфигурация джоинта (AttachableObject.cs):

```csharp
// Тип джоинта
private ConfigurableJoint joint;

// Параметры в инспекторе
breakForce = 50000f
breakTorque = 50000f
slerpDriveSpring = 100000f
slerpDriveDamper = 10000f
slerpDriveMaxForce = 50000f

// Настройка джоинта в коде (метод Attach):
joint.connectedBody = target;
joint.enablePreprocessing = true;
joint.enableCollision = true;
joint.breakForce = breakForce;
joint.breakTorque = breakTorque;

// Anchor
joint.anchor = Vector3.zero;  // В центре масс ящика
joint.autoConfigureConnectedAnchor = false;
joint.connectedAnchor = target.transform.InverseTransformPoint(anchorPoint ?? transform.position);

// Линейные оси - ПОЛНОСТЬЮ ЗАБЛОКИРОВАНЫ
joint.xMotion = ConfigurableJointMotion.Locked;
joint.yMotion = ConfigurableJointMotion.Locked;
joint.zMotion = ConfigurableJointMotion.Locked;

// Угловые оси - ПОЛНОСТЬЮ СВОБОДНЫ
joint.angularXMotion = ConfigurableJointMotion.Free;
joint.angularYMotion = ConfigurableJointMotion.Free;
joint.angularZMotion = ConfigurableJointMotion.Free;

// Привод вращения
joint.rotationDriveMode = RotationDriveMode.Slerp;
JointDrive slerpDrive = new JointDrive();
slerpDrive.positionSpring = 100000f;  // Очень жесткая пружина
slerpDrive.positionDamper = 10000f;   // Сильное демпфирование
slerpDrive.maximumForce = 50000f;     // Высокая максимальная сила
joint.slerpDrive = slerpDrive;
```

#### Rigidbody параметры при прикреплении:
```csharp
rigidbody.solverIterations = 100;              // Было 50, увеличено
rigidbody.solverVelocityIterations = 100;      // Было 50, увеличено
rigidbody.maxDepenetrationVelocity = 2f;       // Без изменений
rigidbody.drag = 2f;                           // Было 5, уменьшено
rigidbody.angularDrag = 5f;                    // Было 10, уменьшено
```

#### Box.prefab параметры:
- Rigidbody: Mass=10, CollisionDetection=Continuous, Interpolate=Interpolate
- BoxCollider: обычный collider без физического материала
- ConstantForce: y=-200 (дополнительная гравитация)

#### Почему это работает:
1. **Locked linear motion** - ящики жестко привязаны по позиции к платформе, не смещаются
2. **Free angular motion + Slerp Drive** - ящики могут слегка вращаться (естественное движение), но привод их стабилизирует
3. **Высокие solver iterations (100)** - PhysX тратит больше времени на решение джоинтов = более стабильно
4. **Умеренный drag (2/5)** - не слишком высокий чтобы не создавать конфликты, но достаточный для демпфирования
5. **Rigidbody.MovePosition/MoveRotation в CarAgent** - правильное движение платформы через PhysX

#### КРИТИЧНОЕ ИСПРАВЛЕНИЕ - DestroyImmediate для джоинта:
```csharp
// При отсоединении (в else if блоке метода Attach):
DestroyImmediate(joint);  // НЕ Destroy! Джоинт должен удалиться НЕМЕДЛЕННО
joint = null;
attachedTo = null;
```

**ПОЧЕМУ ВАЖНО**: Если использовать `Destroy(joint)` вместо `DestroyImmediate(joint)`:
- Джоинт удалится только в конце кадра
- В Box.cs метод ApplyThrowPhysics вызывает `Attach(null)` и сразу `rigidbody.velocity = ...`
- Джоинт еще существует когда применяется velocity - это блокирует правильное применение скорости
- Ящик становится "тяжелым" после выкидывания из хранилища
- **РЕШЕНИЕ**: DestroyImmediate удаляет джоинт сразу, velocity применяется корректно

#### КРИТИЧНОЕ ИСПРАВЛЕНИЕ #2 - Отцепление в BoxesStorage.OnTriggerExit:

**ПРОБЛЕМА**: После выкидывания ящика из хранилища через BoxesStorageSpreader:
1. ApplyThrowPhysics вызывает Attach(null) → восстанавливает drag=0
2. Ящик вылетает и падает обратно в триггер хранилища
3. OnTriggerEnter срабатывает СНОВА → box.Attach(Rigidbody) → drag=2 опять!
4. При подборе игроком drag=2 не сбрасывается → ящик летит плохо

**РЕШЕНИЕ в BoxesStorage.cs**:

```csharp
// OnTriggerExit - ОБЯЗАТЕЛЬНО отцеплять ящик!
private void OnTriggerExit(Collider other)
{
    if (other.TryGetComponent(out Box box) && boxes.Contains(box))
    {
        box.Attach(null);  // КРИТИЧНО! Отцепить при выходе из триггера
        boxes.Remove(box);
    }
}

// OnTriggerEnter - НЕ прикреплять уже прикрепленный ящик!
private void OnTriggerEnter(Collider other)
{
    if (other.TryGetComponent(out Box box) && !boxes.Contains(box))
    {
        // Проверка: если ящик уже прикреплен к чему-то - не прикреплять!
        if (box.TryGetComponent(out AttachableObject attachableObject) && attachableObject.IsAttached)
        {
            return;
        }

        // ... остальной код прикрепления
        box.Attach(Rigidbody, anchorPoint);
        boxes.Add(box);
    }
}
```

#### Что делать если нужно вернуться к этой конфигурации:
1. Скопировать весь код выше в AttachableObject.cs
2. Обновить префаб Box.prefab с параметрами выше
3. Убедиться что CarAgent использует Rigidbody movement (carRigidbody назначен в инспекторе)
4. КРИТИЧНО: использовать DestroyImmediate для удаления джоинта при отсоединении

### WORKFLOW:
- ❗ ВСЕГДА обновлять память проекта (.claude/rules/physics-joints.md) после изменений
- ❗ ВСЕГДА обновлять префаб (Box.prefab) вместе с кодом (AttachableObject.cs)

### FILES:
- `Assets/Scripts/Gameplay/Interactable/BoxesInteraction/Components/AttachableObject.cs` - компонент прикрепления ящиков
- `Assets/Prefabs/Game/Box/Box.prefab` - префаб ящика
- `Assets/Scripts/Gameplay/RoadSystem/CarAgent.cs` - движение платформ (КРИТИЧНО: должно использовать Rigidbody.MovePosition/MoveRotation для корректной работы джоинтов)
- `Assets/Scripts/Gameplay/Interactable/BoxesInteraction/Components/BoxesStorage.cs` - хранилище ящиков (КРИТИЧНО: должно отцеплять в OnTriggerExit и проверять IsAttached в OnTriggerEnter)