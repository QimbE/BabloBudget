# BabloBudget
Приложение BabloBudget представляет собой средство для ведения учёта денежных средств. Оно позволяет составить общий кошелёк, вносить в него изменения (однократные или повторяющиеся) и получать статистику.

## Содержание
- [Технологии](#технологии)
- [Использование](#использование)
- [Тестирование](#тестирование)
- [CI/CD](#ci-cd)
- [Команда проекта](#команда-проекта)

## Технологии
- [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet/)
- [Docker](https://www.docker.com/)
- [Swagger](https://swagger.io/)

## Использование
Для запуска приложения требуется использовать [Docker](https://www.docker.com/)

Для запуска backend необходимо выполнить команду из корневой папки проекта:
```sh
docker-compose up -d
```
Команда не отличается в зависимости от установленной ОС.

Приложение доступно по порту: 
http://localhost:8018/

База данных открыта по порту: 
http://localhost:5433/

OpenAPI документация доступна по адресу: 
http://localhost:8018/swagger/

## Тестирование

Код доменной логики покрыт юнит-тестами MSTest.

## CI-CD

В проекте изпользуются 2 CI пайплайна:
- [main](https://github.com/QimbE/BabloBudget/blob/main/.github/workflows/main.yml): не допускает возможность публикации несобираемого приложения в основную ветку
- [pull_request](https://github.com/QimbE/BabloBudget/blob/main/.github/workflows/pull_request.yml): не допускает merge request при невыполнении хотя бы одного теста

## Команда проекта
- [Цепенщиков Владимир](https://github.com/QimbE) — Back-end
- [Сохранных Степан](https://github.com/likeitbro) — Back-end
- Дарья Литвинова — Front-end
- Гааг Александр — Front-end