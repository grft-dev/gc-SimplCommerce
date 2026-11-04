IF DB_ID(N'SimplCommerceCoreGraft') IS NULL
BEGIN
    CREATE DATABASE [SimplCommerceCoreGraft];
END
GO

USE [SimplCommerceCoreGraft];
GO

IF OBJECT_ID(N'dbo.Core_Country', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Core_Country]
    (
        [Id] NVARCHAR(450) NOT NULL,
        [Name] NVARCHAR(450) NOT NULL,
        [Code3] NVARCHAR(450) NULL,
        [IsBillingEnabled] BIT NOT NULL CONSTRAINT [DF_Core_Country_IsBillingEnabled] DEFAULT (0),
        [IsShippingEnabled] BIT NOT NULL CONSTRAINT [DF_Core_Country_IsShippingEnabled] DEFAULT (0),
        [IsCityEnabled] BIT NOT NULL CONSTRAINT [DF_Core_Country_IsCityEnabled] DEFAULT (1),
        [IsZipCodeEnabled] BIT NOT NULL CONSTRAINT [DF_Core_Country_IsZipCodeEnabled] DEFAULT (1),
        [IsDistrictEnabled] BIT NOT NULL CONSTRAINT [DF_Core_Country_IsDistrictEnabled] DEFAULT (1),
        CONSTRAINT [PK_Core_Country] PRIMARY KEY ([Id])
    );
END
GO

IF OBJECT_ID(N'dbo.Core_StateOrProvince', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Core_StateOrProvince]
    (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [CountryId] NVARCHAR(450) NULL,
        [Code] NVARCHAR(450) NULL,
        [Name] NVARCHAR(450) NOT NULL,
        [Type] NVARCHAR(450) NULL,
        CONSTRAINT [PK_Core_StateOrProvince] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Core_StateOrProvince_Core_Country_CountryId]
            FOREIGN KEY ([CountryId]) REFERENCES [dbo].[Core_Country]([Id])
    );
END
GO

IF OBJECT_ID(N'dbo.Core_District', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Core_District]
    (
        [Id] BIGINT IDENTITY(1,1) NOT NULL,
        [StateOrProvinceId] BIGINT NOT NULL,
        [Name] NVARCHAR(450) NOT NULL,
        [Type] NVARCHAR(450) NULL,
        [Location] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_Core_District] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Core_District_Core_StateOrProvince_StateOrProvinceId]
            FOREIGN KEY ([StateOrProvinceId]) REFERENCES [dbo].[Core_StateOrProvince]([Id])
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Core_Country] WHERE [Id] = N'GC')
BEGIN
    INSERT INTO [dbo].[Core_Country]
        ([Id], [Name], [Code3], [IsBillingEnabled], [IsShippingEnabled], [IsCityEnabled], [IsZipCodeEnabled], [IsDistrictEnabled])
    VALUES
        (N'GC', N'Graftcode Country', N'GCF', 1, 1, 1, 1, 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Core_StateOrProvince] WHERE [Id] = 1001)
BEGIN
    SET IDENTITY_INSERT [dbo].[Core_StateOrProvince] ON;
    INSERT INTO [dbo].[Core_StateOrProvince] ([Id], [CountryId], [Code], [Name], [Type])
    VALUES (1001, N'GC', N'GC-MZ', N'Graft Mazovia', N'Voivodeship');
    SET IDENTITY_INSERT [dbo].[Core_StateOrProvince] OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Core_District] WHERE [Id] = 2001)
BEGIN
    SET IDENTITY_INSERT [dbo].[Core_District] ON;
    INSERT INTO [dbo].[Core_District] ([Id], [StateOrProvinceId], [Name], [Type], [Location])
    VALUES (2001, 1001, N'Graft District', N'Urban', N'52.2297,21.0122');
    SET IDENTITY_INSERT [dbo].[Core_District] OFF;
END
GO
