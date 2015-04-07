using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EarlMini.Core;
using EarlMini.Core.Data;
using NUnit.Framework;
using Moq;

namespace EarlMini.Tests
{
    [TestFixture]
    public class UnitTests
    {
        private const int ExpectedLengthOfFragment = 8;

        private int ExpectedLenthOfUnsecureMiniUrl
        {
            get
            {
                return String.Format(EarlMiniProvider.UnSecureMiniUrlTemplate, EarlMiniProvider.HostName, string.Empty).Length + EarlMiniProvider.FragmentLength;
            }
        }

        private int ExpectedLenthOfSecureMiniUrl
        {
            get
            {
                return String.Format( EarlMiniProvider.SecureMiniUrlTemplate, EarlMiniProvider.HostName, string.Empty ).Length + EarlMiniProvider.FragmentLength;
            }
        }

        private readonly Mock<IRepository> _repositoryMock = new Mock<IRepository>();

        [Test]
        public void Can_GenerateUniqueAplhaNumericStrings_ForMillionIterations()
        {
            var fragments = new List<string>();

            for ( int i = 0 ; i < 1000000 ; i++ )
            {
                var generatedFragment = EarlMiniProvider.GenerateFragment();

                //Debug.WriteLine( generatedFragment );

                fragments.Add( generatedFragment );

                var regex = new Regex( "^[a-zA-Z0-9]*$" );

                Assert.IsNotNullOrEmpty( generatedFragment );
                Assert.IsTrue( generatedFragment.Length == ExpectedLengthOfFragment );
                Assert.IsTrue( generatedFragment.Trim().Length == ExpectedLengthOfFragment );
                Assert.IsTrue( generatedFragment.Replace( " ", "" ).Length == ExpectedLengthOfFragment );
                Assert.IsTrue( regex.IsMatch( generatedFragment ) );
            }

            bool success = fragments.Count == fragments.Distinct().Count();
            
            if ( !success )
            {
                Debug.WriteLine( fragments.Count );
                Debug.WriteLine( fragments.Distinct().Count() );
            }

            Assert.IsTrue( success );
        }

        [Test]
        public void Can_GenerateUniqueAplhaNumericStrings_ForMillionIterations_MultiThreaded()
        {
            var fragments = new BlockingCollection<string>();

            Parallel.For( 0, 1000000, i =>
            {
                var generatedFragment = EarlMiniProvider.GenerateFragment();

                //Debug.WriteLine( generatedFragment );

                fragments.Add( generatedFragment );

                var regex = new Regex( "^[a-zA-Z0-9]*$" );

                Assert.IsNotNullOrEmpty( generatedFragment );
                Assert.IsTrue( generatedFragment.Length == ExpectedLengthOfFragment );
                Assert.IsTrue( generatedFragment.Trim().Length == ExpectedLengthOfFragment );
                Assert.IsTrue( generatedFragment.Replace( " ", "" ).Length == ExpectedLengthOfFragment );
                Assert.IsTrue( regex.IsMatch( generatedFragment ) );
            } );

            bool success = fragments.Count == fragments.Distinct().Count();

            if ( !success )
            {
                Debug.WriteLine( fragments.Count );
                Debug.WriteLine( fragments.Distinct().Count() );
            }

            Assert.IsTrue( success );
        }

        [Test]
        public void Can_InitializeDefaultConfiguration()
        {
            Assert.AreEqual( "EarlMini", EarlMiniProvider.ConnectionStringName );
            Assert.AreEqual( "[dbo].[EarlMini]", EarlMiniProvider.TableName );
            Assert.AreEqual( "url.mini", EarlMiniProvider.HostName );
        }

        [Test]
        public void Can_InitializeCustomConfiguration_ConnectionString_TableName()
        {
            const string connectionStringName = "TestConnStringName";
            const string tableName = "TestTableName";

            EarlMiniProvider.InitializeConfiguration( connectionStringName, tableName );

            Assert.AreEqual( connectionStringName, EarlMiniProvider.ConnectionStringName );
            Assert.AreEqual( tableName, EarlMiniProvider.TableName );

            EarlMiniProvider.InitializeDefaultConfiguration();
        }

