USE [master]
GO
/****** Object:  Database [SusyLeague]    Script Date: 23/06/2019 11:06:11 ******/
CREATE DATABASE [SusyLeague]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'SusyLeague', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.SQLEXPRESS\MSSQL\DATA\SusyLeague.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'SusyLeague_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.SQLEXPRESS\MSSQL\DATA\SusyLeague_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
ALTER DATABASE [SusyLeague] SET COMPATIBILITY_LEVEL = 140
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [SusyLeague].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [SusyLeague] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [SusyLeague] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [SusyLeague] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [SusyLeague] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [SusyLeague] SET ARITHABORT OFF 
GO
ALTER DATABASE [SusyLeague] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [SusyLeague] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [SusyLeague] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [SusyLeague] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [SusyLeague] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [SusyLeague] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [SusyLeague] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [SusyLeague] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [SusyLeague] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [SusyLeague] SET  DISABLE_BROKER 
GO
ALTER DATABASE [SusyLeague] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [SusyLeague] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [SusyLeague] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [SusyLeague] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [SusyLeague] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [SusyLeague] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [SusyLeague] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [SusyLeague] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [SusyLeague] SET  MULTI_USER 
GO
ALTER DATABASE [SusyLeague] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [SusyLeague] SET DB_CHAINING OFF 
GO
ALTER DATABASE [SusyLeague] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [SusyLeague] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [SusyLeague] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [SusyLeague] SET QUERY_STORE = OFF
GO
USE [SusyLeague]
GO
/****** Object:  Schema [seriea]    Script Date: 23/06/2019 11:06:11 ******/
CREATE SCHEMA [seriea]
GO
/****** Object:  Schema [susyleague]    Script Date: 23/06/2019 11:06:11 ******/
CREATE SCHEMA [susyleague]
GO
/****** Object:  Table [seriea].[squadre]    Script Date: 23/06/2019 11:06:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [seriea].[squadre](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Descrizione] [nvarchar](50) NULL,
 CONSTRAINT [PK_squadre] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  UserDefinedFunction [dbo].[fn_getSquadreIdSvincolati]    Script Date: 23/06/2019 11:06:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[fn_getSquadreIdSvincolati]()
RETURNS TABLE 
AS
RETURN 
(
	-- Add the SELECT statement with parameter references here
	SELECT Id
	FROM seriea.squadre
	where Descrizione  = 'SERIE MINORE'
	OR Descrizione  = 'SVINCOLATO'
	OR Descrizione  = 'SERIE ESTERA'
)
GO
/****** Object:  Table [seriea].[giocatori]    Script Date: 23/06/2019 11:06:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [seriea].[giocatori](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Nome] [nvarchar](50) NOT NULL,
	[Cognome] [nvarchar](50) NOT NULL,
	[GazzettaId] [nvarchar](10) NULL,
 CONSTRAINT [PK_giocatori] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [seriea].[giornate]    Script Date: 23/06/2019 11:06:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [seriea].[giornate](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Descrizione] [nvarchar](50) NOT NULL,
	[DataInizio] [datetime] NULL,
	[DataFine] [datetime] NULL,
	[StagioneId] [int] NOT NULL,
 CONSTRAINT [PK_giornate] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [seriea].[squadre_lk_giocatori]    Script Date: 23/06/2019 11:06:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [seriea].[squadre_lk_giocatori](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GiocatoreId] [int] NOT NULL,
	[SquadraId] [int] NOT NULL,
	[DataInizio] [date] NOT NULL,
	[DataFine] [date] NULL,
 CONSTRAINT [PK_squadre_lk_giocatori] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [seriea].[squadre_lk_stagioni]    Script Date: 23/06/2019 11:06:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [seriea].[squadre_lk_stagioni](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[SquadraId] [int] NOT NULL,
	[StagioneId] [int] NOT NULL,
 CONSTRAINT [PK_squadre_lk_stagioni] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [seriea].[stagioni]    Script Date: 23/06/2019 11:06:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [seriea].[stagioni](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Descrizione] [nvarchar](50) NULL,
 CONSTRAINT [PK_stagioni] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [seriea].[statistiche]    Script Date: 23/06/2019 11:06:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [seriea].[statistiche](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GiocatoreId] [int] NOT NULL,
	[StagioneId] [int] NOT NULL,
	[Presenze] [int] NULL,
	[Giocabili] [int] NULL,
	[MediaVoto] [float] NULL,
	[Fantamedia] [float] NULL,
	[GolFatti] [int] NULL,
	[GolSubiti] [int] NULL,
	[Autogol] [int] NULL,
	[Assist] [int] NULL,
	[Ammonizioni] [int] NULL,
	[Espulsioni] [int] NULL,
	[RigoriSbagliati] [int] NULL,
	[RigoriTrasformati] [int] NULL,
	[RigoriParati] [int] NULL,
	[RigoriSubiti] [int] NULL,
 CONSTRAINT [PK_statistiche] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [seriea].[voti]    Script Date: 23/06/2019 11:06:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [seriea].[voti](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GiocatoreId] [int] NOT NULL,
	[GiornataId] [int] NOT NULL,
	[Voto] [float] NOT NULL,
	[Fantavoto] [float] NOT NULL,
	[GolFatti] [int] NULL,
	[GolSubiti] [int] NULL,
	[Autogol] [int] NULL,
	[Assist] [int] NULL,
	[Ammonizione] [bit] NULL,
	[Espulsione] [bit] NULL,
	[RigoriSbagliati] [int] NULL,
	[RigoriTrasformati] [int] NULL,
	[RigoriParati] [int] NULL,
	[RigoriSubiti] [int] NULL,
 CONSTRAINT [PK_voti] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [susyleague].[competizione_lk_stagione]    Script Date: 23/06/2019 11:06:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [susyleague].[competizione_lk_stagione](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CompetizioneId] [int] NOT NULL,
	[StagioneId] [int] NOT NULL,
 CONSTRAINT [PK_competizione_lk_stagione] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [susyleague].[competizioni]    Script Date: 23/06/2019 11:06:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [susyleague].[competizioni](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Descrizione] [nvarchar](50) NULL,
 CONSTRAINT [PK_competizioni] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [susyleague].[giocatore_lk_ruolo]    Script Date: 23/06/2019 11:06:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [susyleague].[giocatore_lk_ruolo](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[GiocatoreId] [int] NOT NULL,
	[RuoloId] [int] NOT NULL,
	[StagioneId] [int] NOT NULL,
 CONSTRAINT [PK_giocatore_lk_ruolo] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [susyleague].[giornate]    Script Date: 23/06/2019 11:06:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [susyleague].[giornate](
	[Id] [int] NOT NULL,
	[Descrizione] [nchar](10) NOT NULL,
	[GiornataSerieAId] [int] NOT NULL,
	[CompetizioneSusyLeagueId] [int] NOT NULL,
 CONSTRAINT [PK_giornate_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [susyleague].[incontri]    Script Date: 23/06/2019 11:06:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [susyleague].[incontri](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Squadra1Id] [int] NOT NULL,
	[Squadra2Id] [int] NOT NULL,
	[GiornataId] [int] NOT NULL,
 CONSTRAINT [PK_incontri] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [susyleague].[ruoli]    Script Date: 23/06/2019 11:06:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [susyleague].[ruoli](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Descrizione1] [nchar](20) NOT NULL,
	[Sigla1] [nchar](10) NOT NULL,
	[Descrizione2] [nchar](20) NULL,
	[Sigla2] [nchar](10) NULL,
 CONSTRAINT [PK_ruoli] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [susyleague].[squadra_lk_giocatore]    Script Date: 23/06/2019 11:06:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [susyleague].[squadra_lk_giocatore](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Squadraid] [int] NOT NULL,
	[StagioneId] [int] NOT NULL,
	[GiocatoreId] [int] NOT NULL,
	[DataInizio] [date] NOT NULL,
	[DataFine] [date] NULL,
 CONSTRAINT [PK_squadra_lk_giocatore] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [susyleague].[squadre]    Script Date: 23/06/2019 11:06:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [susyleague].[squadre](
	[Id] [int] NOT NULL,
	[Descrizione] [nvarchar](50) NULL,
 CONSTRAINT [PK_squadre_1] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [seriea].[squadre_lk_giocatori]  WITH CHECK ADD  CONSTRAINT [FK_squadre_lk_giocatori_giocatori] FOREIGN KEY([GiocatoreId])
REFERENCES [seriea].[giocatori] ([Id])
GO
ALTER TABLE [seriea].[squadre_lk_giocatori] CHECK CONSTRAINT [FK_squadre_lk_giocatori_giocatori]
GO
ALTER TABLE [seriea].[squadre_lk_giocatori]  WITH CHECK ADD  CONSTRAINT [FK_squadre_lk_giocatori_squadre] FOREIGN KEY([SquadraId])
REFERENCES [seriea].[squadre] ([Id])
GO
ALTER TABLE [seriea].[squadre_lk_giocatori] CHECK CONSTRAINT [FK_squadre_lk_giocatori_squadre]
GO
ALTER TABLE [seriea].[squadre_lk_stagioni]  WITH CHECK ADD  CONSTRAINT [FK_squadre_lk_stagioni_squadre] FOREIGN KEY([SquadraId])
REFERENCES [seriea].[squadre] ([Id])
GO
ALTER TABLE [seriea].[squadre_lk_stagioni] CHECK CONSTRAINT [FK_squadre_lk_stagioni_squadre]
GO
ALTER TABLE [seriea].[squadre_lk_stagioni]  WITH CHECK ADD  CONSTRAINT [FK_squadre_lk_stagioni_stagioni] FOREIGN KEY([StagioneId])
REFERENCES [seriea].[stagioni] ([Id])
GO
ALTER TABLE [seriea].[squadre_lk_stagioni] CHECK CONSTRAINT [FK_squadre_lk_stagioni_stagioni]
GO
/****** Object:  StoredProcedure [seriea].[giocatore_insert]    Script Date: 23/06/2019 11:06:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Giuseppe Aurilio>
-- Create date: <02/06/2019>
-- Description:	<inserisce un giocatore che non è già presente>
-- =============================================

/* ESEMPIO DI CHIAMATA

DECLARE	@return_value int,
		@ErrorMessage nvarchar(4000),
		@Id int

EXEC	@return_value = [susyleague].[giocatore_insert]
		@Nome = N'Lorenzo',
		@Cognome = N'Pellegrini',
		@Id = @Id OUTPUT,
		@ErrorMessage = @ErrorMessage OUTPUT

SELECT	@ErrorMessage as N'@ErrorMessage', 
		@Id as N'@Id'

SELECT	'Return Value' = @return_value

*/
CREATE PROCEDURE [seriea].[giocatore_insert]

	 @Nome nvarchar(50),
	 @Cognome nvarchar(50),
	 @GazzettaId nvarchar(10) = null,

	 @Id int  = 0 output,
	 @ErrorMessage NVARCHAR(4000) output
	 
