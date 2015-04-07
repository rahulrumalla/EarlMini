USE WorkDB
CREATE TABLE [EarlMini]
(
	[EarlMiniId] BIGINT PRIMARY KEY IDENTITY (1,1) NOT NULL,
	[OriginalUrl] VARCHAR(2083) NOT NULL,
	[OriginalUrlHash] INT NOT NULL,
	[MiniUrl] VARCHAR(64) NOT NULL,
	[Fragment] CHAR(8) COLLATE Latin1_General_CS_AS NOT NULL, -- For Case Sensitivity
	[FragmentHash] INT NOT NULL, 
	[CreateDate] [DATETIME] NOT NULL
)