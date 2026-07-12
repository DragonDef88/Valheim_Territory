# Clan Territory

**Clan Territory** — мод для Valheim, который превращает ward/защитный тотем в центр живой территории: доступ, правила, карта, казна, подготовка работ, автоматическое выравнивание земли, рубка деревьев и интеграция с Guilds.

Проект построен вокруг идеи: **состояние мира хранится отдельно от Unity-объектов**. Runtime находит ward в мире, восстанавливает территории, синхронизирует состояние через ZDO/сервисы и даёт другим системам стабильную основу.

---

## Что уже готово

### Территории и ward runtime

- Автоматическое обнаружение ward в загруженном мире.
- Runtime registry для активных территорий.
- Восстановление территорий после загрузки мира.
- Territory radius на базе ward radius.
- Защита от пересечения территорий.
- Валидация размещения ward.
- События входа и выхода с территории.
- Локализованные сообщения входа/выхода с территории.

### Ward menu

- Удобное меню управления территорией через ward.
- Вкладки:
  - Overview;
  - Ward;
  - Territory;
  - Terraforming.
- Отображение владельца, radius, статуса защиты, правил и доступа.
- Название меню учитывает Guilds-гильдию создателя ward.
- Если guild у ward нет, используется стандартное название территории.
- Переименование территории.
- Управление permitted players.
- Управление правилами территории.

### Правила территории

- Включение/выключение защиты ward.
- Door lock для дверей внутри территории.
- Auto-close для locked doors.
- Защита построек от урона.
- Проверка доступа через owner/permitted/guild.
- Guild members могут пользоваться territory controls, если ward привязан к их guild.

### Treasury и preparation storage

- Виртуальная казна территории.
- Виртуальный сундук подготовки для terraforming/leveling worker.
- Большие stack limits для ресурсов:
  - treasury stack capacity до 9999;
  - fuel/stone slots до 500.
- Хранение содержимого в ZDO ward.
- Открытие контейнеров без обязательного постоянного world chest.
- Автоматическое поглощение подходящих ресурсов с земли и из territory chests.
- Реальные сундуки внутри территории не используются как vacuum, если предметов такого типа в них ещё нет.
- Открытые контейнеры не трогаются.

### Terraforming / leveling worker

- Автоматическое выравнивание земли по высоте ward.
- Отдельный radius работ для terraforming.
- Спиральный scan от ward наружу.
- Spirit marker показывает текущую точку работ.
- Scan больше не сбрасывается постоянно при повторном running RPC.
- Radius change намеренно сбрасывает scan.
- Fuel расходуется через work-meter, а не на каждый frame.
- Stone используется для поднятия земли.
- При срезании земли stone может возвращаться в storage.
- Поддержка hoe/pickaxe/axe из preparation chest.
- Отсутствие hoe/pickaxe больше не останавливает отдельную рубку деревьев.

### Камни, деревья и пни

- Mining rocks внутри территории через Valheim damage path.
- Рубка взрослых деревьев.
- Молодые деревья/Growup не рубятся.
- Рубка TreeLog.
- Рубка stump/stub через Destructible damage path.
- Tree worker отделён от terraforming scan и работает по территории, а не только возле текущей точки terraforming.

### Map markers

- Ward map pins для найденных территорий.
- Название marker может использовать Guilds guild name.
- Если ward привязан к Guilds-гильдии, marker использует Guilds icon через Guilds API.
- Если guild нет, используется стандартный marker.
- Reflection lookup для Minimap pins закеширован.

### Economy

- Guild treasury / clan bank для Guilds-гильдий.
- Счёт гильдии сохраняется отдельно по миру.
- Валюта первого этапа: обычные Valheim `Coins`.
- Участник guild может внести монеты из инвентаря в казну.
- Только лидер guild может снять монеты из казны.
- Снятие создаёт stack монет рядом с игроком.
- territory upkeep из guild treasury.
- Если территория вассальная из-за Biome Dominion, часть upkeep переводится guild-владельцу биома как tribute.
- Лидер guild может переводить монеты между guild treasury.
- Игрок может платить налог территории из инвентаря в казну guild-владельца территории.
- В ward menu добавлена вкладка Economy для status/deposit/withdraw/upkeep/tax/transfer.
- Этот слой является основой для будущих налогов, содержания территорий, рынков и вассальной дани.

Команды:

```text
/cteco status
/cteco deposit <coins>
/cteco withdraw <coins>
/cteco upkeep [coins]
/cteco tax <coins>
/cteco transfer <guildName> <coins>

/cteconomy status
/cteconomy deposit <coins>
/cteconomy withdraw <coins>
/cteconomy upkeep [coins]
/cteconomy tax <coins>
/cteconomy transfer <guildName> <coins>
```

Файл мира:

```text
BepInEx/config/ClanTerritory/worlds/<world>.economy.txt
```

### Biome dominion

