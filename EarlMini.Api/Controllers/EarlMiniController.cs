using System;
using System.Diagnostics;
using System.Web.Http;
using EarlMini.Core;

namespace EarlMini.Api.Controllers
{
    public class EarlMiniController : ApiController
    {
        [HttpPost]
        public IHttpActionResult Minify( [FromBody] string url, [FromBody] bool useSecureMiniUrl = false )
        {
            try
            {
                if ( string.IsNullOrWhiteSpace( url ) )
                {
                    return BadRequest( "The parameter url is empty" );
                }

                var originalUri = new Uri( url );

                Uri miniUri = EarlMiniProvider.MinifyUrl( originalUri, useSecureMiniUrl );

                if ( miniUri != null && !string.IsNullOrWhiteSpace( miniUri.AbsoluteUri ) )
                {
                    return Ok( new
                    {
                        miniUrl = miniUri.AbsoluteUri
                    } );
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );

                return InternalServerError( );
            }

            return InternalServerError();
        }

        [HttpGet]
        public IHttpActionResult Expand( string miniUrl )
        {
            try
            {
                if ( !Uri.IsWellFormedUriString( miniUrl, UriKind.Absolute ) )
                {
                    return BadRequest( "The parameter miniUrl's format is invalid" );
                }
                
                var miniUri = new Uri( miniUrl );

                if ( string.IsNullOrWhiteSpace( miniUri.AbsolutePath.Substring( 1 ) ) )
                {
                    return BadRequest( "The parameter miniUrl's format is invalid" );
                }

                string originalUrl = EarlMiniProvider.ExpandUrl( miniUri );

                if ( !string.IsNullOrWhiteSpace( originalUrl ) )
                {
                    return Ok( new
                    {
                        orginalUrl = originalUrl
                    } );
                }
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );

                return InternalServerError( );
            }

            return NotFound();
        }
    }
}
