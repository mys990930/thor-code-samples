# 서버 스킬 데이터를 전투 데이터로 초기화

[README로 돌아가기](../README.md)

전투에서 스킬 피해량을 계산하려면 서버에 저장된 보유 스킬 레벨을 먼저 읽어야 합니다. 이 사례에는 액티브·패시브·조합 스킬을 조회하고, 응답을 코드별 레벨 데이터로 바꿔 전투에 연결하는 과정을 담았습니다.

요청과 통신 결과 처리는 요청기에, 스킬 API와 JSON 변환은 DAO·매퍼에, 게임에서 사용하는 데이터 구성은 컨테이너에 나누었습니다. 스킬 강화·뽑기 화면의 UI 연결은 소개 범위에 포함하지 않습니다.

## 파일과 읽는 순서

| 순서 | 파일 | 역할 |
| --- | --- | --- |
| 1 | [SkillsDataContainer](../Samples/ServerSkillData/SkillsDataContainer.cs) | 초기 조회 결과로 레벨 배열을 만들고 성공 확인 후 SkillStat 구성 |
| 2 | [ISkillDAO](../Samples/ServerSkillData/ISkillDAO.cs) / [SkillDAO](../Samples/ServerSkillData/SkillDAO.cs) | 조회·강화·신규 획득 API의 계약, 요청 경로와 매퍼 선택 |
| 3 | [ServerRequester](../Samples/ServerSkillData/ServerRequester.cs) | UnityWebRequest 생성·전송, 요청의 순차 실행, 결과 분류·콜백·정리 |
| 4 | [SkillMapper](../Samples/ServerSkillData/SkillMapper.cs) | 요청 JSON 직렬화, actives·passives·combis 응답 배열과 강화 응답의 모델 변환 |
| 5 | [SkillNetworkModels](../Samples/Shared/Models/SkillNetworkModels.cs) | 통합 응답과 코드·인덱스·레벨, 강화 결과 모델 |
| 6 | [StaticDataInitGuard](../Samples/ServerSkillData/StaticDataInitGuard.cs) / [RequestResult](../Samples/Shared/Core/RequestResult.cs) | 필수 데이터 초기화의 실패 상태와 요청 결과 정의 |
| 7 | [SkillStat](../Samples/Shared/Models/SkillStat.cs) / [SkillLvData](../Samples/Shared/Models/SkillLvData.cs) | 종류별 레벨 배열과 스킬 코드별 조회 사전 |

## 조회 결과가 전투에 연결되는 과정

1. SkillsDataContainer.Init이 SkillDAO.GetAllSkillData를 호출합니다.
2. SkillDAO는 `/skills/all` 경로와 SkillMapper.MapIntegratedSkill을 요청기에 전달합니다.
3. ServerRequester가 요청하고, 응답 JSON을 매퍼에서 IntegratedSkillModel로 변환합니다.
4. 컨테이너의 콜백에서 액티브·패시브·조합 스킬을 각각 SkillLvData 배열로 만듭니다.
5. StaticDataInitGuard로 성공 여부를 확인한 뒤 SkillStat을 구성합니다. 종류별 배열과 코드별 skillLevels 사전을 함께 보관합니다.
6. 전투에서는 [Skill.LoadSkillData](../Samples/CombatSkills/Base/Skill.cs)와 [ActiveSkill.UpdateDMG](../Samples/CombatSkills/Base/ActiveSkill.cs)가 저장된 레벨을 읽어 피해량 테이블에 사용합니다.

서버 응답 모델을 전투 코드에서 매번 직접 해석하지 않고, 초기화 단계에서 게임이 사용하는 조회 구조로 바꾸는 흐름입니다.

## 요청기와 스킬 API의 경계

ServerRequester는 정적 잠금으로 한 번에 하나의 요청을 처리합니다. 전송 타임아웃, 인증 헤더, 응답 코드와 통신 상태에 따른 결과 분류를 공통으로 처리하고, 요청 폐기와 잠금 해제는 finally에서 수행합니다. 전체 스킬 조회에 사용하는 제네릭 GET 경로는 매핑 예외도 실패 결과로 전달합니다.

SkillDAO는 스킬별 API 경로와 사용할 매퍼를 선택합니다. 클래스의 역할을 함께 볼 수 있도록 UpgradeSkill과 GetNewSkill, 관련 [요청 모델](../Samples/Shared/Models/SelectedRequestModels.cs)도 담았습니다. 강화 응답에서는 스킬 정보와 남은 스킬북 재화를 반영하지만, 화면에서 강화 버튼을 연결하거나 뽑기 결과를 표시하는 서비스는 포함하지 않습니다.

SkillsDataContainer는 통신 세부 사항보다 레벨 배열과 SkillStat을 만드는 일을 담당합니다. StaticDataInitGuard는 필수 데이터 요청이나 적용 중의 실패를 보관하고 초기화를 계속할지 판단하는 공통 코드입니다.

요청기와 DAO를 조립하는 NetworkManager는 제외했습니다. 원본에서는 요청기를 AddComponent로 생성하고 실행 모드에 따라 SkillDAO 또는 DummySkillDAO를 선택합니다. 샘플만 실행하기 위한 대체 초기화 코드는 두지 않았습니다.

## 접속 설정과 오류 처리

실제 서버 주소는 `https://example.invalid`로 치환했습니다. 인증 토큰·실제 응답·사용자 데이터와 서버 구현은 포함하지 않습니다.

현재 요청기는 실제 URL이 HTTPS이고 주소 안에 자격정보가 없는 경우에만 전송합니다. 이 조건을 만족하지 않으면 NETWORK_ERROR 결과로 처리한 뒤 요청을 폐기하고 잠금을 해제합니다. 성공 전용 콜백 오버로드는 기존 계약대로 실패 시 호출하지 않습니다.

인증서 검증은 Unity 기본 동작을 사용하고 자동 리디렉션은 차단합니다. 서비스에 연결하려면 최종 HTTPS 주소를 직접 설정해야 합니다. 로그에는 메서드·결과·응답 코드와 잠금 대기·실패 횟수 등을 남기며, 응답 본문·URI·쿼리·원시 오류 메시지는 출력하지 않습니다.

## 적용 범위와 한계

이 사례는 초기 스킬 조회부터 전투에서 레벨을 참조하는 구조까지를 포함합니다. 전체 인증·서버 초기화·성장 UI를 재현하는 구성은 아닙니다.

- 전송 타임아웃은 15초이고 잠금 대기는 5초부터 경고합니다. 잠금 대기 자체의 종료 제한, 공정한 요청 큐나 자동 재시도는 구현되어 있지 않습니다.
- 실패 콜백 계약은 오버로드마다 다릅니다. 문자열 응답만 받는 일부 경로와 GetNewSkill은 성공할 때만 콜백을 호출합니다.
- StaticDataInitGuard는 전역 실패 상태를 사용하므로 초기화 시작 시 외부에서 Reset해야 합니다. 일부 데이터를 적용한 뒤 실패해도 이전 상태로 되돌리지는 않습니다.
- JSON 필수 필드와 응답 구조가 맞다는 전제가 있습니다. 실제 서버와의 TLS·인증·리디렉션 호환성은 검증하지 않았습니다.


