using System;
using System.Linq;
using EarlMini.Core.Data;

namespace EarlMini.Core
{
    /// <summary>
    /// The core class that provides the functionality to Minify or Shorten a Url and Expand an already minified Url.
    /// </summary>
    public sealed class EarlMiniProvider
    {
        #region Private Variables

        private const string CharacterSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        
        private static readonly char[] CharacterSetArray = CharacterSet.ToCharArray();

        private static readonly int CharacterSetLength = CharacterSet.Length;

        private static IRepository EarlMiniRepository { get; set; }

        #endregion

        #region Public Variables

        public static string ConnectionStringName { get; private set; }

        public static string TableName { get; private set; }

        public static string HostName { get; private set; }

        public static string SecureMiniUrlTemplate { get { return "https://www.{0}/{1}"; } }

        public static string UnSecureMiniUrlTemplate { get { return "http://{0}/{1}"; } }

        public static byte FragmentLength { get { return 8; } }

        public static int MaxNumberOfTriesToGenerateUniqueFragment { get { return 5; } }

        #endregion

        #region Constructors
        
        /// <summary>
        /// Set Detaults
        /// </summary>
        static EarlMiniProvider()
        {
            ConnectionStringName = "EarlMini";

            TableName = "[dbo].[EarlMini]";

            HostName = "url.mini";
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This can used for testing purposes. Like, "Mock" injections
        /// </summary>
        /// <param name="repository"></param>
        public static void InitializeTestingConfiguration(IRepository repository)
        {
            EarlMiniRepository = repository;
        }

        /// <summary>
        /// Set Detaults
        /// </summary>
        public static void InitializeDefaultConfiguration()
        {
            ConnectionStringName = "EarlMini";

            TableName = "[dbo].[EarlMini]";

            HostName = "url.mini";
        }

        /// <summary>
        /// Configures the application to use the provided ConnectionString and Table to persist the MiniUrls
        /// </summary>
        /// <param name="connectionStringName">Name of the ConnectionString</param>
        /// <param name="tableName">Name of the Table to persist the MiniUrls</param>
        public static void InitializeConfiguration( string connectionStringName, string tableName )
        {
            ConnectionStringName = connectionStringName;

            TableName = tableName;
        }

        /// <summary>
        /// Configures the application to use the provided ConnectionString and Table to persist the MiniUrls
        /// </summary>
        /// <param name="connectionStringName">Name of the ConnectionString</param>
        /// <param name="tableName">Name of the Table to persist the MiniUrls</param>
        /// <param name="hostName">Name of the host or website that you want to use to construct the miniUrl. For Ex: bit.ly or goo.gl</param>
        public static void InitializeConfiguration( string connectionStringName, string tableName, string hostName )
        {
            ConnectionStringName = connectionStringName;

            TableName = tableName;

            HostName = hostName;
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

            string originalUrl = EarlMiniRepository.GetOriginalUrl( miniUrlFragment );

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
        public static string MinifyUrl( string url, bool useSecureMiniUrl = false )
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
        public static string MinifyUrl( Uri uri, bool useSecureMiniUrl = false )
        {
            if ( uri == null || string.IsNullOrWhiteSpace( uri.AbsoluteUri ) )
                throw new ArgumentException( "uri is null or the url associated is null" );

            bool success;

            string miniUrl;

            string originalUrl = uri.AbsoluteUri;

            int tries = MaxNumberOfTriesToGenerateUniqueFragment;

            do
            {
                string fragment = GenerateFragment();

                miniUrl = String.Format( useSecureMiniUrl ? SecureMiniUrlTemplate : UnSecureMiniUrlTemplate, HostName, fragment );

                //Check if this url is already existing
                string alreadyExistingMiniUrl = EarlMiniRepository.GetMiniUrl( originalUrl );

                if ( string.IsNullOrWhiteSpace( alreadyExistingMiniUrl ) )
                {
                    success = EarlMiniRepository.SaveMiniUrl( originalUrl, fragment, miniUrl );
                }
                else
                {
                    miniUrl = alreadyExistingMiniUrl;

                    success = true;
                }

                tries--;
            } while ( success == false && tries > 0 );

            if (success)
                return miniUrl;
            
            return string.Empty;
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
    }
}
