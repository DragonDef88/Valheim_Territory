Краткий вывод исследования:

Valheim StoreGui — правильный lifecycle-референс: Show, Hide, Update, auto-close по дистанции, смерть/катсцена, Esc/Use.
Valheim PrivateArea хранит permitted players прямо в ZDO через ZDOVars.s_permitted, pu_idX, pu_nameX.
Vanilla PrivateArea.Interact делает toggle enabled или toggle permitted, поэтому наш Prefix правильно заменяет vanilla-действие меню управления.
STU_Ward строит UI через GUIManager.CustomGUIFront, woodpanel и GUIManager.BlockInput, а не через случайный Canvas.
STU_Ward хранит расширенные настройки варда в ZDO-ключах вроде radius, marker alpha/speed, auto-close, restrictions.