- Guilds-гильдия может объявить владение текущим биомом.
- Владение объявляет только лидер guild.
- Биомные правила сохраняются отдельно для мира.
- Поддерживаемые правила первого этапа:
  - biome door lock;
  - biome structure damage protection;
  - biome door auto-close.
- Территории внутри захваченного биома становятся вассальными территориями guild-владельца биома.
- При входе в вассальную территорию показывается локализованное сообщение.
- Локальный ward-владелец/permitted/guild access сохраняет доступ к своей территории, но biome rules дополнительно защищают весь биом.

Команды:

```text
/ctbiome status
/ctbiome claim
/ctbiome release
/ctbiome list
/ctbiome set doorlock on|off
/ctbiome set protection on|off
/ctbiome set autoclose 3-10
```

### Guilds integration

- Подключение к Guilds без compile-time ссылки на `Guilds.dll`.
- Runtime reflection к публичному Guilds API.
- Используется:
  - `API.GetPlayerGuild(Player)`;
  - `API.GetGuild(string)`;
  - `API.GetGuildIcon(Guild)`;
  - `API.GetGuildLeader(Guild)` при необходимости.
- Ward сохраняет:
  - guild id;
  - guild name;
  - guild color.
- Создатель ward, находясь в guild, может привязать ward к своей guild через открытие territory menu.
- Если создатель без guild, guild-привязка очищается.
- Guild members получают доступ к поддерживаемым territory actions.

### Groups integration

- Интеграционный слой Groups уже присутствует.
- Groups остаётся отдельным optional integration path рядом с Guilds.

### Локализация

- Базовая локализация на двух языках:
  - English;
  - Русский.
- Конфиг:

```ini
[Localization]
Language = auto
```

- `auto` определяет язык Valheim-клиента, а не только язык операционной системы.
- Fallback: Valheim Localization → PlayerPrefs → Unity `Application.systemLanguage`.
- Language packs создаются при первом запуске:

```text
BepInEx/plugins/ClanTerritory/Localization/en.txt
BepInEx/plugins/ClanTerritory/Localization/ru.txt
BepInEx/plugins/ClanTerritory/Localization/README.txt
```

- Добавление нового языка:
  1. Скопировать `en.txt` или `ru.txt`.
  2. Переименовать, например, в `de.txt`.
  3. Перевести значения.
  4. Поставить `Localization.Language=de`.
  5. Перезапустить игру.

---

## Установка

1. Установить BepInEx для Valheim.
2. Установить зависимости, которые используются сборкой проекта.
3. Скопировать собранный `ClanTerritory.dll` в папку BepInEx plugins.
4. Запустить игру один раз, чтобы создался конфиг.
5. Настроить параметры в BepInEx config при необходимости.

Рекомендуемая структура:

```text
BepInEx/plugins/ClanTerritory/ClanTerritory.dll
BepInEx/plugins/ClanTerritory/Localization/en.txt
BepInEx/plugins/ClanTerritory/Localization/ru.txt
```

Guilds и Groups являются optional integrations: Clan Territory не должен падать, если эти моды не установлены.

---

## Быстрый тест после установки

1. Зайти в мир.
2. Поставить ward.
3. Открыть territory menu.
4. Проверить map marker.
5. Проверить вход/выход с территории.
6. Включить/выключить protection.
7. Проверить door lock и auto-close.
8. Открыть treasury/preparation.
9. Положить fuel/stone/tools в preparation.
10. Включить terraforming running.
11. Проверить:
    - spirit marker;
    - leveling terrain;
    - mining rocks;
    - chopping trees/logs/stumps.

Для проверки Guilds:

1. Создать или вступить в Guilds-гильдию.
2. Создатель ward открывает territory menu.
3. Ward сохраняет guild id/name/color.
4. Второй участник той же guild проверяет доступ к меню, дверям, treasury/preparation и terraforming.
5. Map marker должен использовать guild name/icon.

Проверить Economy:

1. Убедиться, что персонаж состоит в Guilds-гильдии.
2. Положить Coins в инвентарь.
3. Выполнить `/cteco deposit 10`.
4. Проверить `/cteco status`.
5. Лидером guild выполнить `/cteco withdraw 5`.
6. На территории guild выполнить `/cteco upkeep`.
7. Если есть вторая guild treasury, выполнить `/cteco transfer <guildName> 5`.
8. В ward menu открыть вкладку Economy и проверить кнопки Deposit / Withdraw / Upkeep / Tax / Transfer.
9. Внутри территории выполнить `/cteco tax 5` или кнопку Tax в Economy tab.

---

## Конфигурация

Основные параметры:

```ini
[Territory]
Radius = 100
AllowOverlap = false
DoorAutoCloseSeconds = 5

[Localization]
Language = auto

[Debug]
Enabled = false
```

`Debug.Enabled=true` включает расширенные debug-логи.

---

## Структура проекта

