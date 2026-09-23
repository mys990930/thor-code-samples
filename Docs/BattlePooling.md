# 전투 오브젝트의 생성·재사용·회수

[README로 돌아가기](../README.md)

전투에서는 적·경험치·피해 텍스트가 반복해서 등장하고 사라집니다. 이 사례에는 오브젝트를 풀에서 빌리고 사용이 끝나면 돌려주는 구현과, 이를 호출하는 전투 준비·스폰·사망 처리를 함께 담았습니다.

풀은 객체의 소유와 대여 상태를 관리하고, 체력·충돌체·이벤트 구독·획득 처리 등은 각 게임 오브젝트에서 관리하도록 나누었습니다.

## 파일과 읽는 순서

| 파일 | 역할 |
| --- | --- |
| [Battle](../Samples/Shared/Battle/Battle.cs) / [IdleBattle](../Samples/BattlePooling/IdleBattle.cs) | 전투 준비·시작·종료의 공통 순서와 방치 전투의 스폰 구현 |
| [StageEnterSvc](../Samples/BattlePooling/StageEnterSvc.cs) | 스테이지에 필요한 적 코드 수집과 풀 초기화 요청 |
| [ObjectPoolManager](../Samples/BattlePooling/ObjectPoolManager.cs) | 공용 풀과 적 코드·재화 종류별 풀 구성, 전투 종료 시 정리 |
| [EnemyFactory](../Samples/BattlePooling/EnemyFactory.cs) | 소환 위치 계산, 풀에서 적을 얻어 Enemy.Init 호출 |
| [ObjectPool](../Samples/BattlePooling/ObjectPool.cs) / [IPool](../Samples/BattlePooling/IPool.cs) | 생성·대여·반환·제거와 최대 개수 관리 |
| [IObjectPoolable](../Samples/BattlePooling/IObjectPoolable.cs) | 소속 풀 전달과 개별 객체의 비활성화 계약 |
| [Enemy](../Samples/BattlePooling/Enemy.cs) | 이동·체력·피격·넉백·사망, 생존 목록과 이벤트 정리, 보상과 반환 |
| [EXP](../Samples/BattlePooling/EXP.cs) / [BattleTextMesh](../Samples/BattlePooling/BattleTextMesh.cs) | 경험치 획득과 텍스트 연출, 사용이 끝난 객체의 반환 |

## 전투 준비와 적 생성

Battle.SetupBattle의 준비 순서에서 IdleBattle이 스테이지 데이터와 적 목록을 구성합니다. 풀 준비는 `IdleBattle.SetupStageData → StageEnterSvc.SetupEnemies → ObjectPoolManager.InitEnemyPools`로 이어집니다.

IdleBattle.StartCombat은 스폰 코루틴을 시작합니다. 1초마다 생존 적이 50 미만인지 확인한 뒤 EnemyFactory.GroupSummon으로 1~3마리를 요청합니다. 이 값은 소환 요청을 시작하는 조건이므로, 한 번의 그룹 소환이 끝난 후에도 적 수가 반드시 50 이하라는 의미는 아닙니다.

실제 생성 흐름은 `EnemyFactory → ObjectPool.TryGet → Enemy.Init`입니다. 풀은 사용할 객체를 확보하고, Enemy.Init이 스테이지 계수를 반영한 체력·공격력, 위치와 표시 상태를 설정한 뒤 생존 목록에 등록합니다.

## 풀의 대여와 반환

ObjectPool은 반환된 객체를 보관하는 스택, 중복 반환을 막는 집합, 실제 소유 객체의 집합을 구분합니다. TryGet은 다음 순서로 객체를 찾습니다.

1. 반환 스택에서 유효한 소유 객체를 꺼냅니다. 삭제되거나 이미 대여된 참조는 건너뜁니다.
2. 반환 절차 없이 직접 비활성화된 소유 객체가 있으면 회수합니다.
3. 사용할 객체가 없고 최대 개수 미만이면 새로 생성합니다.
4. 확보할 수 없으면 false를 반환합니다. Get은 같은 상황에서 예외를 던집니다.

Release는 소유권과 중복 반환 여부를 확인하고 객체를 비활성화합니다. OnDisable 안에서 다시 반환·대여하는 경우도 구분합니다. 적 코드와 재화 종류별로 풀 루트를 나누었고, Remove·Disable·Clear도 해당 풀이 소유한 객체만 처리합니다.

현재 ObjectPoolManager의 설정은 다음과 같습니다.
| 대상 | 초기 개수 | 최대 개수 |
| --- | --- | --- |
| 적 코드별 풀 | 150 | 300 |
| 경험치 | 1,000 | 1,000 |
| 피해 텍스트 | 200 | 300 |

## 사망·획득·연출이 끝난 뒤의 정리

Enemy.OnDeath는 생존 목록과 충돌체를 먼저 정리하고 사망 코루틴을 시작한 뒤, 처치 기록과 보상을 처리합니다. Enemy와 Chara의 사망 판정도 피해 텍스트 대여보다 앞에 두었습니다. 보상이나 표시 처리의 실패가 사망 정리의 시작을 막지 않도록 한 순서입니다.

Enemy는 사망 연출 또는 거리 이탈 후 이벤트 구독을 해제하고 풀로 돌아갑니다. EXP는 캐릭터를 향해 이동해 경험치를 지급한 뒤 반환합니다. BattleTextMesh는 일반·치명타·피격·재화 표시를 담당하고, 표시 준비 실패나 연출 종료·실패에도 반환 경로를 거칩니다. 예외를 빈 catch로 숨기지는 않습니다.

전투가 끝나면 Battle의 종료 이벤트를 받은 ObjectPoolManager가 적·재화 풀을 정리하고, 공용 풀의 활성 객체를 회수합니다.

## 풀이 가득 찬 경우

오브젝트의 용도에 따라 확보 실패를 다르게 처리합니다.

| 대상 | 현재 처리 |
| --- | --- |
| 적·장애물 | 해당 소환 요청의 남은 생성을 중단 |
| 피해 텍스트 | 표시 생략 |
| 경험치 | 획득 계수를 적용해 캐릭터의 AddExperience로 즉시 지급 |

경험치는 고갈 시에만 이동·습득 연출 없이 지급되므로, 일반 획득과 위치·시점이 달라지고 계수도 사망 시점에 적용됩니다. AddExperience의 기존 규칙은 유지합니다. 테스트용 levelUpControl이 설정되면 획득을 차단하며, 한 번 호출할 때 한 레벨만 처리합니다.
했습니다.