AS
BEGIN TRY  
	SET NOCOUNT ON;

	declare @num int = 0;
	set @num = (select count (*) from [seriea].[giocatori]
	where Nome = @Nome and Cognome = @Cognome)
	If(@num = 0)
		begin
			INSERT INTO [seriea].[giocatori] ([Nome], [Cognome], [GazzettaId])
			VALUES (@Nome,@Cognome, @GazzettaId);
			
			SET @Id = SCOPE_IDENTITY(); 
		end
	else
		begin
			RAISERROR(50001, -1, -1, '[giocatore_insert]');
		end
	
END TRY  
BEGIN CATCH  
	set @ErrorMessage = ERROR_MESSAGE();
	return ERROR_NUMBER();
END CATCH 
GO
/****** Object:  StoredProcedure [seriea].[giocatore_update]    Script Date: 23/06/2019 11:06:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Giuseppe Aurilio>
-- Create date: <02/06/2019>
-- Description:	<aggiorna il GazzettaId di un giocatore>
-- =============================================

/* ESEMPIO DI CHIAMATA

DECLARE	@return_value int,
		@ErrorMessage nvarchar(4000)

EXEC	@return_value = [susyleague].[giocatore_update]
		@Id = 10,
		@GazzettaId = N'G123',
		@ErrorMessage = @ErrorMessage OUTPUT

SELECT	@ErrorMessage as N'@ErrorMessage'

SELECT	'Return Value' = @return_value

*/
CREATE PROCEDURE [seriea].[giocatore_update]
	 @Id int,
	 @GazzettaId nvarchar(10),
	 
	 @ErrorMessage NVARCHAR(4000) output
