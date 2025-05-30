# BabloBudget
���������� BabloBudget ������������ ����� �������� ��� ������� ����� �������� �������. ��� ��������� ��������� ����� ������, ������� � ���� ��������� (����������� ��� �������������) � �������� ����������.

## ����������
- [����������](#����������)
- [�������������](#�������������)
- [������������](#������������)
- [CI/CD](#ci-cd)
- [������� �������](#�������-�������)

## ����������
- [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet/)
- [Docker](https://www.docker.com/)
- [Swagger](https://swagger.io/)

## �������������
��� ������� ���������� ��������� ������������ [Docker](https://www.docker.com/)

��� ������� backend ���������� ��������� ������� �� �������� ����� �������:
```sh
docker-compose up -d
```
������� �� ���������� � ����������� �� ������������� ��.

���������� �������� �� �����: 
http://localhost:8018/

���� ������ ������� �� �����: 
http://localhost:5433/

OpenAPI ������������ �������� �� ������: 
http://localhost:8018/swagger/

## ������������

��� �������� ������ ������ ����-������� MSTest.

## CI-CD

� ������� ������������ 2 CI ���������:
- [main](https://github.com/QimbE/BabloBudget/blob/main/.github/workflows/main.yml): �� ��������� ����������� ���������� ������������� ���������� � �������� �����
- [pull_request](https://github.com/QimbE/BabloBudget/blob/main/.github/workflows/pull_request.yml): �� ��������� merge request ��� ������������ ���� �� ������ �����

## ������� �������
- [���������� ��������](https://github.com/QimbE) � Back-end
- [��������� ������](https://github.com/likeitbro) � Back-end
- ����� ��������� � Front-end
- ���� ��������� � Front-end