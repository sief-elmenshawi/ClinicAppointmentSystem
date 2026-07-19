using Clinic.Application.Interfaces;
using Clinic.Domain.Entities;
using MediatR;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Clinic.Application.Common.Behaviors;

public class AuditLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    private static readonly string[] SensitiveFieldNames = { "password", "token", "secret" };

    public AuditLoggingBehavior(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var isCommand = requestName.EndsWith("Command");

        if (!isCommand)
            return await next(cancellationToken);

        var isSuccessful = true;

        try
        {
            return await next(cancellationToken);
        }
        catch
        {
            isSuccessful = false;
            throw;
        }
        finally
        {
            var log = new AuditLog
            {
                UserId = _currentUserService.UserId ?? "Anonymous",
                RequestName = requestName,
                RequestData = SerializeSafely(request),
                IsSuccessful = isSuccessful
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private static string SerializeSafely(TRequest request)
    {
        var jsonNode = JsonSerializer.SerializeToNode(request);

        if (jsonNode is JsonObject jsonObject)
        {
            foreach (var property in jsonObject.ToList())
            {
                if (SensitiveFieldNames.Any(sensitive =>
                        property.Key.Contains(sensitive, StringComparison.OrdinalIgnoreCase)))
                {
                    jsonObject[property.Key] = "***REDACTED***";
                }
            }
        }

        return jsonNode?.ToJsonString() ?? string.Empty;
    }
}