```text
Source/ClanTerritory/
  Config/
  Core/
  Domain/
  Events/
  Features/
    Diagnostics/
    Map/
    Persistence/
    Runtime/
    Territory/
    TerritoryInteraction/
    TerritoryNaming/
    WardDetection/
    WardInteraction/
    WardMenu/
  Integration/
    Guilds/
    Groups/
    Valheim/Harmony/
  Utils/

Docs/
  Research/
  Localization/
```

---

## Текущий статус

```text
Living World Runtime             ✅ готово
Territory registry               ✅ готово
Ward menu                        ✅ готово
Territory rules                  ✅ готово
Treasury / preparation storage   ✅ готово
Terraforming worker              ✅ готово
Tree / rock / stump workers      ✅ готово
Map markers                      ✅ готово
Biome dominion                   ✅ первый этап
Guilds integration               ✅ готово
Groups integration layer         ✅ есть
EN/RU localization               ✅ готово
External language packs          ✅ готово
```

---

## Документация разработки

Исследования и история решений находятся в:

```text
Docs/Research/Valheim/
```

Каждый крупный шаг разработки фиксируется отдельным документом, чтобы было понятно:

- какую проблему решали;
- какие Valheim классы изучались;
- почему выбран текущий подход;
- какие ограничения остались.

---

## Основной принцип

> Мы не управляем случайными Unity-объектами.
>
> Мы управляем состоянием мира.

Clan Territory развивается как модульная gameplay-платформа для территорий, guild-aware правил, persistent storage и автоматизированных world workers в Valheim.



### Guild Diplomacy

Первый слой дипломатии между Guilds guild:

```text
/ctdip list
/ctdip status <guild>
/ctdip ally <guild>
/ctdip enemy <guild>
/ctdip vassal <guild>
/ctdip neutral <guild>
/ctdip set ally|enemy|vassal|neutral <guild>
```

Алиас:

```text
/ctdiplomacy ...
```

Отношения сохраняются отдельно по миру:

```text
BepInEx/config/ClanTerritory/worlds/<world>.diplomacy.txt
```

На этом этапе дипломатия только сохраняет и показывает отношения. Автоматическое влияние на двери, структуру, налоги и доступ будет подключаться отдельными безопасными шагами.

## Ward menu: Clan overview

Если ward привязан к Guilds guild, вкладка Overview показывает строку клана и кнопку `Clan`.

Кнопка `Clan` переключает центральный блок Overview на описание клана, если Guilds API отдаёт description. Если описание недоступно, показывается аккуратная заглушка без ошибки.


## Ward menu: Clan diplomacy

В Clan-панели ward menu теперь показываются дипломатические отношения текущей guild.

Если текущий игрок является leader guild, доступны кнопки:

```text
Ally
Enemy
Vassal
Neutral
```

После нажатия кнопки открывается ввод имени целевой guild. Изменение сохраняется через `DiplomacyService` в world diplomacy file.


## Offline Companions compatibility guard

Clan Territory avoids opening ward menus or virtual territory containers while another inventory/container UI is already open.

This prevents UI/container switching conflicts with companion inventory windows from Offline Companions.


## Plateautem-style terraforming mode

Terraforming worker now uses a faster Plateautem-style flattening profile by default:

```text
- faster spiral sweep;
- immediate one-pass terrain operation;
- larger leveling operation radius;
- stronger but still clamped height delta;
- lower work threshold;
- immediate virtual preparation chest persistence.
```

The worker still respects Clan Territory ownership/access rules, ward radius, configured terraforming radius, fuel, stone, and tool requirements.


## Direct heightmap plane flattening

Terraforming now uses a more Plateautem-like terrain algorithm:

```text
- the worker still scans in safe steps;
- each terrain step writes TerrainComp height deltas toward a target plane;
- the inner area is flattened as a plane;
- only the outer edge uses falloff;
- smooth deltas are cleared in the affected vertices to avoid leftover humps;
- rocks and ore nodes inside the flattening radius are hit before terrain leveling.
```

This is different from vanilla-style repeated `TerrainOp.Level` operations, which can leave overlapping falloff artifacts.


## Virtual container switch compatibility

Clan Territory virtual containers now close/persist when another mod opens a different `InventoryGui` container through `InventoryGui.Show(Container)`.

This improves compatibility with companion inventory mods that switch containers without going through the normal `InventoryGui.Hide()` path.


## InventoryGui.Show signature compatibility fix

The virtual container switch compatibility hook now dynamically finds `InventoryGui.Show` overloads whose first argument is `Container`, instead of requiring the exact `Show(Container)` signature.


## Built-in terraforming removed

Clan Territory no longer runs its own terraforming worker or patches `TerrainComp.LevelTerrain`.

Terrain shaping should be handled by a dedicated terrain mod such as Plateautem. Clan Territory remains responsible for territory ownership, guild access, ward rules, economy, diplomacy, biome dominion, and protection logic.

The legacy code paths are disabled so existing saved ZDO values do not break worlds, but the ward menu no longer exposes the terraforming tab and no background heightmap worker is executed.
