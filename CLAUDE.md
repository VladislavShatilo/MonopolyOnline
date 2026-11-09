# MonopolyOnline

Онлайн-мультиплеерная настольная игра в стиле «Монополии» (поле из брендов/компаний) на Unity. Игроки заходят в комнату через Photon, ходят по очереди, бросают кубики, покупают компании, платят ренту, торгуются, закладывают имущество, берут кредиты и т.д.

## Стек

- **Unity 6000.4.6f1** (Unity 6), 2D/UGUI + 3D-кубики. Рендер-пакеты — см. `Packages/manifest.json`.
- **Photon PUN2** (`Assets/Photon`) — сеть, комнаты, RPC. Также PhotonChat (демо-код в `Assets/Photon/PhotonChat`).
- **Zenject** (`Assets/Plugins/Zenject`) — DI. Вся архитектура строится вокруг него.
- **DOTween** (`Assets/Plugins/Demigiant`) — анимации UI/окон.
- **TextMeshPro**, Addressables, Timeline (подключены пакетами, в игровом коде почти не используются).
- Тесты: NUnit + **Moq** (`Assets/Plugins/Tests/Moq.dll`, `Castle.Core.dll`).
- В пакетах есть `com.unity.ai.assistant` и MCP-интеграция `unity-mcp` (инструменты `mcp__unity-mcp__*`: консоль, скриншоты сцены/камеры, RunCommand).

## Сцены (порядок в Build Settings)

1. `ConnectToServer` — `ConnectToServer.cs`: `PhotonNetwork.ConnectUsingSettings()` + выбор региона, после `OnConnectedToMaster` грузит `AutorizationScene`.
2. `AutorizationScene` — `AuthUI`, `AuthInstaller`: ввод ника/пароля, `AuthService.Login` ставит `PhotonNetwork.NickName` (пароль нигде не проверяется — это не настоящая авторизация), далее `LobbyScene`.
3. `LobbyScene` — `LobbyManager`, `LobbyUI`, `RoomPopup`, `LobbyInstaller`: список комнат, создание комнаты (`MaxPlayers = 2` захардкожено в `LobbyManager.OnCreateRoomClicked`), вход → `PhotonNetwork.LoadLevel("GameScene")`.
4. `GameScene` — сама игра (`GameplayInstaller` — SceneContext).

Редактор: `Assets/Editor/MainSceneAutoLoader.cs` при нажатии Play всегда открывает `ConnectToServer` (первую сцену), а после выхода возвращает прежнюю. Учитывать при запуске/тестировании конкретной сцены.
`Assets/_Recovery/0.unity` — автосохранение Unity, мусор, не использовать.

## Архитектура

Код в `Assets/Scripts` (сборка `GameAssembly.asmdef`, ~6800 строк, плоская структура без пространств имён). Слои:

- **Interface/** — все интерфейсы `I*` (~75 штук). Любая зависимость внедряется через интерфейс.
- **Services/** — `BoardService`, `CellHandlerService`, `ChanceService`, `CompanyService`, `CompanyUIService`, `JailService`, `LoanService`, `TurnService`. Корневая корпоративная игровая логика (без Unity-зависимостей там, где возможно).
- **Корень Scripts/** — остальные сервисы (`BankService`, `AuctionService`, `TradeService`, `MortgageService`, `BranchService`, `LapMoneyService`, …), UseCase-классы (`RollDiceUseCase`, `PlayerMoveUseCase`, `AuctionUseCase`, `BranchUseCase`, `SendChatMessageUseCase`, `TurnCompanyUIUseCase`), презентеры (`*Presenter`), репозитории (`PlayerRepository`, `CompanyRepository`, `BoardRepository`, `UICompanyCellRepository`), модели (`PlayerData`, `Company`, `Turn`, `TradeOffer`, `ChanceBuff`, `DiceResult`).
- **Photon*-менеджеры** (`PhotonTurnManager`, `PhotonCompanyManager`, `PhotonAuctionManager`, `PhotonTradeManager`, `PhotonJailManager`, `PhotonLoanManager`, `PhotonMortgageManager`, `PhotonBranchManager`, `PhotonChanceManager`, `PhotonBankNotifier`, `PhotonChatService`, `DiceManagerPhoton`, `PhotonPlayerMoveManager`, `PhotonTurnSynchronizer`, `PhotonCompanySyncManager`, `PhotonPlayerSpawner` …) — `MonoBehaviourPun`-компоненты на сцене, тонкая обёртка над RPC. Реализуют `IPhoton*Manager` и вызывают сервисы.
- **UI/** — `Windows/` (Buy, PayRent, Auction, Jail, RansomJail, LoanPay, Turn; база `UIWindowBase`), `Trade/`, `Cells/` (клетки поля), `CompanyStats/`, `UIPlayerStats`. UI реализует `I*Window`-интерфейсы, презентеры работают только с интерфейсами (MVP).
- **LobbyAndConnection/** — подключение, авторизация, лобби.
- **TestsHelpers/** — `TestUI*`-заглушки окон для тестов.

### Обёртки над статикой
`IPhotonNetworkWrapper` (`PhotonNetwork.*`: IsMasterClient и т.п.) и `IPhotonViewWrapper` (`RPC(photonView, name, target, args)`) — **всегда** использовать их вместо прямых вызовов `PhotonNetwork`/`photonView.RPC` в тестируемом коде, чтобы можно было мокать в EditMode-тестах. Исключения в существующем коде: `GameManager.Initialize`, `GameStarter`, `AuthService`, лобби.

### Event bus
`EventBus.cs` (`IEventBus`: `Subscribe<T>/Unsubscribe<T>/Publish<T>`) — ключевой механизм связи между сервисами/презентерами/UI. Все типы событий объявлены в том же файле (`EventBus.cs`, ~550 строк): `HandleCellEvent`, `MovePlayerEvent`, `DiceRolledEvent`, `OfferPurchaseEvent`, `StartAuctionEvent`, `EndAuctionWithWinnerEvent`, `CompanyMortgagedEvent`, `OnUpdatePlayerMoneyEvent`, `MoveToJailEvent`, `LapMoneyEvent`, … Новое событие — добавлять туда же.
Подписка делается через `IInitializable.Initialize()`, отписка через `IDisposable.Dispose()` (обязательно отписываться).

### Zenject
- `ProjectContext` (`Assets/Resources/ProjectContext.prefab`) → `ProjectInstaller`: только `IAuthService` → `AuthService`.
- Сцены Auth/Lobby: `AuthInstaller`, `LobbyInstaller` (биндят UI-компоненты со сцены).
- `GameplayInstaller` (GameScene) — главный контейнер: ScriptableObject-настройки, все UI-окна, все Photon-менеджеры (`FromInstance` из SerializeField), сервисы (`AsSingle`), презентеры (`BindInterfacesTo<…>().AsSingle().NonLazy()`), ячейки `UICompanyCell` (28 штук, `foreach`-bind). Добавляя новый сервис/окно/менеджер — регистрировать здесь и (для MonoBehaviour) прокинуть `[SerializeField]` на сцене.
- Внедрение методом `[Inject] public void Construct(...)` с `?? throw new ArgumentNullException(...)` — **единый стиль проекта**, конструкторного внедрения почти нет.
- `ZenjectAutoInject.cs` — инъекция в динамически созданные объекты.
- Строковые id биндингов: `"BoardParent"`, `"PlayerRoot"`, `"PlayerSettings"`, `"PlayerStatsPrefab"`, `"PlayerStatsContainer"`.
- `SceneLoaderService`/`FadeService` (+ `Resources/FadeCanvas.prefab`) — загрузка сцен с затемнением (в `ConnectToServer` пока не используется, ветка закомментирована).

### Старт игры
`GameBootstrapper.Awake` → `boardService.InitializeBoard()`, `companyUIService.InitializeUI()`, `cellOccupancyService.InitializePlayer()`, `playerStatsService.Initialize()`, `gameManager.Initialize()` (создаёт `PlayerData` для каждого игрока Photon, публикует `PlayerJoinedEvent`, спавнит локального игрока через `IPlayerSpawner`).
`GameStarter` (`MonoBehaviourPunCallbacks`): когда `PlayerCount == MaxPlayers`, инициализирует `PlayerMove`/`PlayerSkin` игроков (тег `Player`), а мастер-клиент вызывает `RequestStartRandomTurn()`.

### Сетевая модель (важно)
- **Мастер-клиент авторитетный**: клиент шлёт RPC на `RpcTarget.MasterClient` (`RPC_TryBuyCompany`, `RPC_RequestEndTurn`, …), мастер выполняет логику в сервисах и рассылает результат (`RpcTarget.All`). Например, `BankService` меняет `Money` у мастера → `PhotonBankNotifier.NotifyBalanceChanged` → `RPC_UpdateMoney` обновляет `PlayerData` у всех и публикует `OnUpdatePlayerMoneyEvent`.
- Состояние игры (`PlayerRepository`, `CompanyRepository`, `Turn`) хранится локально на каждом клиенте и синхронизируется RPC; никакой общей сериализованной модели нет. Проверки `IsMasterClient` — внутри RPC-методов и сервисов.
- Ход: `TurnService` (мастер) → `TimerManager.StartTurnTimer` + `PhotonTurnSynchronizer.RequestStartTurn`. Дубль → `RegisterDouble` → дополнительный ход. `TurnMode`: Normal / Auction / Trade.
- Чат и системные сообщения: `PhotonChatService.SendMessage(playerId, text, isChat)`; системные (isChat=false) шлёт только мастер. Лог — `MessageLogPresenter`/`MessageLogView`.

## Игровые правила (как реализовано)

- Поле: 6 типов клеток `CellType` (Company, FieldCompany, DiceCompany, Spend, Question, Corner), 10 групп `CompanyGroup` (Perfume, Cars, Clothes, SocialMedia, Games, Drinks, Airlines, FastFood, Hotels, Phones). Углы `CornerType`: Start, ChillJail, Caisno, Police (Police отправляет в тюрьму; Casino пока заглушка).
- Данные поля — `Scripts/BoardConfig.asset` (ScriptableObject `BoardConfig` → список `CellData`). Редактор поля: меню **Monopoly → Build Board** (`BoardGeneratorEditor`), `CellDataEditor` для инспектора. 28 покупаемых клеток (`UICompanyCell[]`).
- Три вида компаний: `Company` (рента по уровням `RentLevel`, ветки/филиалы `BranchPrice`, макс. уровень `maxBranchLevel = 5`), `FieldCompany` (рента зависит от числа владений в группе), `DiceCompany` (рента = множитель × сумма кубиков).
- Настройки — `Scripts/GameSettings.asset` (`GameSettings`): стартовые деньги 100 000, деньги за круг 2 000, залог 7 ходов, таймеры (ход 90 с, аукцион 15 с, обмен 15 с), тюрьма (3 хода, выкуп 500, клетка 10), кредит 5000 → возврат 5500. `PlayerSettings.asset` — префаб игрока и стартовая позиция.
- Механики: бросок кубиков (3D-кубики `DiceManager3D` + `DiceRoll3D`, `Resources/DiceCube.prefab`), покупка/аукцион (`AuctionService/UseCase/Presenter`), рента, залог/выкуп (`MortgageService`), ветки (`BranchService`), обмен между игроками (`TradeService`, `TradeOffer`, `TradeWindowPresenter`, `TradeReviewWindowPresenter`), тюрьма и выкуп (`JailService`, `JailPresenter`), кредиты (`LoanService`, `LoanPayPresenter`), карточки «шанс» (`ChanceService` — колода `ChanceBuff`: ±деньги, Teleport, SkipTurn, ReverseMove, Jail), монеты за круг (`LapMoneyService`), цвета игроков (`UnityPlayerColorService`), подсветка клеток (`CellHighlighterService`), занятость клеток (`CellOccupancyService`).
- `PlayerData`: Id = Photon `ActorNumber`, деньги, клетка, тюрьма, кредит, `OwnedCompanies`, флаги `SkipNextTurn`, `NextMoveBackward`. `VisibleCapital`/`LiquidAssets` реализованы неполно (часть логики закомментирована).

## Тесты

- **EditMode** — `Assets/TestsEdit` (`TestsEdit.asmdef`, ~90 файлов `*Tests.cs`): почти каждый сервис/презентер/менеджер имеет тесты на NUnit + Moq. Photon-менеджеры тестируются через мок `IPhotonViewWrapper`/`IPhotonNetworkWrapper`.
- **PlayMode** — `Assets/Tests/PlayMode` (`PlayMode.asmdef`): UI-компоненты, кубики, перемещение, скины.
- Запуск: Window → General → Test Runner, либо из CLI:
  ```bash
  "<UnityEditor>/Unity.exe" -batchmode -projectPath D:/UnityProjects/MonopolyOnline -runTests -testPlatform EditMode -testResults results.xml
  ```
  (Unity-редактор лучше не открывать параллельно из двух процессов — проект блокируется.)
- Добавляя новый сервис/презентер — добавлять тест рядом по образцу существующих (`TestsEdit/<Name>Tests.cs`), зависимости мокать через Moq по интерфейсам.

## Соглашения кода

- Классы внутри размечены `#region LIFE_CYCLE`, `#region PUBLIC_METHODS`, `#region PRIVATE_METHODS`, `#region RPC`, `#region CALLBACKS` — придерживаться.
- Поля зависимостей — `private`, без `_` префикса, заполняются в `Construct`; null-проверки через `?? throw new ArgumentNullException(nameof(x))`.
- Presenter-ы реализуют `IInitializable, IDisposable` и подписываются на `IEventBus`; логика — в сервисах/UseCase, а не во View.
- RPC-методы называются `RPC_*`, `[PunRPC] private`, внутри — проверка `IsMasterClient` через обёртку, если логика авторитетная.
- Один публичный тип на файл, имя файла = имя типа. Все скрипты лежат в корне `Assets/Scripts` (папки только для Interface, Services, UI, LobbyAndConnection, TestsHelpers).
- Комментарии местами в кодировке Windows‑1251 (в файлах видны «кракозябры»). При правке не перекодировать файлы целиком и сохранять кодировку/BOM, если нет явной задачи это исправить.
- Неиспользуемые/закомментированные куски (казино `ICasinoService`, часть `VisibleCapital`) — заготовки на будущее.

## Структура репозитория

```
Assets/
  Scripts/        игровой код (GameAssembly.asmdef) + BoardConfig/GameSettings/PlayerSettings .asset
  TestsEdit/      EditMode-тесты
  Tests/PlayMode/ PlayMode-тесты
  Editor/         BoardGeneratorEditor, CellDataEditor, MainSceneAutoLoader, CodeLineCounterWindow
  Scenes/         ConnectToServer, AutorizationScene, LobbyScene, GameScene
  Prefabs/        клетки поля, окна статистики, комнаты, сообщения
  Resources/      ProjectContext, Player, DiceCube, FadeCanvas, DOTweenSettings
  Photon/         PUN2 + PhotonChat (сторонний код, не править; в PhotonServerSettings задан AppId)
  Plugins/        Zenject, DOTween (Demigiant), Moq/Castle.Core для тестов
  Animation/, Images/, Materials/, TextMesh Pro/
```

Git: ветка `master`. `Library/`, `Temp/`, `obj/`, `*.csproj`, `.sln` — генерируемые, не редактировать вручную.

## Как вносить изменения (чек-лист)

1. Новая механика: интерфейс в `Interface/` → сервис/UseCase → (если сетевая) `Photon*Manager` с RPC на мастер + интерфейс `IPhoton*Manager` → события в `EventBus.cs` → презентер + `I*Window` + `UI*Window`.
2. Зарегистрировать всё в `GameplayInstaller` и добавить `[SerializeField]`-ссылки на сцене `GameScene` (для MonoBehaviour-компонентов и окон; новый Photon-менеджер требует `PhotonView` на объекте).
3. Написать EditMode-тесты в `TestsEdit`.
4. Проверить компиляцию через `Unity_GetConsoleLogs` (unity-mcp) или в редакторе; по возможности прогнать тесты.
5. Не менять `.unity`/`.prefab`/`.asset` вручную текстом без необходимости — только через редактор/Unity MCP (сериализация YAML хрупкая).

## Известные особенности / риски

- «Авторизация» — только ник, пароль не проверяется и хранится в памяти в открытом виде (`PlayerAuthData`).
- Комнаты на 2 игрока (`MaxPlayers = 2`), игра стартует, только когда комната заполнена.
- Мастер-клиент авторитетный → при его уходе миграция состояния не реализована.
- Много `.meta`-удалений в `Assets/Plugins/Zenject/Source/**/*.csproj.meta` в рабочем дереве — артефакты Unity, не связаны с игровой логикой.
- Папка `Assets/_Recovery` и `Temp.txt` в корне — лишние файлы.