AS
BEGIN TRY  
	SET NOCOUNT ON;

	Update [seriea].[giocatori]
	set GazzettaId = @GazzettaId
	where Id = @Id

	iF(@@RowCount = 0)
		RAISERROR(50002, -1, -1, '[giocatore_update]');
	iF(@@RowCount > 1)
		RAISERROR(50003, -1, -1, '[giocatore_update]');	

END TRY  
BEGIN CATCH  
    
	set @ErrorMessage = ERROR_MESSAGE();
	return ERROR_NUMBER();
END CATCH 
GO
/****** Object:  StoredProcedure [seriea].[giornata_insert]    Script Date: 23/06/2019 11:06:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Giuseppe Aurilio>
-- Create date: <03/06/2019>
-- Description:	<inserisce una giornata nella stagione di serie a>
-- =============================================

/* ESEMPIO DI CHIAMATA

DECLARE	@return_value int,
		@Id int,
		@ErrorMessage nvarchar(4000)

EXEC	@return_value = [seriea].[giornata_insert]
		@Descrizione = N'1°',
		@DataInizio = N'18/8/2017 18:00',
		@DataFine = N'20/8/2017 20:30',
		@StagioneId = 7,
		@Id = @Id OUTPUT,
		@ErrorMessage = @ErrorMessage OUTPUT

SELECT	@Id as N'@Id',
		@ErrorMessage as N'@ErrorMessage'

SELECT	'Return Value' = @return_value

*/
CREATE PROCEDURE [seriea].[giornata_insert]

	 @Descrizione nvarchar(50),
	 @DataInizio datetime,
	 @DataFine datetime,
	 @StagioneId int,

	 @Id int  = 0 output,
	 @ErrorMessage NVARCHAR(4000) output
	 
