# 토르 키우기 — Unity/C# 코드 샘플

‘토르 키우기’는 ‘Vampire Survivors’ 와 같은 서바이벌 장르의 게임에 방치형 RPG의 지속적인 성장과 파밍 구조를 결합한 방치형 서바이벌 RPG 게임입니다. 게임 전반을 기획하고 개발했으며, 출시 전 개발을 중단했습니다.

이 저장소에는 프로젝트에서 일부를 발췌해 정리했습니다. 메서드 일부만 따로 떼어 놓기보다, 기능의 시작점부터 실제 처리까지 따라갈 수 있도록 부모 클래스·호출부·인터페이스·모델을 함께 담았습니다.

> 전체 게임이 아닌 코드 샘플입니다. 씬과 에셋, 일부 의존 코드가 제외되어 이 저장소만으로 컴파일하거나 게임을 실행할 수는 없습니다.

## 담당 범위

프로젝트의 전반적인 클라이언트 코드를 담당했습니다. stat/item/skill/gacha의 UI 연결 서비스는 다른 개발자가 담당한 부분이 있어 소개 범위에서 제외했습니다. 여기서는 직접 구현한 전투 로직과 데이터 처리 흐름을 중심으로 소개합니다.

## 주요 구현과 탐색 순서

| 순서 | 사례 | 살펴볼 내용 | 시작 코드 |
| --- | --- | --- | --- |
| 1 | [등급에 따라 달라지는 스킬 실행](Docs/CombatSkills.md) | 공통 초기화·피해량 계산, 등급별 공격 동작, 투사체의 피격 연결 | [CharaBuildSvc](Samples/CombatSkills/CharaBuildSvc.cs) → [Chara.UpdateSkill](Samples/Shared/Battle/Chara.cs) → [NormalSkill](Samples/CombatSkills/Base/NormalSkill.cs) |
| 2 | [전투 오브젝트의 생성·재사용·회수](Docs/BattlePooling.md) | 전투 준비와 적 생성, 풀 소유권·반환, 개별 오브젝트의 상태 정리 | [IdleBattle](Samples/BattlePooling/IdleBattle.cs) → [EnemyFactory](Samples/BattlePooling/EnemyFactory.cs) → [ObjectPool](Samples/BattlePooling/ObjectPool.cs) |
| 3 | [서버 스킬 데이터 초기화](Docs/ServerSkillData.md) | 요청·응답 변환·데이터 보관의 역할 분리, 초기화 실패 처리 | [SkillsDataContainer](Samples/ServerSkillData/SkillsDataContainer.cs) → [SkillDAO](Samples/ServerSkillData/SkillDAO.cs) → [ServerRequester](Samples/ServerSkillData/ServerRequester.cs) |

## 디렉터리 구성

`Samples`에는 C# 파일 71개가 있습니다. 사례별 구현을 나누고, 여러 사례가 사용하는 코드는 `Shared`에서 함께 참조하도록 정리했습니다.

```text
thor-code-samples/
├── README.md
├── .gitignore
├── Samples/
│   ├── CombatSkills/          # 스킬 공통 계층·개별 구현·캐릭터 생성 연결 (11개)
│   │   ├── Base/              # 스킬 종류별 부모 클래스와 투사체 기반 (6개)
│   │   ├── Examples/          # 스킬 2개와 각각의 투사체 (4개)
│   │   └── CharaBuildSvc.cs
│   ├── BattlePooling/         # 전투 준비·스폰·풀·적·경험치·텍스트 (10개)
│   ├── ServerSkillData/       # 요청기·DAO·매퍼·컨테이너·초기화 검사 (6개)
│   └── Shared/
│       ├── Battle/            # 캐릭터·전투 부모·애니메이션 연결 (6개)
│       ├── Core/              # 이벤트·스탯 보관·수치 타입·공통 계약 (8개)
│       ├── Interfaces/        # 피격·몬스터·이벤트 수신 계약 (7개)
│       └── Models/            # 스킬·성장·스테이지·요청/응답 모델 (23개)
└── Docs/                      # 사례 설명 3개, 의존성 설명 1개
```

## 1. 스킬 실행 — CombatSkills

스킬은 공통 데이터와 피해량 계산을 사용하되, 실제 공격 동작은 각 스킬에서 구현했습니다. 서버에 저장되는 **스킬 레벨**과 전투 중 올라가는 **등급(1~4)**을 구분하며, 피해량 테이블에는 두 값을 함께 사용합니다.

