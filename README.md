### Требования:
- Установленная и запущенная локальная данных.
- Строку подключеня добавить в файл appsettings.json, поле "ConnectionStrings" - "default"
- В базе данных должны быть внесены таблицы и процедуры. Шаги описаны ниже [Настройка локальной базы данных](#настройка-локальной-базы-данных)

### Порядок запуска:
- Открыть этот проект дважды, в двух разных окнах.
- Сбилдить проект
- В одном окне проект **SoapDemo** установить как стартовый проект (Startup Project)
- В другом окне проект **IdentityDemo** установить как стартовый проект (Startup Project)
- Запустить **SoapDemo** - это фоновый вебсервис, работающий с базой
- Запустить **IdentityDemo** - это вебсервис, где через визуальный интерфейс можно проверить функционал.

### Что можно протестировать:
- **Register** - регистрация нового пользователя
- **Login**  - вход пользоателя. Т.к. база данных изначально пуста, для авторизации нужно сначала зарегистрироваться


### Настройка локальной базы данных

﻿В локальной базе данных создать таблицу IdentityDemo
Запусть три скрипта:
**1) Создание таблицы IdentityUser**
```
USE [IdentityDemo]
 GO
 SET ANSI_NULLS ON
 GO
 SET QUOTED_IDENTIFIER ON
 GO
 CREATE TABLE [dbo].[IdentityUser](
     [ID] [int] IDENTITY(1,1) NOT NULL,
     [Email] nvarchar NULL,
     [Password] nvarchar NULL,
     [Role] nvarchar NULL,
     [Reg_Date] [datetime] NULL
 ) ON [PRIMARY]
 GO
 ```
 
** 2) Создание процедуры sp_loginUser:**
```
 USE [IdentityDemo]
 GO
 SET ANSI_NULLS ON
 GO
 SET QUOTED_IDENTIFIER ON
 GO
 -- =============================================
 -- Author:    FreeCode Spot
 -- Create date: 
 -- Description:    
 -- =============================================
 CREATE PROCEDURE [dbo].[sp_loginUser]
     @email Nvarchar(50),
     @password nvarchar(200)
     AS
 BEGIN
     SET NOCOUNT ON;
 Select * FROM IdentityUser where Email = @email and [Password] = CONVERT(VARCHAR(32), HashBytes('MD5', @password), 2) 
 END
 ```

** 3) Создание процедуры sp_registerUser:**
```
 USE [IdentityDemo]
 GO
 SET ANSI_NULLS ON
 GO
 SET QUOTED_IDENTIFIER ON
 GO
 -- =============================================
 -- Author:        FeeCode Spot
 -- Create date: 
 -- Description:    
 -- =============================================
 CREATE PROCEDURE [dbo].[sp_registerUser]
     @email Nvarchar(50),
     @password nvarchar(200),
     @role nvarchar(50),
     @retval int OUTPUT
 AS
 BEGIN
     SET NOCOUNT ON;
       INSERT INTO IdentityUser(Email,[Password],[Role],Reg_Date) VALUES(@email,CONVERT(VARCHAR(32), HashBytes('MD5', @password), 2),@role,GETDATE())
       if(@@ROWCOUNT > 0)
       BEGIN
         SET @retval = 200
       END
       ELSE
       BEGIN
       SET @retval = 500
       END
 END
```
