# Science Bug Rider

Unity 기반 곤충 탑승 게임입니다.  
플레이어는 다양한 곤충을 타고 장애물을 피하며 목표 지점에 도달해야 합니다.

---

## 프로젝트 환경

- **Unity 버전**: 2022.x 이상
- **시작 씬**: `StartScene`, `Stage1`
- **조작법**:
  - `WASD`: 이동
  - `E`: 벌레 탑승/하차
  - `F`: 벌레 호출
  - `Space`: 무당벌레 비행 및 상승

---

## 코드 구조

### Player
- `PlayerMovement.cs`: 플레이어 조작, 탑승/하차
- `FollowingCamera.cs`: 플레이어를 따라가는 3인칭 카메라
- `KillZoneTrigger.cs`: 낙사 시 리스폰 처리

### Bugs
- `BugMovement.cs`: 기본 벌레 이동
- `AntMovement.cs`: 대시 기능 포함 개미
- `LadybugMovement.cs`: 비행 가능한 무당벌레
- `BugFlightMovement.cs`: 비행 전용 벌레 이동 처리

###  전략 패턴
- `IBugMovementStrategy.cs`: 이동 전략 인터페이스
- `WalkMovementStrategy.cs`: 걷기 전략 구현
- `FlyMovementStrategy.cs`: 비행 전략 구현

###  인터페이스
- `IRideableBug.cs`: 탑승 가능한 벌레 인터페이스

---

##  주요 기능

- 전략 패턴으로 걷기/비행 분리
- 대시, 비행, 낙사 처리
- UI 연동 (대시/비행 쿨타임 안내)
- 트리거 기반 리스폰 처리

---

##  수정 및 개선이 필요한 항목

- [ ] `S` 키로 후진 시 간헐적으로 카메라 흔들림 / 방향 문제 있음
- [ ] 전진할 때 간헐적으로 방향 이상해짐
- [ ] 전진 시 카메라 미세 흔들림 있음
- [ ] 개미 탑승 시 방향 어색함 (탑승 위치/방향 재조정 필요)
- [ ] Tag 및 Layer가 Ground로 설정되어 있어도 비행 착지 시 땅 뚫는 버그 있음
- [ ] 무당벌레 강제 하강 후 카메라 계속 회전함
- [ ] 무당벌레 착지 후 탑승이 풀림

---

##  참고 사항

- 모든 스크립트에 한글+영문 병기의 주석 작성 원칙 유지
- 추후 Git 연동 시 `README.md`는 루트에서 자동 인식됨
