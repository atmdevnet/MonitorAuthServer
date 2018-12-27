using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace MonitorAuthServer.Model
{
    public class HasScopeRequirement : AuthorizationHandler<HasScopeRequirement>, IAuthorizationRequirement
    {
        private readonly string _issuer;
        private readonly string _scope;

        public HasScopeRequirement(string scope, string issuer)
        {
            _scope = scope;
            _issuer = issuer;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            // If user does not have the scope claim, get out of here
            if (!context.User.HasClaim(c => c.Type.Equals("scope") && c.Issuer.Equals(_issuer)))
                return Task.CompletedTask;

            // Split the scopes string into an array
            var scopes = context.User.FindFirst(c => c.Type.Equals("scope") && c.Issuer.Equals(_issuer)).Value.Split(' ');

            // Succeed if the scope array contains the required scope
            if (scopes.Any(s => s.Equals(_scope)))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
