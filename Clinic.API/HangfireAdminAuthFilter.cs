using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace Clinic.API;

// بيسمح بفتح /hangfire لليوزر المتسجّل فعلاً وDone عليه بصلاحية Admin بس
public sealed class HangfireAdminAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
               && httpContext.User.IsInRole("Admin");
    }
}