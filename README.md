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
- Убраны закоммиченные в оригинале папки `bin/` и `obj/`, добавлен `.gitignore`.

В изменённых файлах в первой строке стоит пометка об изменении (требование GPL-2.0).

## Установка

1. Скачать `TrayPingMonitor-VPN-v1.0.0-win-x64.zip` из [Releases](../../releases) и распаковать в постоянную
   папку, например `%LOCALAPPDATA%\Programs\TrayPingMonitor`. Контрольные суммы — в `SHA256SUMS.txt`.
2. Нужен [.NET 10 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/10.0).
3. Запустить `TrayPingMonitor.exe`. При первом запуске откроется окно настроек: хост (IPv4, IPv6 или имя,
   например `192.168.1.1` — адрес роутера за VPN), интервал пинга и порог «медленно» в мс.
4. Правой кнопкой по значку → **Run at startup** — автозапуск (`HKCU\...\Run`, права администратора не нужны).

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
the original author's). Changes: pixel-font "VPN" label icon instead of latency digits, .NET 10, removed committed
`bin/`/`obj/`. Download the zip from Releases, install the .NET 10 Desktop Runtime, run `TrayPingMonitor.exe`,
set the host (e.g. your router `192.168.1.1`). License: GPL-2.0.
