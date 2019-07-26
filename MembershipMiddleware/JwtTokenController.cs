using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace JwtTokenMiddleware
{

    public class JwtTokenMiddleware
    {
        private readonly RequestDelegate next;
        public JwtTokenMiddleware(RequestDelegate next)
        {
            this.next = next;

        }
        public async Task Invoke(HttpContext context)
        {
            //Nel momento in cui un altro middleware produce la risposta
            //emettiamo il token nell'intestazione intestazione X-Token.
            //L'intestazione potrebbe avere un altro nome, l'importante è che
            //sia documentata ai client che useranno la nostra API
            //Ogni richiesta viene generato un nuovo token con validità 20 minuti. 
            //Il client deve decidere se quando sostituire il client chiamante.
            context.Response.OnStarting(() =>
            {
                var identity = context.User.Identity as ClaimsIdentity;
                if (identity.IsAuthenticated)
                {
                    //Il client potrà usare questo nuovo token nella sua prossima richiesta
                    context.Response.Headers.Add("X-Token", CreateTokenForIdentity(identity));
                }
                return Task.CompletedTask;
            });
            await next.Invoke(context);
        }

        private StringValues CreateTokenForIdentity(ClaimsIdentity identity)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MiaChiaveSegreta"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(issuer: "Issuer",
                                             audience: "Audience",
                                             claims: identity.Claims,
                                             expires: DateTime.Now.AddMinutes(20),
                                             signingCredentials: credentials
                                             );
            var tokenHandler = new JwtSecurityTokenHandler();
            var serializedToken = tokenHandler.WriteToken(token);
            return serializedToken;
        }
    }
}