| 파일 | 담고 있는 내용 |
| --- | --- |
| [CharaBuildSvc](Samples/CombatSkills/CharaBuildSvc.cs) | 캐릭터 프리팹 생성, 스탯 연결, 전투 모드에 따른 시작 스킬 구성. Chara.UpdateSkill을 호출하는 진입점 |
| [Skill](Samples/CombatSkills/Base/Skill.cs) / [ActiveSkill](Samples/CombatSkills/Base/ActiveSkill.cs) | 스킬 코드·레벨·에셋·피해량 테이블을 연결하고, 공격 스킬의 피해량 갱신과 전투 정지·재개 이벤트 처리 |
| [NormalSkill](Samples/CombatSkills/Base/NormalSkill.cs) | 현재 등급에 맞는 GradeIAction~GradeIVAction 실행. 등급 상승 시 피해량을 다시 계산하고 공격 코루틴 재시작 |
| [PassiveSkill](Samples/CombatSkills/Base/PassiveSkill.cs) / [CombinationSkill](Samples/CombatSkills/Base/CombinationSkill.cs) | 등급별 효과 적용과 조합 스킬 실행을 위한 부모 클래스. 개별 패시브·조합 스킬 구현은 제외 |
| [SkillProjectile](Samples/CombatSkills/Base/SkillProjectile.cs) | 투사체의 초기화·이벤트 연결·타깃 탐색과 충돌 진입 처리. 개별 투사체가 OnObjHit을 재정의해 피격 대상에 피해 전달 |

`Examples`에는 서로 다른 공격 방식을 가진 두 스킬을 담았습니다.

- **망치 공격 — [Skill1140001](Samples/CombatSkills/Examples/Skill1140001.cs), [SkillProj1140001](Samples/CombatSkills/Examples/SkillProj1140001.cs)**: 바라보는 방향에 맞춰 공격 위치와 파티클 방향을 바꾸고, 등급에 따라 타격 횟수와 크기를 조정합니다. 투사체 쪽에서는 충돌체의 활성 시간과 실제 피격 호출을 처리합니다.
- **화면 밖에서 진입하는 공격 — [Skill1140002](Samples/CombatSkills/Examples/Skill1140002.cs), [SkillProj1140002](Samples/CombatSkills/Examples/SkillProj1140002.cs)**: 화면 밖의 임의 위치에서 적을 향해 이동하는 공격입니다. 스킬은 등급별 투사체 수와 속도를 설정하고, 투사체는 타깃 선택·이동·반복 간격을 담당합니다.

코드는 `Chara.UpdateSkill → Skill.Init → ActiveSkill.OnInit → NormalSkill.SkillAction → 개별 GradeAction` 순서로 읽으면 됩니다. 이어서 각 투사체의 `OnObjHit → IHittable.Hit`을 보면 공격이 적의 피격 처리로 연결됩니다. 프리팹의 자식 순서와 파티클·충돌체 구성이 전제되어 있어 코드만으로 연출까지 재현되지는 않습니다.

## 2. 전투 오브젝트 관리 — BattlePooling

반복해서 등장하는 적·경험치·피해 텍스트를 풀에서 빌리고, 사용이 끝나면 돌려주는 흐름입니다. 풀 자체의 자료구조와 각 게임 오브젝트의 초기화·정리 책임을 함께 담았습니다.

| 파일 | 담고 있는 내용 |
| --- | --- |
| [IdleBattle](Samples/BattlePooling/IdleBattle.cs) / [StageEnterSvc](Samples/BattlePooling/StageEnterSvc.cs) | 스테이지 데이터와 적 코드를 읽고 풀 준비 요청. 방치 전투에서는 생존 적 수를 확인하며 그룹 소환 반복 |
| [ObjectPoolManager](Samples/BattlePooling/ObjectPoolManager.cs) | 공용 풀과 적 코드·재화 종류별 풀 구성, 전투 종료 시 정리 |
| [ObjectPool](Samples/BattlePooling/ObjectPool.cs) / [IPool](Samples/BattlePooling/IPool.cs) / [IObjectPoolable](Samples/BattlePooling/IObjectPoolable.cs) | 생성·대여·반환·제거 계약. 반환 스택과 소유 집합을 구분해 중복 반환과 다른 풀 객체의 혼입 방지 |
| [EnemyFactory](Samples/BattlePooling/EnemyFactory.cs) | 일반·그룹·혼합 소환의 위치 계산과 Enemy.Init 호출. 보스·장애물 생성 분기도 포함하지만 해당 클래스의 구현은 제외 |
| [Enemy](Samples/BattlePooling/Enemy.cs) | 이동, 체력·공격력 초기화, 피격·넉백·사망, 생존 목록 정리, 보상 생성과 풀 반환 |
| [EXP](Samples/BattlePooling/EXP.cs) / [BattleTextMesh](Samples/BattlePooling/BattleTextMesh.cs) | 캐릭터 추적·경험치 획득 후 반환, 일반·치명타·피격·재화 텍스트 연출 후 반환 |

