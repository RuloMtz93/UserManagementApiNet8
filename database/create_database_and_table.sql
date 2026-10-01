/* =========================
   Crear la base de datos
   ========================= */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF DB_ID('UsersDb') IS NULL
    CREATE DATABASE UsersDb;
GO

USE UsersDb;
GO

IF OBJECT_ID('dbo.Users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id           UNIQUEIDENTIFIER NOT NULL
                         CONSTRAINT DF_Users_Id DEFAULT NEWSEQUENTIALID(),
        UserName     VARCHAR(50)      NOT NULL,
        PasswordHash VARCHAR(500)     NOT NULL,
        Email        VARCHAR(150)     NOT NULL,
        Status       TINYINT          NOT NULL
                         CONSTRAINT DF_Users_Status DEFAULT 1,
        CreatedDate  DATETIME2        NOT NULL
                         CONSTRAINT DF_Users_CreatedDate DEFAULT SYSUTCDATETIME(),
        UpdatedDate  DATETIME2        NULL,

        CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT CK_Users_Status CHECK (Status IN (0, 1))
    );
END
GO


IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Users_UserName' AND object_id = OBJECT_ID('dbo.Users'))
    CREATE UNIQUE NONCLUSTERED INDEX UX_Users_UserName
        ON dbo.Users (UserName);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Users_Email' AND object_id = OBJECT_ID('dbo.Users'))
    CREATE UNIQUE NONCLUSTERED INDEX UX_Users_Email
        ON dbo.Users (Email);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Active' AND object_id = OBJECT_ID('dbo.Users'))
    CREATE NONCLUSTERED INDEX IX_Users_Active
        ON dbo.Users (CreatedDate)
        INCLUDE (UserName, Email)
        WHERE Status = 1;
GO

/* ================================================
   Stored procedures de administración de usuarios.
   ================================================ */

/* ---------- sp_User_Create ---------- */
CREATE OR ALTER PROCEDURE dbo.sp_User_Create
    @UserName     VARCHAR(50),
    @PasswordHash VARCHAR(500),
    @Email        VARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF EXISTS (SELECT 1 FROM dbo.Users WHERE UserName = @UserName)
        THROW 50001, 'El nombre de usuario ya está registrado.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email)
        THROW 50002, 'El correo electrónico ya está registrado.', 1;

    BEGIN TRY
        INSERT INTO dbo.Users (UserName, PasswordHash, Email, Status, CreatedDate)
        OUTPUT INSERTED.Id, INSERTED.UserName, INSERTED.Email, INSERTED.Status, INSERTED.CreatedDate, INSERTED.UpdatedDate
        VALUES (@UserName, @PasswordHash, @Email, 1, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() IN (2601, 2627)
        BEGIN
            IF ERROR_MESSAGE() LIKE '%UX_Users_UserName%'
                THROW 50001, 'El nombre de usuario ya está registrado.', 1;
            ELSE
                THROW 50002, 'El correo electrónico ya está registrado.', 1;
        END;

        THROW;
    END CATCH;
END
GO

/* ---------- sp_User_GetById ---------- */
CREATE OR ALTER PROCEDURE dbo.sp_User_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, UserName, Email, Status, CreatedDate, UpdatedDate
    FROM dbo.Users
    WHERE Id = @Id;
END
GO

/* ---------- sp_User_GetActive ---------- */
CREATE OR ALTER PROCEDURE dbo.sp_User_GetActive
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, UserName, Email, Status, CreatedDate, UpdatedDate
    FROM dbo.Users
    WHERE Status = 1
    ORDER BY CreatedDate;
END
GO

/* ---------- sp_User_Update ---------- */
CREATE OR ALTER PROCEDURE dbo.sp_User_Update
    @Id     UNIQUEIDENTIFIER,
    @Email  VARCHAR(150),
    @Status TINYINT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @Status NOT IN (0, 1)
        THROW 50004, 'El Status debe ser 1 (Active) o 0 (Inactive).', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @Id)
        THROW 50003, 'El usuario no existe.', 1;

    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email AND Id <> @Id)
        THROW 50002, 'El correo electrónico ya está registrado.', 1;

    BEGIN TRY
        UPDATE dbo.Users
        SET Email       = @Email,
            Status      = @Status,
            UpdatedDate = SYSUTCDATETIME()
        WHERE Id = @Id;

        SELECT Id, UserName, Email, Status, CreatedDate, UpdatedDate
        FROM dbo.Users
        WHERE Id = @Id;
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() IN (2601, 2627)
            THROW 50002, 'El correo electrónico ya está registrado.', 1;

        THROW;
    END CATCH;
END
GO

/* ---------- sp_User_Deactivate ---------- */
CREATE OR ALTER PROCEDURE dbo.sp_User_Deactivate
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Users
    SET Status      = 0,
        UpdatedDate = SYSUTCDATETIME()
    WHERE Id = @Id;

    IF @@ROWCOUNT = 0
        THROW 50003, 'El usuario no existe.', 1;

    SELECT Id, UserName, Email, Status, CreatedDate, UpdatedDate
    FROM dbo.Users
    WHERE Id = @Id;
END
GO