AS
BEGIN TRY  
	SET NOCOUNT ON;

	declare @num int = 0;
	set @num = (select count (*) from [seriea].[giornate]
				where Descrizione = @Descrizione and StagioneId = @StagioneId)
	If(@num = 0)
		begin
			INSERT INTO [seriea].[giornate] (Descrizione, DataInizio, DataFine, StagioneId)
			VALUES (@Descrizione, @DataInizio, @DataFine, @StagioneId);
			
			SET @Id = SCOPE_IDENTITY(); 
		end
	else
		begin
			RAISERROR(50004, -1, -1, '[giornata_insert]');
		end
	
END TRY  
BEGIN CATCH  
	set @ErrorMessage = ERROR_MESSAGE();
	return ERROR_NUMBER();
END CATCH 
GO
/****** Object:  StoredProcedure [seriea].[giornata_update]    Script Date: 23/06/2019 11:06:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Giuseppe Aurilio>
-- Create date: <03/06/2019>
-- Description:	<aggiorna la giornata di una stagione>
-- =============================================

/* ESEMPIO DI CHIAMATA

DECLARE	@return_value int,
		@ErrorMessage nvarchar(4000)

--ERROR
--exec @return_value = [seriea].[giornata_update]  1, '1° Giornata', @ErrorMessage = @ErrorMessage OUTPUT

--Update senza date
exec @return_value = [seriea].[giornata_update]  2, '1° Giornata', @ErrorMessage = @ErrorMessage OUTPUT

--Update con date
--exec @return_value = [seriea].[giornata_update]  2, '1° Giornata', '18/08/2018 18:00', '20/08/2018 20:30', @ErrorMessage = @ErrorMessage OUTPUT


SELECT	@ErrorMessage as N'@ErrorMessage'

SELECT	'Return Value' = @return_value
*/

CREATE PROCEDURE [seriea].[giornata_update]
	 @Id int,

	 @Descrizione nvarchar(50),
	 @DataInizio datetime = null,
	 @DataFine datetime = null,
	 
	 @ErrorMessage NVARCHAR(4000) output
AS
BEGIN TRY  
	SET NOCOUNT ON;

	Update [seriea].[giornate]
	set Descrizione = @Descrizione, 
		DataInizio = IsNull(@DataInizio, DataInizio),
		DataFine = IsNull(@DataFine, DataFine)
	where Id = @Id
	iF(@@RowCount = 0)
		RAISERROR(50005, -1, -1, '[giornata_update]');
END TRY  
BEGIN CATCH  
    
	set @ErrorMessage = ERROR_MESSAGE();
	return ERROR_NUMBER();
END CATCH 
GO
/****** Object:  StoredProcedure [seriea].[squadra_delete_giocatore]    Script Date: 23/06/2019 11:06:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Giuseppe Aurilio>
-- Create date: <02/06/2019>
-- Description:	<associa un giocatore ad una  squadra, a partire dalla data specificata>
-- =============================================

/* ESEMPIO DI CHIAMATA

DECLARE	@return_value int,
		@Id int,
		@ErrorMessage nvarchar(4000)

EXEC	@return_value = [seriea].[squadra_delete_giocatore]
		@GiocatoreId = 1,
		@SquadraId = 15,
		@DataFine = '31/05/2018',
		@ErrorMessage = @ErrorMessage OUTPUT

SELECT	@Id as N'@Id',
		@ErrorMessage as N'@ErrorMessage'

SELECT	'Return Value' = @return_value

*/
CREATE PROCEDURE [seriea].[squadra_delete_giocatore]
	 
	 @GiocatoreId int,
	 @SquadraId int,
	 @DataFine date,

	 @ErrorMessage NVARCHAR(4000) output
