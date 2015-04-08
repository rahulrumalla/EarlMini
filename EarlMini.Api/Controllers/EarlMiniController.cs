using System;
using System.Diagnostics;
using System.Net.Http;
using System.Web.Http;
using EarlMini.Core;
using Newtonsoft.Json.Linq;

namespace EarlMini.Api.Controllers
{
    public class EarlMiniController : ApiController
    {
        [HttpPost]
        public IHttpActionResult Minify( [FromBody] JToken jsonBody )
        {
            var url = jsonBody.Value<string>("url");

            try
            {
                if ( string.IsNullOrWhiteSpace( url ) )
                {
                    return BadRequest( "The parameter url is empty" );
                }

                var originalUri = new Uri( url );

                string miniUrl = EarlMiniProvider.MinifyUrl( originalUri );

                if ( !string.IsNullOrWhiteSpace( miniUrl ) )
                {
                    return Ok( new
                    {
                        miniUrl
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

        [System.Web.Http.HttpGet]
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
