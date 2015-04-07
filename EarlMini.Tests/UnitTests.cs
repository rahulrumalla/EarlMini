using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EarlMini.Core;
using NUnit.Framework;

namespace EarlMini.Tests
{
    [TestFixture]
    public class UnitTests
    {
        public const int ExpectedLengthOfFragment = 8;

        [Test]
        public void Can_GenerateUniqueAplhaNumericStrings_OverMillionIterations()
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
        public void Can_GenerateUniqueAplhaNumericStrings_OverMillionIterations_MultiThreaded()
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
    }
}
