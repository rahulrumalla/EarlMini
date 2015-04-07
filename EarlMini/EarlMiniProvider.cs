using System;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using SequelocityDotNet;

namespace UrlMini
{
    public sealed class EarlMiniProvider
    {
        private static string _connectionStringName;

        private static string _tableName;

        private const string CharacterSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private static readonly char[] CharacterSetArray = CharacterSet.ToCharArray();

        private static readonly int CharacterSetLength = CharacterSet.Length;

        private const string SecureMiniUrlTemplate = "https://www.url.mini/{0}";

        private const string UnSecureMiniUrlTemplate = "http://url.mini/{0}";

        private const byte FragmentLength = 8;

        static EarlMiniProvider()
        {
            _connectionStringName = "EarlMini";

            _tableName = "[dbo].[EarlMini]";
        }

        public static void Initialize( string connectionStringName, string tableName )
        {
            _connectionStringName = connectionStringName;

            _tableName = tableName;
        }

        public static string ExpandUrl( string url )
        {
            return ExpandUrl( new Uri( url ) );
        }

        public static string ExpandUrl( Uri url )
        {
            string miniUrlFragment = GetLastSegmentFromUrl(url);
            
            string originalUrl = GetOriginalUrl( miniUrlFragment );

            return originalUrl;
        }

        private static string GetLastSegmentFromUrl(Uri url)
        {
            string lastSegment = url.Segments.Last();

            if (lastSegment.EndsWith(@"/"))
                lastSegment = lastSegment.Substring(0, lastSegment.Length - 1);

            return lastSegment;
        }

        public static Uri MinifyUrl( string url, bool useSecureMiniUrl = false )
        {
            return MinifyUrl( new Uri( url ), useSecureMiniUrl );
        }

        public static Uri MinifyUrl( Uri url, bool useSecureMiniUrl = false )
        {
            bool success;

            string miniUrl;

            int tries = 5;

            do
            {
                string fragment = GenerateFragment();

                miniUrl = String.Format( useSecureMiniUrl ? SecureMiniUrlTemplate : UnSecureMiniUrlTemplate,
                    fragment );

                success = SaveMiniUrl( url.AbsoluteUri, fragment, ref miniUrl );

                tries--;
            } while ( success == false && tries > 0 );

            return new Uri( miniUrl );
        }

        /// <summary>
        /// Generates a thread-safe random string from the character set that this class was initialized with. Default is the AplhaNumeric Character set.
        /// </summary>
        /// <returns></returns>
        public static string GenerateFragment()
        {
            var result = new char[FragmentLength];

            byte index = FragmentLength;

            while ( index-- > 0 )
            {
                result[index] = CharacterSetArray[SafeRandomProvider.GetThreadRandom().Next( CharacterSetLength )];
            }

            return new string( result );
        }

        private static bool SaveMiniUrl( string url, string fragment, ref string miniUrl )
        {
            bool success;

            string alreadyExistingMiniUrl = GetMiniUrl( url );

            if ( string.IsNullOrWhiteSpace( alreadyExistingMiniUrl ) )
            {
                DbConnection connection = Sequelocity.CreateDbConnection( _connectionStringName );

                var result = Sequelocity.GetDatabaseCommand( connection )
                    .GenerateInsertForSqlServer( new
                        {
                            OriginalUrl = url,
                            OriginalUrlHash = GetSqlBinaryCheckSum( url ),
                            MiniUrl = miniUrl,
                            Fragment = fragment,
                            FragmentHash = GetSqlBinaryCheckSum( fragment ),
                            CreateDate = DateTime.Now,
                            CreatedByUser = "system"
                        }, _tableName )
                    .ExecuteScalar<int>();

                success = result > 0;
            }
            else
            {
                miniUrl = alreadyExistingMiniUrl;

                success = true;
            }

            return success;
        }

        private static string GetOriginalUrl( string fragment )
        {
            const string sql = @"
SELECT  em.OriginalUrl
FROM    dbo.EarlMini em
WHERE   em.FragmentHash = BINARY_CHECKSUM(@Fragment) 
";

            DbConnection connection = Sequelocity.CreateDbConnection( _connectionStringName );

            object result = Sequelocity
                .GetDatabaseCommand( connection )
                .SetCommandText( sql )
                .AddParameter( "@Fragment", fragment )
                .ExecuteScalar();

            if ( result != null )
            {
                return result.ToString();
            }

            return string.Empty;
        }

        private static string GetMiniUrl( string url )
        {
            const string sql = @"
SELECT  em.MiniUrl
FROM    dbo.EarlMini em
WHERE   em.OriginalUrlHash = BINARY_CHECKSUM(@Url) 
";

            DbConnection connection = Sequelocity.CreateDbConnection( _connectionStringName );

            object result = Sequelocity
                .GetDatabaseCommand( connection )
                .SetCommandText( sql )
                .AddParameter( "@Url", url )
                .ExecuteScalar();

            if ( result != null )
            {
                return result.ToString();
            }

            return string.Empty;
        }

        private static int GetSqlBinaryCheckSum( string fragment )
        {
            const string sql = @"SELECT BINARY_CHECKSUM(@Fragment)";

            DbConnection connection = Sequelocity.CreateDbConnection( _connectionStringName );

            int checksum = Sequelocity
                .GetDatabaseCommand( connection )
                .SetCommandText( sql )
                .AddParameter( "@Fragment", fragment )
                .ExecuteScalar()
                .ToInt();

            return checksum;
        }
    }
}
