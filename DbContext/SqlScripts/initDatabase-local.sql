USE goodmusicefc;
GO

--create a schema for guest users, i.e. not logged in
CREATE SCHEMA gstusr;
GO

--create a schema for logged in user
CREATE SCHEMA usr;
GO

--create a view that gives overview of the database content
CREATE OR ALTER VIEW gstusr.vwInfoDb AS
    SELECT (SELECT COUNT(*) FROM supusr.MusicGroups WHERE Seeded = 1) as nrSeededMusicGroups, 
        (SELECT COUNT(*) FROM supusr.MusicGroups WHERE Seeded = 0) as nrUnseededMusicGroups,
        (SELECT COUNT(*) FROM supusr.Albums WHERE Seeded = 1) as nrSeededAlbums, 
        (SELECT COUNT(*) FROM supusr.Albums WHERE Seeded = 0) as nrUnseededAlbums,
        (SELECT COUNT(*) FROM supusr.Artists WHERE Seeded = 1) as nrSeededArtists, 
        (SELECT COUNT(*) FROM supusr.Artists WHERE Seeded = 0) as nrUnseededArtists;
GO


--create the DeleteAll procedure
CREATE OR ALTER PROC supusr.spDeleteAll
    @Seeded BIT = 1,

    @nrMusicGroupsAffected INT OUTPUT,
    @nrAlbumsffected INT OUTPUT,
    @nrArtistsAffected INT OUTPUT
    
    AS

    SET NOCOUNT ON;

    SELECT  @nrMusicGroupsAffected = COUNT(*) FROM supusr.MusicGroups WHERE Seeded = @Seeded;
    SELECT  @nrAlbumsffected = COUNT(*) FROM supusr.Albums WHERE Seeded = @Seeded;
    SELECT  @nrArtistsAffected = COUNT(*) FROM supusr.Artists WHERE Seeded = @Seeded;

    DELETE FROM supusr.MusicGroups WHERE Seeded = @Seeded;
    DELETE FROM supusr.Albums WHERE Seeded = @Seeded;
    DELETE FROM supusr.Artists WHERE Seeded = @Seeded;

    SELECT * FROM gstusr.vwInfoDb;

    --throw our own error
    --;THROW 999999, 'my own supusr.spDeleteAll Error directly from SQL Server', 1

    --show return code usage
    RETURN 0;  --indicating success
    --RETURN 1;  --indicating your own error code, in this case 1
GO


--create-users.sql
--Create 3 logins
IF SUSER_ID (N'gstusr') IS NOT NULL
DROP LOGIN gstusr;

IF SUSER_ID (N'usr') IS NOT NULL
DROP LOGIN usr;

IF SUSER_ID (N'supusr') IS NOT NULL
DROP LOGIN supusr;

CREATE LOGIN gstusr WITH PASSWORD=N'pa$$Word1', 
    DEFAULT_DATABASE=goodmusicefc, DEFAULT_LANGUAGE=us_english, 
    CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;

CREATE LOGIN usr WITH PASSWORD=N'pa$$Word1', 
DEFAULT_DATABASE=goodmusicefc, DEFAULT_LANGUAGE=us_english, 
CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;

CREATE LOGIN supusr WITH PASSWORD=N'pa$$Word1', 
DEFAULT_DATABASE=goodmusicefc, DEFAULT_LANGUAGE=us_english, 
CHECK_EXPIRATION=OFF, CHECK_POLICY=OFF;


--create 3 users from the logins, we will late set credentials for these
DROP USER IF EXISTS  gstusrUser;
DROP USER IF EXISTS usrUser;
DROP USER IF EXISTS supusrUser;

CREATE USER gstusrUser FROM LOGIN gstusr;
CREATE USER usrUser FROM LOGIN usr;
CREATE USER supusrUser FROM LOGIN supusr;


--create-roles-credentials.sql
--create roles
CREATE ROLE roleGstUsr;
CREATE ROLE roleUsr;
CREATE ROLE roleSupUsr;

--assign securables creadentials to the roles
GRANT SELECT, EXECUTE ON SCHEMA::gstusr to roleGstUsr;
GRANT SELECT, UPDATE, INSERT ON SCHEMA::supusr to roleUsr;
GRANT SELECT, UPDATE, INSERT, DELETE, EXECUTE ON SCHEMA::supusr to roleSupUsr

--finally, add the users to the roles
ALTER ROLE roleGstUsr ADD MEMBER gstusrUser;

ALTER ROLE roleGstUsr ADD MEMBER usrUser;
ALTER ROLE roleUsr ADD MEMBER usrUser;

ALTER ROLE roleGstUsr ADD MEMBER supusrUser;
ALTER ROLE roleUsr ADD MEMBER supusrUser;
ALTER ROLE roleSupUsr ADD MEMBER supusrUser;
GO

--create-gstusr-login.sql
CREATE OR ALTER PROC gstusr.spLogin
    @UserNameOrEmail NVARCHAR(100),
    @Password NVARCHAR(200),

    @UserId UNIQUEIDENTIFIER OUTPUT,
    @UserName NVARCHAR(100) OUTPUT,
    @Role NVARCHAR(100) OUTPUT
    
    AS

    SET NOCOUNT ON;
    
    SET @UserId = NULL;
    SET @UserName = NULL;
    SET @Role = NULL;
    
    SELECT Top 1 @UserId = UserId, @UserName = UserName, @Role = [Role] FROM dbo.Users 
    WHERE ((UserName = @UserNameOrEmail) OR
           (Email IS NOT NULL AND (Email = @UserNameOrEmail))) AND ([Password] = @Password);
    
    IF (@UserId IS NULL)
    BEGIN
        ;THROW 999999, 'Login error: wrong user or password', 1
    END

GO


