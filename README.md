# VlaDO — Document Flow System 📁🚀

VlaDO (Владение ДокументоОборотом) — это веб-приложение для хранения, редактирования и отслеживания истории документов в виде цепочек версий. Проект объединяет API на .NET и фронт на React с современными практиками документооборота и аутентификации.

---

## 🔧 Технологии

- 🌐 **Frontend**: React + Vite + Axios + Bootstrap
- 🛠️ **Backend**: ASP.NET Core (.NET 8) + Entity Framework Core (SQLite)
- 🔒 **Аутентификация**: JWT
- 🐳 **Docker**: production-сборка с фронтом и API
- 🗃️ **База данных**: SQLite (персистится в volume)

---

## 🚀 Запуск проекта (локально)

### 1. Установка зависимостей
```bash
cd clietnvlado
npm install
2. Запуск фронта
bash
Копировать
Редактировать
npm run dev
По умолчанию будет доступен на: http://localhost:5173

3. Запуск бэкенда
bash
Копировать
Редактировать
cd ../VlaDO
dotnet run
По умолчанию: http://localhost:5223

🐳 Production (Docker)
Сборка фронта
bash
Копировать
Редактировать
cd clietnvlado
npm run build:full
Сборка Docker-образа
bash
Копировать
Редактировать
cd ../VlaDO
docker build -t vlado-app .
Запуск контейнера
bash
Копировать
Редактировать
docker run -p 8080:80 vlado-app
Приложение будет доступно по адресу: http://localhost:8080

🔐 Аутентификация
API использует JWT. После логина токен нужно указывать в заголовке:

makefile
Копировать
Редактировать
Authorization: Bearer <ваш токен>
Swagger доступен по адресу: http://localhost:5223/swagger

📂 Возможности
Загрузка и хранение документов

Создание и управление комнатами

Версионирование документов (форки и цепочки)

Приглашения пользователей и права доступа

История активности по документам

Сброс пароля через email

Docker-сборка всего приложения

📎 Скрипты
Название	Команда	Описание
dev	npm run dev	Запуск фронта (localhost:5173)
build	npm run build	Сборка фронта
copy	npm run copy	Копирование сборки в wwwroot
build:full	npm run build:full	Сборка + копирование

🧑‍💻 Автор
Разработка: Владислав
