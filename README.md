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
