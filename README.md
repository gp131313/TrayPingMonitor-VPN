# TrayPingMonitor-VPN

Индикатор доступности хоста в трее Windows 10/11: цветной кружок с маленькой подписью **VPN** под ним.
Одним взглядом видно, поднят ли туннель до домашней сети, и значок не путается с другими «светофорчиками» в трее.

Форк [Sajjad-s/TrayPingMonitor](https://github.com/Sajjad-s/TrayPingMonitor) — вся логика пинга, трея и настроек
взята оттуда, спасибо автору. Здесь изменён вид значка и версия .NET.

| Цвет | Значение |
|---|---|
| зелёный | хост отвечает |
| жёлтый | медленно (выше порога, по умолчанию 150 мс) или есть потери в последних пингах |
| красный | не отвечает |
| серый | старт или хост не задан |

Задержка и потери — во всплывающей подсказке при наведении.

## Что изменено относительно оригинала

- **Значок**: вместо цифр задержки внутри кружка — кружок поменьше и под ним подпись `VPN`, нарисованная
  пиксельным шрифтом (метод `TrayIconFactory.CreateVpnIcon`). Буквы того же цвета, что и статус, поэтому
  читаются и на тёмной, и на светлой панели задач.
- **.NET 10** вместо .NET 8 (`net10.0-windows`).
- **Автовосстановление** (с v1.1.0): пункт меню **Auto-restore if closed** — если приложение закрыли или оно
  упало, оно поднимется само в течение минуты (см. ниже).
- **Один экземпляр на пользователя**: повторный запуск ничего не делает, значки не дублируются.
- **Переключатель VPN** (с v1.2.0, необязательно): один пункт меню запускает заданную вами задачу Планировщика —
  отключить туннель, если хост отвечает, или подключить, если нет (см. ниже).
- Убраны закоммиченные в оригинале папки `bin/` и `obj/`, добавлен `.gitignore`.

В изменённых файлах в первой строке стоит пометка об изменении (требование GPL-2.0).

## Установка

1. Скачать zip последней версии из [Releases](../../releases) (например, `TrayPingMonitor-VPN-v1.2.0-win-x64.zip`)
   и распаковать в постоянную папку, например `%LOCALAPPDATA%\Programs\TrayPingMonitor`. Контрольные суммы —
   в `SHA256SUMS.txt`.
2. Нужен [.NET 10 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/10.0).
3. Запустить `TrayPingMonitor.exe`. При первом запуске откроется окно настроек: хост (IPv4, IPv6 или имя,
   например `192.168.1.1` — адрес роутера за VPN), интервал пинга и порог «медленно» в мс.
4. Правой кнопкой по значку → **Run at startup** — автозапуск (`HKCU\...\Run`, права администратора не нужны).
5. Там же → **Auto-restore if closed** — автовосстановление.

## Автовосстановление

Галочка **Auto-restore if closed** создаёт задачу Планировщика `TrayPingMonitor keepalive` от вашей учётной
записи (без прав администратора): она запускает `TrayPingMonitor.exe` при входе в систему (через 30 с) и
каждую минуту. Пока приложение работает, повторные запуски ничего не делают — второй экземпляр сразу выходит.
Если приложение закрыли или оно упало, значок вернётся в течение минуты.

- Пока галочка стоит, пункт **Exit** подписан «Exit (auto-restore brings it back within a minute)».
- Закрыть насовсем: снять галочку (задача удалится), затем **Exit**.
- Если exe перенесли в другую папку, при следующем запуске задача сама перенастроится на новый путь.

Из командной строки (для скриптов):

```
TrayPingMonitor.exe --keepalive on|off|status
```

Код возврата: `0` — включено / выполнено, `1` — выключено (для `status`), `2` — ошибка.

## Переключатель VPN (необязательно, с v1.2.0)

Если в `settings.json` заданы имена задач Планировщика `VpnDisconnectTask` и/или `VpnConnectTask`, в меню
появляется один пункт-переключатель. Его подпись выбирается при открытии меню по цвету значка:

- хост отвечает (зелёный или жёлтый) — **Disconnect VPN**, запускается задача `VpnDisconnectTask`;
- не отвечает (красный или серый) — **Connect VPN**, запускается задача `VpnConnectTask`.

```json
{"Host":"192.168.1.1","IntervalMs":1000,"LatencyThresholdMs":150,"RunAtStartup":true,"WindowSize":20,
 "VpnDisconnectTask":"VPN Disconnect","VpnConnectTask":"VPN Connect"}
```

Сам TrayPingMonitor прав администратора не получает — он только запускает задачи по имени. Что они делают, решаете
вы. Например, задача с «highest privileges» останавливает `openconnect` и ставит флаг, по которому ваш watchdog
перестаёт поднимать туннель; вторая снимает флаг и запускает watchdog. Скрипты таких задач держите в папке, куда
обычный пользователь не может писать, иначе правка скрипта = выполнение с правами администратора.

Крайние случаи:

- Дома без VPN хост отвечает напрямую, поэтому пункт покажет **Disconnect VPN**.
- При полном туннеле разрыв VPN рвёт и все удалённые сеансы, которые шли через него.
- Без этих двух параметров пункта в меню нет.

Если значка не видно — он спрятан за стрелкой у часов: *Параметры → Персонализация → Панель задач →
Другие значки области уведомлений*.

Exe не подписан — SmartScreen при первом запуске может предупредить: *Подробнее → Выполнить в любом случае*.
Не доверяете готовому exe — соберите сами (ниже).

Настройки хранятся в `%AppData%\TrayPingMonitor\settings.json`:

```json
{"Host":"192.168.1.1","IntervalMs":1000,"LatencyThresholdMs":150,"RunAtStartup":true,"WindowSize":20}
```

## Сборка из исходников

Нужен .NET 10 SDK (подойдёт и портативный zip SDK, без установки в систему).

```
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true
```

Exe окажется в `bin\Release\net10.0-windows\win-x64\publish\`. Чтобы exe работал без установленного .NET,
соберите с `--self-contained true` (файл станет заметно больше).

## Ограничения

- Один хост на экземпляр. Проверка — ICMP-пинг: если хост отвечает на ping, но не пропускает трафик, значок
  будет зелёным; если ping фильтруется, значок будет красным при рабочем соединении.
- Только Windows (WinForms).

## Поддержать

Если индикатор пригодился — можно кинуть на кофе, см. [DONATE.md](DONATE.md):

- **Dogecoin**: `D7z9UaBsmcV7EqJo5Y5fdLG9xUNw47dNgr`

## Лицензия

GPL-2.0, как у оригинала — см. [LICENSE](LICENSE). Без каких-либо гарантий.

---

## English

A Windows 10/11 tray ping indicator: a colored dot with a tiny **VPN** label under it (green — reachable,
yellow — slow or packet loss, red — unreachable, gray — starting). Handy to see at a glance that your tunnel to
the home network is up, and easy to tell apart from other tray dots.

Fork of [Sajjad-s/TrayPingMonitor](https://github.com/Sajjad-s/TrayPingMonitor) (all ping/tray/settings logic is
the original author's). Changes: pixel-font "VPN" label icon instead of latency digits, .NET 10, single instance
per user, optional **Auto-restore if closed** (a per-user scheduled task restarts the app within a minute; also
`TrayPingMonitor.exe --keepalive on|off|status`), optional **VPN toggle** menu item (runs the scheduled task named
in `VpnDisconnectTask` when the host answers, `VpnConnectTask` when it does not), removed committed
`bin/`/`obj/`. Download the zip from Releases, install the .NET 10 Desktop Runtime, run `TrayPingMonitor.exe`,
set the host (e.g. your router `192.168.1.1`). License: GPL-2.0.