AS
BEGIN TRY  
	SET NOCOUNT ON;
	update [seriea].squadre_lk_giocatori
	SET DataFine = @DataFine
	where GiocatoreId = @GiocatoreId 
	and SquadraId = @SquadraId
	AND DataFine is null
	iF(@@RowCount = 0)
		RAISERROR(50002, -1, -1, '[squadra_delete_giocatore]');
	iF(@@RowCount > 1)
		RAISERROR(50003, -1, -1, '[squadra_delete_giocatore]');	

END TRY  
BEGIN CATCH  
    
	set @ErrorMessage = ERROR_MESSAGE();
	return ERROR_NUMBER();
END CATCH 
GO
/****** Object:  StoredProcedure [seriea].[squadra_insert]    Script Date: 23/06/2019 11:06:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Giuseppe Aurilio>
-- Create date: <02/06/2019>
-- Description:	<inserisce una squadra che non è già presente>
-- =============================================

/* ESEMPIO DI CHIAMATA
DECLARE	@return_value int,
		@Id int,
		@ErrorMessage nvarchar(4000)

EXEC	@return_value = [seriea].[squadra_insert] 'Cittadella', @Id = @Id OUTPUT,@ErrorMessage = @ErrorMessage OUTPUT
		
SELECT	@Id as N'@Id',
		@ErrorMessage as N'@ErrorMessage'

SELECT	'Return Value' = @return_value
*/
CREATE PROCEDURE [seriea].[squadra_insert]
	 @Descrizione nvarchar(50),

	 @Id int  = 0 output,
	 @ErrorMessage NVARCHAR(4000) output
AS
BEGIN TRY  
	SET NOCOUNT ON;

	declare @num int = 0;
	set @num = (select count (*) from [seriea].[squadre]
	where Descrizione = @Descrizione )
	If(@num = 0)
		begin
			INSERT INTO [seriea].[squadre] (Descrizione)
			VALUES (@Descrizione);
			
			SET @Id = SCOPE_IDENTITY(); 
		end
	else
		begin
			RAISERROR(50006, -1, -1, '[squadra_insert]');
		end
	
END TRY  
BEGIN CATCH  
	set @ErrorMessage = ERROR_MESSAGE() ;
	return ERROR_NUMBER();
END CATCH 
GO
/****** Object:  StoredProcedure [seriea].[squadra_insert_giocatore]    Script Date: 23/06/2019 11:06:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Giuseppe Aurilio>
-- Create date: <02/06/2019>
-- Description:	<associa un giocatore ad una  squadra, a partire dalla data specificata>
-- =============================================

/* ESEMPIO DI CHIAMATA

DECLARE	@return_value int,
		@Id int,
		@ErrorMessage nvarchar(4000)

EXEC	@return_value = [seriea].[squadra_insert_giocatore]
		@GiocatoreId = 1,
		@SquadraId = 15,
		@DataInizio = '01/01/2000',
		@Id = @Id OUTPUT,
		@ErrorMessage = @ErrorMessage OUTPUT

SELECT	@Id as N'@Id',
		@ErrorMessage as N'@ErrorMessage'

SELECT	'Return Value' = @return_value

*/
CREATE PROCEDURE [seriea].[squadra_insert_giocatore]
	 
	 @GiocatoreId int,
	 @SquadraId int,
	 @DataInizio date,

	 @Id int OUTPUT,
	 @ErrorMessage NVARCHAR(4000) output
AS
BEGIN TRY  
	SET NOCOUNT ON;

	declare @num int = 0;
	/* verifico se il giocatore è libero, ovvero se è associato ad una
			-serie estera
			-serie minore
			-svincolato
	oppure non associato a nessuno squadra.*/
	set @num = (select count (*) from [seriea].squadre_lk_giocatori
				where GiocatoreId = @GiocatoreId
				and SquadraId  in (select * from  [dbo].fn_getSquadreIdSvincolati() )
				and DataFine  > @DataInizio
				)
	If(@num = 0)
		begin
			INSERT INTO [seriea].squadre_lk_giocatori (GiocatoreId, SquadraId, DataInizio)
			VALUES (@GiocatoreId, @SquadraId, @DataInizio);
			SET @Id = SCOPE_IDENTITY(); 
			iF(@@RowCount = 0)
				RAISERROR(50002, -1, -1, '[squadra_insert_giocatore]');
			iF(@@RowCount > 1)
				RAISERROR(50003, -1, -1, '[squadra_insert_giocatore]');	
		end
	else
		begin
			RAISERROR(50014, -1, -1, '[squadra_insert_giocatore]');
		end
