# WeatherInfo API с кэшем и статистикой

Сервис получает данные о погоде по городам, кэширует их, ведет статистику запросов, отдает иконки погодных условий.

Для получения прогноза погоды используется geocoding-api (получение координат для передачи в следующее внешнее API) и open-meteo (получает координаты города и возвращает данные о прогнозе).    

## Основные возможности:
- Получение прогноза погоды на конкретную дату
- Получение прогноза погоды на неделю
- Ведение статистики обращений 
- Отдача иконок погоды через '/static/icons/...'

---

## Технологии:
- **Backend:** С# .NET 8 + ASP.Net Core Web API
- **Database:** PostgreSQL 
- **Cach:** In-Memory
- **Http Client:** HttpClient with Polly (retry/jitter/backoff)
- **Докеризация:** Docker + docker-compose
- **Логирование:** Serilog

---

## Список переменных окружения:

БД:
- 'CONNECTIONSTRING' - строка подключения к PostgreSQL:'Host=localhost;Port=5432;Database=WeatherInfoDb;Username=postgres;Password=123P@ssword123ITSWERYIMPORTANT'
- 'DB_NAME' - 'WeatherInfoDb' 

- 'CACHE_WEATHERTTLMINUTES' - время жизни кэша погоды в минутах: '60'
- 'e_REFRESHAHEADMINUTES' - время обновления кэша заранее (refresh ahead): '10'

Внешние сервисы:
- 'OPENMETEO\_BASEURL' - URL внешнего API погоды: 'https://api.open-meteo.com/v1/forecast'
- 'OPENMETEO\_TIMEZONE' - часовой пояс для запроса к Open-Meteo: 'Europe/London'
- 'GEOCODING\_BASEURL' - URL сервиса геокодинга для резолва города: 'https://geocoding-api.open-meteo.com/v1/search'
- 'GEOCODING\_LANGUAGE' - выставлен: 'ru'

Конфигурация приложения:
- 'ASPNETCORE\_ENVIRONMENT' - 'Development'

Порты для локального запуска:
- 'http' - 'http://localhost:5288'
- 'https'- 'https://localhost:7198'

---

## Установка и запуск:

Примечание:

- Для запуска может потребоваться выполнение команды:
	dotnet tool install --global dotnet-ef

- Необходимо подтвердить создание и использование доверенного сертификата при первом запуске

Шаги:

1. Скачайте и установите .net 8 SDK с официального сайта microsoft
	- ссылка: https://dotnet.microsoft.com/en-us/download/dotnet/8.0
	- проверить версии установленные на Ваш ПК можно командой в cmd: dotnet --version

2. Для локального запуска требуется поднять БД PostgreSQL
	Для быстрого создания БД можно воспользоваться SQL:

	CREATE DATABASE "WeatherInfoDb";
	CREATE USER postgres WITH PASSWORD '123P@ssword123ITSWERYIMPORTANT';
	GRANT ALL PRIVILEGES ON DATABASE "WeatherInfoDb" to postgres;

3. Выберете место для клонирования проекта, перейдите в нужную директорию, нажмите на адресную строку, введите cmd и нажмите enter

4. Клонируйте проект командой: git clone

5. Востановите пакеты (dotnet restore)

6. Примените миграции командой: dotnet ef database update 

7. Для локального запуска приложения используйте: dotnet run

---

## Запуск при помощи Docker:

1. Скачайте и установите Docker

2. В директории проекта при помощи cmd выполните команду:
	docker compose up --build

3. Дождитесь запуска контейнеров weatherinfo:
	weatherinfo_db - БД приложения
	weatherinfo_api - само приложение

---

## Примеры запросов в Docker:

http://localhost:5000/api/weather/Malaga/week

http://localhost:5000/api/weather/Москва?date=2026-02-11

http://localhost:5000/api/stats/top-cities?from=2026-02-13&to=2026-02-13&limit=10

http://localhost:5000/api/stats/requests?from=2026-02-12&to=2026-02-13&page=1&pageSize=10

---

## Проверка записей в БД в контейнере Docker:

1. Откройте cmd

2. Введите команду:
	docker exec -it weatherinfo_db psql -U postgres -d weatherdb

3. Проверка созданных БД в контейнере:
	\dt

4. Для просмотра всех записей в БД необходимо использовать SQL запрос:
	SELECT COUNT(*) FROM "WeatherRequests";

---

## Остановка контейнеров Docker

Остановка контейнеров осуществляется командой:
	docker compose down