준비 과정은 `IdleBattle.SetupStageData → StageEnterSvc.SetupEnemies → ObjectPoolManager.InitEnemyPools`, 실제 소환은 `IdleBattle.SpawnEnemies → EnemyFactory.GroupSummon → ObjectPool.TryGet → Enemy.Init`으로 이어집니다. 그다음 `Enemy.OnDeath/Die`와 EXP·BattleTextMesh의 반환 지점을 보면 생명주기를 한 바퀴 따라갈 수 있습니다.

풀은 객체의 소유와 대여 상태를 관리하고, 적의 체력·충돌체·이벤트 구독이나 경험치 획득은 개별 객체에서 처리합니다. 현재 사본은 풀이 가득 찼을 때 피해 텍스트를 생략하고, 경험치는 획득 계수를 적용해 즉시 지급하며, 적·장애물은 해당 소환 요청의 남은 생성을 중단합니다.

## 3. 서버 데이터 처리 — ServerSkillData

서버의 스킬 정보를 조회한 뒤, 전투 코드에서 스킬 코드로 레벨을 찾을 수 있는 구조로 바꾸는 과정입니다. 통신 처리와 스킬 API, JSON 변환, 데이터 보관을 나누었습니다.

| 파일 | 담고 있는 내용 |
| --- | --- |
| [ServerRequester](Samples/ServerSkillData/ServerRequester.cs) | UnityWebRequest 기반 요청, 인증 헤더, 요청 잠금·타임아웃, 결과 분류와 정리. HTTPS 주소 검사와 리디렉션 제한 포함 |
| [ISkillDAO](Samples/ServerSkillData/ISkillDAO.cs) / [SkillDAO](Samples/ServerSkillData/SkillDAO.cs) | 전체 스킬 조회·강화·신규 획득 API. 강화 응답에서 스킬 정보와 스킬북 재화 반영 |
| [SkillMapper](Samples/ServerSkillData/SkillMapper.cs) | 강화 요청의 JSON 직렬화와 액티브·패시브·조합 스킬 응답의 모델 변환 |
| [SkillsDataContainer](Samples/ServerSkillData/SkillsDataContainer.cs) | 초기 조회 결과를 SkillLvData 배열과 SkillStat으로 구성해 보관 |
| [StaticDataInitGuard](Samples/ServerSkillData/StaticDataInitGuard.cs) | 필수 데이터의 요청·적용 실패 기록, 초기화 성공 여부 확인, 필수 리소스 로드 검사 |

조회는 `SkillsDataContainer.Init → SkillDAO.GetAllSkillData → ServerRequester` 순서로 시작합니다. 응답은 `SkillMapper → IntegratedSkillModel → SkillLvData → SkillStat.skillLevels`로 정리되며, 스킬의 `LoadSkillData`와 `UpdateDMG`에서 저장된 레벨을 읽습니다.

서버 주소는 예시 주소로 치환했으며 인증 초기화와 서버 구현은 포함하지 않습니다. API별 콜백 계약과 필수 JSON 필드에 대한 전제가 남아 있어, 이 코드만으로 실제 서버 연동을 검증할 수는 없습니다.

## 공통 코드 — Shared

세 사례를 이해하는 데 필요한 전투 기반 코드와 데이터 정의입니다. 스탯 계산 코드는 포함하지만 스탯 화면 연결 서비스는 포함하지 않습니다.