END TRY  
BEGIN CATCH  
    
	set @ErrorMessage = ERROR_MESSAGE();
	return ERROR_NUMBER();
END CATCH 
GO
/****** Object:  StoredProcedure [seriea].[stagione_all_get]    Script Date: 23/06/2019 11:06:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Giuseppe Aurilio>
-- Create date: <20/06/2019>
-- Description:	restituisce la lista di tutte le stagioni presenti
-- =============================================

/* ESEMPIO DI CHIAMATA
exec [seriea].[stagione_all_get]
*/
CREATE PROCEDURE [seriea].[stagione_all_get]
AS
BEGIN  
	SET NOCOUNT ON;
	select Id, Descrizione
	from seriea.stagioni
END 
GO
/****** Object:  StoredProcedure [seriea].[stagione_insert]    Script Date: 23/06/2019 11:06:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Giuseppe Aurilio>
-- Create date: <02/06/2019>
-- Description:	<inserisce una stagione che non è già presente>
-- =============================================

/* ESEMPIO DI CHIAMATA
DECLARE	@return_value int,
		@Id int,
		@ErrorMessage nvarchar(4000)

EXEC	@return_value = [seriea].[stagione_insert] '2020/2021', @Id = @Id OUTPUT,@ErrorMessage = @ErrorMessage OUTPUT

SELECT	@Id as N'@Id',
		@ErrorMessage as N'@ErrorMessage'

SELECT	'Return Value' = @return_value
*/
CREATE PROCEDURE [seriea].[stagione_insert]
	 @Descrizione nvarchar(50),

	 @Id int  = 0 output,
	 @ErrorMessage NVARCHAR(4000) output
AS
BEGIN TRY  
	SET NOCOUNT ON;

	declare @num int = 0;
	set @num = (select count (*) from [seriea].[stagioni]
	where Descrizione = @Descrizione )
	If(@num = 0)
		begin
			INSERT INTO [seriea].[stagioni] (Descrizione)
			VALUES (@Descrizione);
			
			SET @Id = SCOPE_IDENTITY(); 
		end
	else
		begin
			RAISERROR(50007, -1, -1, '[stagione_insert]');
		end
	
END TRY  
BEGIN CATCH  
	set @ErrorMessage = ERROR_MESSAGE() ;
	return ERROR_NUMBER();
END CATCH 
GO
/****** Object:  StoredProcedure [seriea].[statistica_insert]    Script Date: 23/06/2019 11:06:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Giuseppe Aurilio>
-- Create date: <07/06/2019>
-- Description:	<inserisce una statistica per un {giocatore, stagione} che non è già presente>
-- =============================================

/* ESEMPIO DI CHIAMATA
DECLARE	@return_value int,
		@Id int,
		@ErrorMessage nvarchar(4000)

EXEC	@return_value = [seriea].[statistica_insert]
		@GiocatoreId = 15,
		@StagioneId = 7,
		@Presenze = 12,
		@Giocabili = 12,
		@MediaVoto = 6.5,
		@Fantamedia = 6.5,
		@Id = @Id OUTPUT,
		@ErrorMessage = @ErrorMessage OUTPUT

SELECT	@Id as N'@Id',
		@ErrorMessage as N'@ErrorMessage'

SELECT	'Return Value' = @return_value
*/
CREATE PROCEDURE [seriea].[statistica_insert]
	 @GiocatoreId int,
	 @StagioneId int,
	 @Presenze int,
	 @Giocabili int = null,
	 @MediaVoto float = null,
	 @Fantamedia float = null, 
	 @GolFatti int = null,
	 @GolSubiti int = null,
	 @Autogol int = null,
	 @Assist int = null,
	 @Ammonizioni int = null,
	 @Espulsioni int = null,
	 @RigoriSbagliati int = null,
	 @RigoriTrasformati int = null,
	 @RigoriParati int = null,
	 @RigoriSubiti int = null,

	 @Id int  = 0 output,
	 @ErrorMessage NVARCHAR(4000) output
