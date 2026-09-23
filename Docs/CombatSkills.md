# 등급에 따라 달라지는 스킬 실행

[README로 돌아가기](../README.md)

전투 중 스킬을 처음 얻으면 생성하고, 같은 스킬을 다시 선택하면 등급에 따라 공격 방식이 달라집니다. 이 사례에는 공통 초기화와 피해량 계산, 등급별 동작 선택, 두 스킬의 공격·투사체 구현을 담았습니다.

서버에 저장되는 **스킬 레벨**(Level)과 전투 중 올라가는 **등급**(Grade)을 구분했습니다. 레벨과 등급은 피해량 계산에 함께 사용하고, 공격 횟수·크기·투사체 수와 이동 방식은 각 스킬에서 처리합니다.

## 파일과 읽는 순서

| 순서 | 파일 | 역할 |
| --- | --- | --- |
| 1 | [CharaBuildSvc](../Samples/CombatSkills/CharaBuildSvc.cs) | 캐릭터 생성과 전투 모드별 시작 스킬 구성, Chara.UpdateSkill 호출 |
| 2 | [Chara](../Samples/Shared/Battle/Chara.cs) | 종류별 스킬 보관, 신규 스킬의 프리팹 생성·초기화, 기존 스킬의 LevelUp 호출 |
| 3 | [Skill](../Samples/CombatSkills/Base/Skill.cs) | 스킬 코드 설정, 에셋·서버 레벨·피해량 테이블 연결, 전투 데이터 초기화 |
| 4 | [ActiveSkill](../Samples/CombatSkills/Base/ActiveSkill.cs) | 공격 스킬의 피해량 갱신, 스탯 변경·전투 정지 이벤트 연결, 실행 코루틴 시작 |
| 5 | [NormalSkill](../Samples/CombatSkills/Base/NormalSkill.cs) | 1~4등급의 동작 선택, 등급 상승 시 기존 동작 정리와 재시작 |
| 6 | [Skill1140001](../Samples/CombatSkills/Examples/Skill1140001.cs) / [SkillProj1140001](../Samples/CombatSkills/Examples/SkillProj1140001.cs) | 망치 공격의 방향·연속 타격·애니메이션 대기와 충돌·파티클 처리 |
| 7 | [Skill1140002](../Samples/CombatSkills/Examples/Skill1140002.cs) / [SkillProj1140002](../Samples/CombatSkills/Examples/SkillProj1140002.cs) | 등급별 투사체 활성화·속도 설정, 화면 밖에서 타깃 방향으로 이동 |
| 8 | [SkillProjectile](../Samples/CombatSkills/Base/SkillProjectile.cs) / [IHittable](../Samples/Shared/Interfaces/IHittable.cs) | 공통 충돌 진입점·타깃 선택과 피격 대상의 계약 |

## 생성부터 공격까지

CharaBuildSvc가 시작 스킬을 선택하면 Chara.UpdateSkill이 종류와 등급에 따라 새 스킬을 만들거나 기존 스킬의 LevelUp을 호출합니다. 새 스킬의 실행은 다음 순서로 이어집니다.

```text
Chara.UpdateSkill
  → Skill.Init: 코드·데이터 연결
  → ActiveSkill.OnInit: 개별 초기화·피해량 계산·코루틴 시작
  → NormalSkill.SkillAction: 현재 등급의 GradeAction 선택
  → Skill1140001 / Skill1140002: 실제 공격 동작
```

두 스킬은 NormalSkill의 GradeIAction~GradeIVAction을 재정의합니다. 같은 초기화·등급 상승 진입점을 사용하면서, 선택되는 공격 동작은 각 스킬에서 달라집니다. LevelUp에서는 등급을 올리고 피해량을 갱신한 뒤 ResetSkillAction으로 기존 동작을 정리하고 다시 시작합니다.

피격은 `SkillProjectile의 트리거 처리 → 개별 OnObjHit → IHittable.Hit`으로 연결됩니다. 두 투사체는 ActiveSkill의 DMG를 전달하며, 피해를 받는 일반 적의 구현은 [Enemy](../Samples/BattlePooling/Enemy.cs)에 있습니다.

## 두 스킬의 차이

**망치 공격(1140001)**은 캐릭터가 바라보는 방향에 맞춰 공격 위치와 파티클 방향을 바꿉니다. 1등급은 한 번, 2등급은 양쪽으로 두 번 공격합니다. 3등급에서는 크기를 키우고, 4등급에서는 세 번째 타격을 이어갑니다. 스킬은 공격 순서와 대기 시간을, 투사체는 충돌체 활성 시간과 피격 호출을 담당합니다.

**목표 방향으로 이동하는 공격(1140002)**은 화면 밖의 임의 위치에서 타깃을 향해 이동합니다. 1등급은 투사체 하나, 2등급은 둘을 활성화하고, 3등급은 기존 두 투사체의 속도를 높입니다. 4등급에서는 세 번째 투사체를 추가합니다. 이동과 반복 간격은 SkillProj1140002의 코루틴에서 처리합니다.

## 공통 데이터와 개별 동작의 경계

[SkillLvData](../Samples/Shared/Models/SkillLvData.cs)는 보유 스킬 레벨을, [SkillBattleData](../Samples/Shared/Models/SkillBattleData.cs)는 전투 중 등급과 표시 정보를 담습니다. ActiveSkill.UpdateDMG는 [SkillDamageTable](../Samples/Shared/Models/SkillDamageTable.cs)을 레벨·등급으로 조회하고, 반환된 비율을 캐릭터 공격력에 적용합니다.

[SkillAssetData](../Samples/Shared/Models/SkillAssetData.cs)와 [SkillValueTableSO](../Samples/Shared/Models/SkillValueTableSO.cs)는 설명·아이콘·조합 코드와 수치 에셋을 연결하는 구조입니다. 클래스 정의만 담았으며 실제 에셋과 수치 데이터는 제외했습니다.

[PassiveSkill](../Samples/CombatSkills/Base/PassiveSkill.cs)과 [CombinationSkill](../Samples/CombatSkills/Base/CombinationSkill.cs)도 Chara의 스킬 관리 흐름을 읽을 수 있도록 함께 담았습니다. 전자는 등급에 따른 효과 적용, 후자는 조합 스킬의 실행을 위한 부모 클래스이며 개별 구현 전체를 포함하지는 않습니다.

## 적용 범위와 한계

이 사례에서는 두 일반 스킬이 초기화·피해량 계산·등급 상승을 공유하고, 공격 동작과 투사체 처리를 나누는 구조를 볼 수 있습니다. 캐릭터의 접촉 피해는 CanTakeContactDamage에서 진행·정지·사망·무적 상태를 먼저 확인하며, CalculateDamage는 치명타율 0과 1의 경계를 별도로 처리합니다.

- 프리팹의 자식 개수와 컴포넌트, 스킬 코드·레벨·등급에 대응하는 테이블이 유효하다는 전제가 있습니다. 배열·사전 접근 전 검사가 모든 경로에 들어 있지는 않습니다.
- S른 스킬과 조합 대상, 입력·자동 이동·애니메이션 에셋은 제외했습니다. [AnimationAdapter](../Samples/Shared/Battle/AnimationAdapter.cs)는 외부 HeroEditor 타입을 참조합니다.

공