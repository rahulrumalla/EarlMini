using System;
using System.Data.Common;
using System.Linq;
using SequelocityDotNet;

namespace EarlMini.Core
{
    /// <summary>
    /// The core class that provides the functionality to Minify or Shorten a Url and Expand an already minified Url.
    /// </summary>
    public sealed class EarlMiniProvider
    {
        #region Private Fiels

        private static string _connectionStringName;

        private static string _tableName;

        private static string _hostName;

        private const string CharacterSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private static readonly char[] CharacterSetArray = CharacterSet.ToCharArray();

        private static readonly int CharacterSetLength = CharacterSet.Length;

        private const string SecureMiniUrlTemplate = "https://www.{0}/{1}";

        private const string UnSecureMiniUrlTemplate = "http://{0}/{1}";

        private const byte FragmentLength = 8; 

        #endregion

        #region Constructors
        
        /// <summary>
        /// Set Detaults
        /// </summary>
        static EarlMiniProvider()
        {
            _connectionStringName = "EarlMini";

            _tableName = "[dbo].[EarlMini]";

            _hostName = "url.mini";
        } 

        #endregion

        #region Public Methods

        /// <summary>
        /// Configures the application to use the provided ConnectionString and Table to persist the MiniUrls
        /// </summary>
        /// <param name="connectionStringName">Name of the ConnectionString</param>
        /// <param name="tableName">Name of the Table to persist the MiniUrls</param>
        public static void InitializeConfiguration( string connectionStringName, string tableName )
        {
            _connectionStringName = connectionStringName;

            _tableName = tableName;
        }

        /// <summary>
        /// Configures the application to use the provided ConnectionString and Table to persist the MiniUrls
        /// </summary>
        /// <param name="connectionStringName">Name of the ConnectionString</param>
        /// <param name="tableName">Name of the Table to persist the MiniUrls</param>
        /// <param name="hostName">Name of the host or website that you want to use to construct the miniUrl. For Ex: bit.ly or goo.gl</param>
        public static void InitializeConfiguration( string connectionStringName, string tableName, string hostName )
        {
            _connectionStringName = connectionStringName;

            _tableName = tableName;

            _hostName = hostName;
        }

        /// <summary>
        /// Expands the miniUrl submitted or returns the original Url associated with this miniUrl
        /// </summary>
        /// <param name="miniUrl">The mini url</param>
        /// <returns>The original expanded Url string</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string ExpandUrl( string miniUrl )
        {
            if (string.IsNullOrWhiteSpace(miniUrl))
                throw new ArgumentNullException(miniUrl);

            return ExpandUrl( new Uri( miniUrl ) );
        }

        /// <summary>
        /// Expands the miniUri submitted or returns the original Url associated with this miniUrl
        /// </summary>
        /// <param name="miniUri">The Uri wrapping the mini url</param>
        /// <returns>The original expanded Url string</returns>
        /// <exception cref="ArgumentException"></exception>
        public static string ExpandUrl( Uri miniUri )
        {
            if(miniUri == null || string.IsNullOrWhiteSpace(miniUri.AbsoluteUri))
                throw new ArgumentException("miniUri is null or the url associated is null");

            string miniUrlFragment = GetLastSegmentFromUrl( miniUri );

            string originalUrl = GetOriginalUrl( miniUrlFragment );

            return originalUrl;
        }

        /// <summary>
        /// Gets the last 'Segment' of the Url. If the segment ends with "/", it will be omitted
        /// Segments are seperated by "/".
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static string GetLastSegmentFromUrl( Uri uri )
        {
            if ( uri == null || string.IsNullOrWhiteSpace( uri.AbsoluteUri ) )
                throw new ArgumentException( "uri is null or the url associated is null" );

            string lastSegment = uri.Segments.Last();

            if ( !string.IsNullOrWhiteSpace(lastSegment) && lastSegment.EndsWith( @"/" ) )
                lastSegment = lastSegment.Substring( 0, lastSegment.Length - 1 );

            return lastSegment;
        }

        /// <summary>
        /// Minifies or shortens the supplied urlstring using a randomly generated 8-characted aplhanumeric string or fragment
        /// </summary>
        /// <param name="url">The url string to minify</param>
        /// <param name="useSecureMiniUrl">If true uses 'https://www..' else 'http://..'</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Uri MinifyUrl( string url, bool useSecureMiniUrl = false )
        {
            if(string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException();

            return MinifyUrl( new Uri( url ), useSecureMiniUrl );
        }

        /// <summary>
        /// Minifies or shortens the supplied urlstring using a randomly generated 8-characted aplhanumeric string or fragment
        /// </summary>
        /// <param name="uri">The uri wrapped around the url to minify</param>
        /// <param name="useSecureMiniUrl"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static Uri MinifyUrl( Uri uri, bool useSecureMiniUrl = false )
        {
            if ( uri == null || string.IsNullOrWhiteSpace( uri.AbsoluteUri ) )
                throw new ArgumentException( "uri is null or the url associated is null" );

            bool success;

            string miniUrl;

            int tries = 5;

            do
            {
                string fragment = GenerateFragment();

                miniUrl = String.Format( _hostName, useSecureMiniUrl ? SecureMiniUrlTemplate : UnSecureMiniUrlTemplate, fragment );

                success = SaveMiniUrl( uri.AbsoluteUri, fragment, ref miniUrl );

                tries--;
            } while ( success == false && tries > 0 );

            if (string.IsNullOrWhiteSpace(miniUrl))
                return null;

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

        #endregion

        #region Private Methods

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

        #endregion
    }
}