AS
BEGIN TRY  
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @num int = 0;
	set @num = (select count (*) from [seriea].statistiche
	where StagioneId = @StagioneId AND GiocatoreId = @GiocatoreId )
	If(@num = 0)
		begin
			INSERT INTO [seriea].statistiche (GiocatoreId, StagioneId, Presenze, Giocabili, MediaVoto, Fantamedia,
			GolFatti, GolSubiti, Autogol, Assist, Ammonizioni, Espulsioni, RigoriSbagliati, 
			RigoriTrasformati, RigoriParati, RigoriSubiti)
			VALUES (@GiocatoreId, @StagioneId, @Presenze, @Giocabili, @MediaVoto, @Fantamedia,
			@GolFatti, @GolSubiti, @AutoGol, @Assist, @Ammonizioni, @Espulsioni, @RigoriSbagliati, 
			@RigoriTrasformati, @RigoriParati, @RigoriSubiti);
			
			SET @Id = SCOPE_IDENTITY(); 
		end
	else
		begin
			RAISERROR(50008, -1, -1, '[statistica_insert]');
		end

END TRY  
BEGIN CATCH  
	set @ErrorMessage = ERROR_MESSAGE() ;
	return ERROR_NUMBER();
END CATCH 
GO
/****** Object:  StoredProcedure [seriea].[statistica_update]    Script Date: 23/06/2019 11:06:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Giuseppe Aurilio>
-- Create date: <02/06/2019>
-- Description:	<aggiorna i dati di una statistica per un giocatore in una stagione>
-- =============================================
/* ESEMPIO DI CHIAMATA
DECLARE	@return_value int,
		@ErrorMessage nvarchar(4000)

EXEC	@return_value = [seriea].[statistica_update]
		@Id = 3,
		@Presenze = 33,
		@Giocabili = 34,
		@MediaVoto = 9.4,
		@Fantamedia = 5.66,
		@GolFatti = 3,
		@GolSubiti = 4,
		@Autogol = 5,
		@Assist = 3,
		@Ammonizioni = 2,
		@Espulsioni = 5,
		@RigoriSbagliati = 6,
		@RigoriTrasformati = 3,
		@RigoriParati = 2,
		@RigoriSubiti = 4,
		@ErrorMessage = @ErrorMessage OUTPUT

SELECT	@ErrorMessage as N'@ErrorMessage'

SELECT	'Return Value' = @return_value

*/
CREATE PROCEDURE [seriea].[statistica_update]
	 @Id int,
	
	 @Presenze int,
	 @Giocabili int = null,
	 @MediaVoto float = null,
	 @Fantamedia float = null, 
	 @GolFatti int = null,
	 @GolSubiti int = null,
	 @Autogol int = null,
	 @Assist int = null,
	 @Ammonizioni int = null,
	 @Espulsioni int = null,
	 @RigoriSbagliati int = null,
	 @RigoriTrasformati int = null,
	 @RigoriParati int = null,
	 @RigoriSubiti int = null,
	 @ErrorMessage NVARCHAR(4000) output
AS
BEGIN TRY  
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	Update [seriea].statistiche
	set Presenze = @Presenze,
		Giocabili = @Giocabili ,
		MediaVoto = @MediaVoto ,
		Fantamedia = @Fantamedia , 
		GolFatti = @GolFatti ,
		GolSubiti = @GolSubiti ,
		Autogol = @Autogol ,
		Assist = @Assist ,
		Ammonizioni = @Ammonizioni ,
		Espulsioni = @Espulsioni , 
		RigoriSbagliati = @RigoriSbagliati ,
		RigoriTrasformati = @RigoriTrasformati ,
		RigoriParati = @RigoriParati ,
		RigoriSubiti = @RigoriSubiti 
	where Id = @Id
	iF(@@RowCount = 0)
		RAISERROR(50009, -1, -1, 'statistica_update');

END TRY  
BEGIN CATCH  
	set @ErrorMessage = ERROR_MESSAGE() ;
	return ERROR_NUMBER();
END CATCH 
GO
/****** Object:  StoredProcedure [seriea].[voto_insert]    Script Date: 23/06/2019 11:06:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Giuseppe Aurilio>
-- Create date: <07/06/2019>
-- Description:	<inserisce una statistica per un {giocatore, Giornata} che non è già presente>
-- =============================================

/* ESEMPIO DI CHIAMATA

DECLARE	@return_value int,
		@Id int,
		@ErrorMessage nvarchar(4000)

EXEC	@return_value = [seriea].[voto_insert]
		@GiocatoreId = 15,
		@GiornataId = 3,
		@Voto = 6.5,
		@Fantavoto = 7.5,
		@GolFatti = 0,
		@GolSubiti = 0,
		@Autogol = 0,
		@Assist = 1,
		@Ammonizione = 0,
		@Espulsione = 1,
		@RigoriSbagliati = 1,
		@RigoriTrasformati = 1,
		@RigoriParati = 1,
		@RigoriSubiti = 1,
		@Id = @Id OUTPUT,
		@ErrorMessage = @ErrorMessage OUTPUT

SELECT	@Id as N'@Id',
		@ErrorMessage as N'@ErrorMessage'

SELECT	'Return Value' = @return_value

*/
CREATE PROCEDURE [seriea].[voto_insert]
	 @GiocatoreId int,
	 @GiornataId int,
	 @Voto float,
	 @Fantavoto float, 
	 @GolFatti int = null,
	 @GolSubiti int = null,
	 @Autogol int = null,
	 @Assist int = null,
	 @Ammonizione bit = null,
	 @Espulsione bit = null,
	 @RigoriSbagliati int = null,
	 @RigoriTrasformati int = null,
	 @RigoriParati int = null,
	 @RigoriSubiti int = null,

	 @Id int  = 0 output,
	 @ErrorMessage NVARCHAR(4000) output	