| 위치 | 주요 파일과 역할 |
| --- | --- |
| `Shared/Battle` | [Battle](Samples/Shared/Battle/Battle.cs)의 전투 준비·종료 공통 순서, [Chara](Samples/Shared/Battle/Chara.cs)의 상태·스킬·피해·경험치 처리, [CharaBattleStat](Samples/Shared/Battle/CharaBattleStat.cs)의 전투 보정값. [IAnimator](Samples/Shared/Battle/IAnimator.cs)·[AnimationAdapter](Samples/Shared/Battle/AnimationAdapter.cs)는 애니메이션 호출을 연결하며, CharaMovement는 대기·이동 상태를 정의 |
| `Shared/Core` | [EventManager](Samples/Shared/Core/EventManager.cs)·[BattleEventManager](Samples/Shared/Core/BattleEventManager.cs)의 알림, [StatsDataContainer](Samples/Shared/Core/StatsDataContainer.cs)의 스탯 보관과 CharaStat 수명 관리. [big](Samples/Shared/Core/big.cs)는 가수·지수 기반 수치 타입, [SerializableDictionary](Samples/Shared/Core/SerializableDictionary.cs)는 Unity 직렬화용 딕셔너리. IGameInit·IListener·RequestResult는 공통 계약과 결과 정의 |
| `Shared/Interfaces` | [IHittable](Samples/Shared/Interfaces/IHittable.cs)의 피격 계약, [IMonster](Samples/Shared/Interfaces/IMonster.cs)의 공격력 계약. 나머지 5개는 전투 정지·종료·보스 처치·캐릭터 정보·스탯 변경 알림의 수신 계약 |
| `Shared/Models` | 스킬·성장·스테이지·보상·서버 요청/응답 모델과 열거형 23개 |

모델은 사용하는 목적에 따라 다음과 같이 묶어 볼 수 있습니다.

- **스킬 데이터**: [SkillAssetData](Samples/Shared/Models/SkillAssetData.cs)는 설명·아이콘·조합 코드·수치 에셋 참조, [SkillBattleData](Samples/Shared/Models/SkillBattleData.cs)는 전투 중 등급과 표시 정보, [SkillStat](Samples/Shared/Models/SkillStat.cs)·SkillLvData는 보유 스킬 레벨을 담습니다. SkillDamageTable·SkillValueTableSO는 레벨·등급별 피해량 테이블 구조이며, SkillType은 스킬 종류입니다.
- **성장과 피해**: [CharaStat](Samples/Shared/Models/CharaStat.cs), BaseStat·SpecialStat·AwakeningStat·ArtifactStat은 성장 수치와 캐릭터 스탯 계산에 사용합니다. Damage는 피해량과 치명타 여부, EnemyInfo는 적의 기본 정보를 담습니다.
- **스테이지와 통신**: [StageData](Samples/Shared/Models/StageData.cs)·BattleTimelineSO·SpawnEventData는 스테이지 설정과 소환 일정, Reward·StageResultState는 보상과 결과 상태입니다. [SkillNetworkModels](Samples/Shared/Models/SkillNetworkModels.cs)·[SelectedRequestModels](Samples/Shared/Models/SelectedRequestModels.cs)는 서버와 주고받는 모델이며, BattleEnums·EnumCollection에는 관련 상태와 종류를 정의했습니다.

## 실행 환경과 제외한 구성

개발 환경은 **Unity 6000.3.9f1 / C#**입니다. 스킬 프리팹과 수치 에셋, 씬·이미지·음원·폰트, 입력·전역 관리자와 초기화 진입점, 인증 구성과 서버 구현은 제외했습니다. ScriptableObject의 클래스 정의는 있지만 실제 수치 에셋은 들어 있지 않습니다.

코드에 등장하는 보스·장애물·화면·에셋 로더 등 일부 타입도 호출 문맥만 남아 있습니다. Newtonsoft JSON, TextMeshPro, HeroEditor, FantasyMonsters 등 외부 패키지·에셋의 역할과 제외한 프로젝트 타입은 [의존성 문서](Docs/Dependencies.md)에 정리했습니다.

`Docs`에는 [스킬](Docs/CombatSkills.md), [풀링](Docs/BattlePooling.md), [서버 데이터](Docs/ServerSkillData.md)의 구현 설명과 의존성 문서가 있습니다. 각 사례의 배경과 적용 범위, 현재 한계는 해당 문서에서 더 자세히 다룹니다. 발췌한 코드에는 공개를 위해 정리하면서 보완한 내용도 포함되어 있습니다.