        [Test]
        public void Can_InitializeCustomConfiguration_ConnectionString_TableName_HostName()
        {
            const string connectionStringName = "TestConnStringName";
            const string tableName = "TestTableName";
            const string hostName = "testhost.com";

            EarlMiniProvider.InitializeConfiguration( connectionStringName, tableName, hostName );

            Assert.AreEqual( connectionStringName, EarlMiniProvider.ConnectionStringName );
            Assert.AreEqual( tableName, EarlMiniProvider.TableName );
            Assert.AreEqual( hostName, EarlMiniProvider.HostName );

            EarlMiniProvider.InitializeDefaultConfiguration();
        }

        [Test]
        [TestCase( "https://www.google.com" )]
        [TestCase( "https://www.google.com/" )]
        public void Can_GetLastSegmentFromUrl_WithoutAbsolutePath(string url)
        {
            var uri = new Uri(url);

            string lastSegment = EarlMiniProvider.GetLastSegmentFromUrl(uri);

            Assert.AreEqual(string.Empty, lastSegment);
        }

        [Test]
        [TestCase( "https://www.url.mini/abcd1234" )]
        [TestCase( "https://www.url.mini/abcd1234/" )]
        public void Can_GetLastSegmentFromUrl_WithtAbsolutePath( string url )
        {
            var uri = new Uri( url );

            string lastSegment = EarlMiniProvider.GetLastSegmentFromUrl( uri );

            Assert.IsNotNullOrEmpty(lastSegment);
            Assert.IsTrue( !lastSegment.EndsWith("/") );
            Assert.IsTrue( !lastSegment.Contains("/") );
            Assert.AreEqual( lastSegment, "abcd1234" );
        }

        [Test]
        [TestCase( "https://www.google.com" )]
        [TestCase( "https://www.google.com/" )]
        public void Can_MinifyUrl_UsingUnsecureTemplate(string url)
        {
            //Arrange
            _repositoryMock.Setup(x => x.GetSqlBinaryCheckSum(It.IsAny<string>()))
                .Returns(-123456);

            _repositoryMock.Setup(x => x.GetMiniUrl(It.IsAny<string>()))
                .Returns(string.Empty);
            
            _repositoryMock.Setup( x => x.SaveMiniUrl( It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>() ) )
                .Returns(true);

            EarlMiniProvider.InitializeTestingConfiguration(_repositoryMock.Object);

            //Act
            string miniUrl = EarlMiniProvider.MinifyUrl( url );

            //Assert
            Assert.IsNotNullOrEmpty( miniUrl );
            Assert.IsTrue( miniUrl.Length == ExpectedLenthOfUnsecureMiniUrl );
        }

        [Test]
        [TestCase( "https://www.google.com" )]
        [TestCase( "https://www.google.com/" )]
        public void Can_MinifyUrl_UsingSecureTemplate( string url )
        {
            //Arrange
            _repositoryMock.Setup( x => x.GetSqlBinaryCheckSum( It.IsAny<string>() ) )
                .Returns( -123456 );

            _repositoryMock.Setup( x => x.GetMiniUrl( It.IsAny<string>() ) )
                .Returns( string.Empty );

            _repositoryMock.Setup( x => x.SaveMiniUrl( It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>() ) )
                .Returns( true );

            EarlMiniProvider.InitializeTestingConfiguration( _repositoryMock.Object );

            //Act
            string miniUrl = EarlMiniProvider.MinifyUrl( url, true );

            //Assert
            Assert.IsNotNullOrEmpty( miniUrl );
            Assert.IsTrue( miniUrl.Length == ExpectedLenthOfSecureMiniUrl );
        }
    }
}