AS
BEGIN TRY  
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	declare @num int = 0;
	set @num = (select count (*) from [seriea].voti
	where GiornataId = @GiornataId AND GiocatoreId = @GiocatoreId )
	If(@num = 0)
		begin
			INSERT INTO [seriea].voti
			(GiocatoreId, GiornataId, Voto, Fantavoto,
			GolFatti, GolSubiti, Autogol, Assist, Ammonizione, Espulsione, RigoriSbagliati, 
			RigoriTrasformati, RigoriParati, RigoriSubiti)
			VALUES (@GiocatoreId, @GiornataId, @Voto, @Fantavoto,
			@GolFatti, @GolSubiti, @Autogol, @Assist, @Ammonizione, @Espulsione, @RigoriSbagliati, 
			@RigoriTrasformati, @RigoriParati, @RigoriSubiti);
			
			SET @Id = SCOPE_IDENTITY(); 
		end
	else
		begin
			RAISERROR(50010, -1, -1, '[voto_insert]');
		end

END TRY  
BEGIN CATCH  
	set @ErrorMessage = ERROR_MESSAGE() ;
	return ERROR_NUMBER();
END CATCH 

GO
/****** Object:  StoredProcedure [seriea].[voto_update]    Script Date: 23/06/2019 11:06:12 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Giuseppe Aurilio>
-- Create date: <02/06/2019>
-- Description:	<aggiorna i dati di un voto per un giocatore in una giornata>
-- =============================================

/* ESEMPIO DI CHIAMATA
DECLARE	@return_value int,
		@ErrorMessage nvarchar(4000)

EXEC	@return_value = [seriea].[voto_update]
		@Id = 3,
		@Voto = 5.4,
		@Fantavoto = 6.4,
		@GolFatti = 3,
		@GolSubiti = 4,
		@Autogol = 4,
		@Assist = 1,
		@Ammonizione = 0,
		@Espulsione = 0,
		@RigoriSbagliati = 4,
		@RigoriTrasformati = 4,
		@RigoriParati = 4,
		@RigoriSubiti = 4,
		@ErrorMessage = @ErrorMessage OUTPUT

SELECT	@ErrorMessage as N'@ErrorMessage'

SELECT	'Return Value' = @return_value

*/
CREATE PROCEDURE [seriea].[voto_update]
	@Id int,
	
	@Voto float,
	@Fantavoto float, 
	@GolFatti int = null,
	@GolSubiti int = null,
	@Autogol int = null,
	@Assist int = null,
	@Ammonizione bit = null,
	@Espulsione bit = null,
	@RigoriSbagliati int = null,
	@RigoriTrasformati int = null,
	@RigoriParati int = null,
	@RigoriSubiti int = null,
	@ErrorMessage NVARCHAR(4000) output
AS

BEGIN TRY  
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	Update [seriea].voti
	set 
		Voto =@Voto,
		Fantavoto = @Fantavoto , 
		GolFatti = @GolFatti ,
		GolSubiti = @GolSubiti ,
		Autogol  = @Autogol ,
		Assist = @Assist ,
		Ammonizione  = @Ammonizione ,
		Espulsione = @Espulsione ,
		RigoriSbagliati = @RigoriSbagliati ,
		RigoriTrasformati = @RigoriTrasformati ,
		RigoriParati = @RigoriParati ,
		RigoriSubiti = @RigoriSubiti 
	where Id = @Id
	iF(@@RowCount = 0)
		RAISERROR(50011, -1, -1, 'voto_update');

END TRY  
BEGIN CATCH  
	set @ErrorMessage = ERROR_MESSAGE() ;
	return ERROR_NUMBER();
END CATCH 
GO
USE [master]
GO
ALTER DATABASE [SusyLeague] SET  READ_WRITE 
GO
