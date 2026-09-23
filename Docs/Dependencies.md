# 코드 구성과 실행에 필요한 의존성

[README로 돌아가기](../README.md)

이 저장소에는 스킬 실행, 전투 오브젝트 관리, 서버 스킬 데이터 처리를 읽는 데 필요한 코드를 담았습니다. 부모 클래스·인터페이스·호출부·모델은 함께 두고, 전체 게임에 걸친 서비스와 에셋은 제외했습니다.

각 사례에 어떤 공통 코드가 쓰이고, 실행하려면 어떤 구성이 더 필요한지 아래에 정리했습니다. C# 파일만 새 Unity 프로젝트에 넣어 컴파일하거나 플레이할 수 있는 구성은 아닙니다.

## 포함한 공통 코드

- `Shared/Battle`: 캐릭터의 상태·스킬·피해 처리, 전투 보정값, 애니메이션 연결과 전투 부모 클래스.
- `Shared/Models`: 스킬·피해량·적·스테이지·보상·타임라인·요청/응답 모델과 열거형.
- `Shared/Core`: 이벤트 관리자, 초기화 계약, 수치 타입 `big`, `SerializableDictionary`, 요청 결과.
- `Shared/Interfaces`: 피격·몬스터 계약과 전투/스탯 이벤트 리스너.

여러 사례가 사용하는 공통 정의는 한 곳에서 참조합니다. `big`는 가수·지수로 성장 수치를 표현하는 타입으로, 임의 정밀도 수학 라이브러리는 아닙니다.
## 제외한 프로젝트 코드

| 타입 또는 묶음 | 프로젝트에서의 역할 | 제외 범위와 연결 지점 |
| --- | --- | --- |
| `BattleManager`, `BattleDataContainer` | 전투 상태·생존 목록·타이머·정지 대기·선택 가능 스킬 관리 | 모든 전투 모드와 UI로 범위가 확대됨. `Battle`, `Chara`, 스킬, 적이 참조. 열거형만 별도 포함 |
| `NetworkManager`, `DummySkillDAO` | 인증 토큰, 요청기/DAO 조립, 환경별 초기화 | 접속·인증 설정과 전체 DAO 조합 제외. 요청기·컨테이너의 진입 의존성 |
| `GameManager`, `InputManager`, `Autopilot` | 프레임 보정값, 입력 방향, 자동 이동 | 입력/자동 플레이 전체 구현은 사례 범위 밖 |
| `ClientSkillsDataContainer`, `BattleAssetContainer` | 스킬 프리팹·수치 에셋 및 전투 에셋 조회 | 실제 에셋 레지스트리 제외. 스킬 생성/초기화에서 필요 |
| `ClientDataContainer`, `ClientAssetContainer`, `ClientMainAssetContainer` | 캐릭터·스프라이트·공용 에셋 참조 | 프리팹·화면 구성 등 전체 에셋 의존성 제외 |
| `ClientContentsDataContainer`, `ContentsDataContainer` | 스테이지·타임라인·레이드 스킬 정보 | 전체 콘텐츠 데이터와 서버 초기화 제외 |
| `ClientStatsDataContainer`, `ClientItemsDataContainer`, `DataContainer` | 성장 수치·장비·재화·캐릭터 스탯 보관 | 성장 시스템 전체와 UI 연결 범위 제외. 스탯 계산·DAO 재화 갱신에서 참조 |
| `AssetLoadManager`, `IAssetLoadManager` | 에셋 로딩 추상화 | 로더 전체 구현 제외. `Enemy`의 기존 참조 유지 |
| `AudioManager`, `EffectController`, `RewardEffectSpawner`, `MapBGControlManager` | 소리·전환/보상 연출·배경 | 외부 에셋과 연출 구성 제외 |
| `IdleUIView`, `SurviveUIView`, `RaidUIView` | 모드별 전투 화면 | `Battle`의 화면 참조만 보존. 화면 전체 구현 제외 |
| `Boss`, `RaidBoss`, `Obstacle` | 보스·장애물 전용 구현 | `EnemyFactory`의 주변 분기. 일반 적 풀링 사례의 범위 밖 |
| `OnValueChanged` | 재화 등 값 변경 이벤트 리스너 | `EventManager`의 주변 알림 계약. 사례에 필요한 전투 리스너만 포함 |

stat/item/skill/gacha의 UI 연결 서비스에는 다른 개발자가 담당한 부분이 많아 `StatUpgradeSvc`, `ItemViewSVC`, `SkillViewSVC`, `DrawSVC` 등은 소개 범위에서 제외했습니다. CharaBuildSvc가 참조하는 다른 스킬과 패시브·조합 스킬의 개별 구현도 모두 담지는 않았습니다.

## 외부 패키지·에셋

아래 버전은 원본 프로젝트의 설정과 패키지 파일을 기준으로 적었습니다. 패키지 소스와 에셋은 저장소에 포함하지 않으며, 표의 버전이 전체 게임의 빌드 검증을 의미하지는 않습니다. 별도 풀 검사에는 Unity 6000.3.9f1과 내장 JSON 직렬화 모듈 1.0.0을 사용했습니다.

| 항목 | 버전 / 설정 파일 | 이 샘플과의 관계 |
| --- | --- | --- |
| Unity | `6000.3.9f1` / `ProjectSettings/ProjectVersion.txt` | MonoBehaviour, 코루틴, UnityWebRequest, 물리·애니메이션 등 |
| Newtonsoft JSON | `3.2.2` / manifest·lock | `SkillMapper`와 `SkillDAO`의 JSON 처리 |
| UGUI | `2.0.0` / manifest·lock | 포함된 전투/캐릭터 코드의 UI 타입 |
| Collections | `2.6.2` / lock | 투사체 코드에 관련 using 유지 |
| Visual Scripting | `1.9.9` / manifest·lock | 원본에 존재하는 관련 using 유지 |
| TextMeshPro | 별도 패키지 버전 미확인 | `BattleTextMesh`의 TMP 타입. 텍스트·폰트 에셋 제외 |
| Google Mobile Ads | `8.7.0` / 플러그인 manifest 파일명 | 관련 using이 남아 있으나 광고 SDK·설정은 제외 |
| HeroEditor / FantasyMonsters | 버전 미확인 | `AnimationAdapter`·`Chara` / `Enemy`가 외부 타입 참조. 외부 코드와 유료 에셋 제외 |
| JetBrains.Annotations | 별도 버전 미확인 | 모델 원본의 using 유지 |

원본에는 Addressables `2.8.1`, Purchasing `5.0.2`도 사용 설정이 있지만, 다운로드·결제 기능은 이 저장소의 소개 범위에서 제외했습니다. 스킬 컨테이너에는 미사용 UnityEditor 참조가 없으며, 다른 플랫폼 관련 using과 전체 빌드 호환성은 별도 확인이 필요합니다.


## 스탯 보관과 이벤트 구독의 수명

[StatsDataContainer](../Samples/Shared/Core/StatsDataContainer.cs)는 서버에서 읽은 성장 데이터를 보관하고 [CharaStat](../Samples/Shared/Models/CharaStat.cs)을 생성합니다. BaseStat·SpecialStat·AwakeningStat·ArtifactStat도 이 초기화와 스탯 계산을 따라갈 수 있도록 함께 담았습니다.

CharaStat은 스탯 변경 이벤트를 구독합니다. 현재 구현에서는 StatsDataContainer가 기존 인스턴스를 교체하거나 자신이 파괴될 때 Dispose를 호출해 구독을 해제합니다. 수명을 관리하는 쪽과 실제 구독 해제 코드를 함께 읽을 수 있는 부분입니다.

