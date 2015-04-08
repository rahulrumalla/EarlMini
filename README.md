## EarlMini
**EarlMini** or "Url Mini" is a stand-alone Url Shortener Software Solution built on the .NET framework.

The idea behind this project is to provide anyone a full-suite of lightwieght software artifacts to host their own implementation of EarlMini.

Broadly speaking, this suite of artifacts comprise of

1. Core EarlMini Library, that caters to customized configuration and functionality. 
2. Test Suite that tries to bullet-proof the system and which can be leveraged as part of continuous integration.
3. Lastly, a lightweight website / API ( .NET WEB API ) that can serve the request to minify or expand a URL and also redirect the caller to the original Url.

##FEATURES
**EarlMini** uses a **case-sensitive alphanumeric** set to generate a unique **8-character fragment** that is used to for a **mini url**. 
For example,
The url  `http://azure.microsoft.com/blog/2015/04/08/microsoft-unveils-new-container-technologies-for-the-next-generation-cloud` can be represented by `http://url.mini/aEg7vzum`. The latter would direct the user to the original url


## INSTALLATION & CONFIGURATION 
The package is available via NuGet under the package name *EarlMini*, at *(  coming soon ...  )*

The client needs to configure the following before using *EarlMini*
- The connection string to the server where the url-data can be persisited ( Currently Supports Sql Server Only )
- The destination table name 
- The host name of the 'site' that redirects
This can be achieved by calling the follwing method
```cs
const string connectionStringName = "MyConnectionStringName";
const string tableName = "MyTableName";
const string hostName = "my.site";

EarlMiniProvider.InitializeConfiguration( connectionStringName, tableName, hostName );
```

The schema for the table in SqlServer can be found in the /Files folder.
Make sure to make the necessary customizations before setting up
```sql
USE MyDatabase
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
```

## USAGE

To Minify a Url:
--
Using the *EarlMini.Core*,
```cs
string miniUrl = EarlMiniProvider.MinifyUrl( "http://azure.microsoft.com/blog/2015/04/08/microsoft-unveils-new-container-technologies-for-the-next-generation-cloud" );
```

Via the *EarlMini API*
```json
https://url.mini/Minify

{"url": "http://azure.microsoft.com/blog/2015/04/08/microsoft-unveils-new-container-technologies-for-the-next-generation-cloud" }
```

To Expand a *MiniUrl*:
--
Using the *EarlMini.Core*,
```cs
string miniUrl = EarlMiniProvider.ExpandUrl( "http://url.mini/aEg7vzum" );
```

Via the *EarMini API* or *HTTP GET*
```json
https://url.mini/v1/api/EarlMini/Expand?miniUrl=http://url.mini/aEg7vzum
```

##SITE REDIRECTION
The API also acts as the place-holder to 'Redirect' the user to the original when the 'MiniUrl' is visited.
For example,
The url `http://url.mini/aEg7vzum` Would get redirected to `http://azure.microsoft.com/blog/2015/04/08/microsoft-unveils-new-container-technologies-for-the-next-generation-cloud`

## CONTRIBUTING

1. Fork it!
2. Create your feature branch: `git checkout -b my-new-feature`
3. Commit your changes: `git commit -am 'Add some feature'`
4. Push to the branch: `git push origin my-new-feature`
5. Submit a pull request
