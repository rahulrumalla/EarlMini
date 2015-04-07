using System;
using System.Data.Common;
using SequelocityDotNet;

namespace EarlMini.Core.Data
{
    internal sealed class Repository : IRepository
    {
        public bool SaveMiniUrl( string originalUrl, string fragment, string miniUrl )
        {
            DbConnection connection = Sequelocity.CreateDbConnection( EarlMiniProvider.ConnectionStringName );

            var result = Sequelocity.GetDatabaseCommand( connection )
                .GenerateInsertForSqlServer( new
                {
                    OriginalUrl = originalUrl,
                    OriginalUrlHash = GetSqlBinaryCheckSum( originalUrl ),
                    MiniUrl = miniUrl,
                    Fragment = fragment,
                    FragmentHash = GetSqlBinaryCheckSum( fragment ),
                    CreateDate = DateTime.Now
                }, EarlMiniProvider.TableName )
                .ExecuteScalar<int>();

            bool success = result > 0;

            return success;
        }

        public string GetOriginalUrl( string fragment )
        {
            const string sql = @"
SELECT  em.OriginalUrl
FROM    dbo.EarlMini em
WHERE   em.FragmentHash = BINARY_CHECKSUM(@Fragment) 
";

            DbConnection connection = Sequelocity.CreateDbConnection( EarlMiniProvider.ConnectionStringName );

            var result = Sequelocity
                .GetDatabaseCommand( connection )
                .SetCommandText( sql )
                .AddParameter( "@Fragment", fragment )
                .ExecuteScalar<string>();

            return result;
        }

        public string GetMiniUrl( string url )
        {
            const string sql = @"
SELECT  em.MiniUrl
FROM    dbo.EarlMini em
WHERE   em.OriginalUrlHash = BINARY_CHECKSUM(@Url) 
";

            DbConnection connection = Sequelocity.CreateDbConnection( EarlMiniProvider.ConnectionStringName );

            var result = Sequelocity
                .GetDatabaseCommand( connection )
                .SetCommandText( sql )
                .AddParameter( "@Url", url )
                .ExecuteScalar<string>();

            return result;
        }

        public int GetSqlBinaryCheckSum( string fragment )
        {
            const string sql = @"SELECT BINARY_CHECKSUM(@Fragment)";

            DbConnection connection = Sequelocity.CreateDbConnection( EarlMiniProvider.ConnectionStringName );

            var checksum = Sequelocity
                .GetDatabaseCommand( connection )
                .SetCommandText( sql )
                .AddParameter( "@Fragment", fragment )
                .ExecuteScalar<int>();

            return checksum;
        } 
    }
}
