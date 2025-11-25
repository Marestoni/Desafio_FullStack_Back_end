using Azure.Identity;
using EduGraphScheduler.Application;
using EduGraphScheduler.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Net;

namespace EduGraphScheduler.Infrastructure.Services;

public class MicrosoftGraphService : IMicrosoftGraphService
{
    private readonly MicrosoftGraphSettings _settings;
    private GraphServiceClient? _graphClient;
    private readonly ILogger<MicrosoftGraphService> _logger;

    public MicrosoftGraphService(IOptions<MicrosoftGraphSettings> settings, ILogger<MicrosoftGraphService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    private GraphServiceClient GetGraphServiceClient()
    {
        if (_graphClient != null)
            return _graphClient;

        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };

        var clientSecretCredential = new ClientSecretCredential(
            _settings.TenantId,
            _settings.ClientId,
            _settings.ClientSecret,
            options);

        _graphClient = new GraphServiceClient(clientSecretCredential);
        return _graphClient;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var options = new TokenCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
        };

        var clientSecretCredential = new ClientSecretCredential(
            _settings.TenantId,
            _settings.ClientId,
            _settings.ClientSecret,
            options);

        var tokenRequestContext = new Azure.Core.TokenRequestContext(new[] { "https://graph.microsoft.com/.default" });
        var token = await clientSecretCredential.GetTokenAsync(tokenRequestContext, default);

        return token.Token;
    }

    public async Task<IEnumerable<MicrosoftGraphUser>> GetUsersAsync()
    {
        var graphClient = GetGraphServiceClient();
        var users = new List<MicrosoftGraphUser>();

        try
        {
            var result = await graphClient.Users
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = new[] { "id", "displayName", "givenName", "surname", "mail", "userPrincipalName", "jobTitle", "department", "officeLocation" };
                });

            if (result?.Value != null)
            {
                foreach (var user in result.Value)
                {
                    users.Add(new MicrosoftGraphUser
                    {
                        Id = user.Id ?? string.Empty,
                        DisplayName = user.DisplayName ?? string.Empty,
                        GivenName = user.GivenName ?? string.Empty,
                        Surname = user.Surname ?? string.Empty,
                        Mail = user.Mail ?? string.Empty,
                        UserPrincipalName = user.UserPrincipalName ?? string.Empty,
                        JobTitle = user.JobTitle ?? string.Empty,
                        Department = user.Department ?? string.Empty,
                        OfficeLocation = user.OfficeLocation ?? string.Empty
                    });
                }

                var pageIterator = PageIterator<User, UserCollectionResponse>
                    .CreatePageIterator(graphClient, result, (user) =>
                    {
                        users.Add(new MicrosoftGraphUser
                        {
                            Id = user.Id ?? string.Empty,
                            DisplayName = user.DisplayName ?? string.Empty,
                            GivenName = user.GivenName ?? string.Empty,
                            Surname = user.Surname ?? string.Empty,
                            Mail = user.Mail ?? string.Empty,
                            UserPrincipalName = user.UserPrincipalName ?? string.Empty,
                            JobTitle = user.JobTitle ?? string.Empty,
                            Department = user.Department ?? string.Empty,
                            OfficeLocation = user.OfficeLocation ?? string.Empty
                        });
                        return true;
                    });

                await pageIterator.IterateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users from Microsoft Graph");
            throw;
        }

        return users;
    }

    public async Task<IEnumerable<MicrosoftGraphEvent>> GetUserEventsAsync(string userPrincipalName)
    {
        var graphClient = GetGraphServiceClient();
        var events = new List<MicrosoftGraphEvent>();

        try
        {
            var result = await graphClient.Users[userPrincipalName].Calendar.Events
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = new[] { "id", "subject", "bodyPreview", "start", "end", "location", "isAllDay", "organizer", "lastModifiedDateTime" };
                    requestConfiguration.QueryParameters.Top = 1000;
                });

            if (result?.Value != null)
            {
                foreach (var graphEvent in result.Value)
                {
                    var startDateTime = GetDateTimeFromDateTimeTimeZone(graphEvent.Start);
                    var endDateTime = GetDateTimeFromDateTimeTimeZone(graphEvent.End);

                    events.Add(new MicrosoftGraphEvent
                    {
                        Id = graphEvent.Id ?? string.Empty,
                        Subject = graphEvent.Subject ?? "Sem assunto",
                        BodyPreview = graphEvent.BodyPreview ?? string.Empty,
                        Start = startDateTime,
                        End = endDateTime,
                        Location = graphEvent.Location?.DisplayName ?? string.Empty,
                        IsAllDay = graphEvent.IsAllDay ?? false,
                        OrganizerEmail = graphEvent.Organizer?.EmailAddress?.Address ?? string.Empty,
                        OrganizerName = graphEvent.Organizer?.EmailAddress?.Name ?? string.Empty,
                        LastModifiedDateTime = graphEvent.LastModifiedDateTime?.DateTime ?? DateTime.MinValue
                    });
                }

                var pageIterator = PageIterator<Event, EventCollectionResponse>
                    .CreatePageIterator(graphClient, result, (graphEvent) =>
                    {
                        var startDateTime = GetDateTimeFromDateTimeTimeZone(graphEvent.Start);
                        var endDateTime = GetDateTimeFromDateTimeTimeZone(graphEvent.End);

                        events.Add(new MicrosoftGraphEvent
                        {
                            Id = graphEvent.Id ?? string.Empty,
                            Subject = graphEvent.Subject ?? "Sem assunto",
                            BodyPreview = graphEvent.BodyPreview ?? string.Empty,
                            Start = startDateTime,
                            End = endDateTime,
                            Location = graphEvent.Location?.DisplayName ?? string.Empty,
                            IsAllDay = graphEvent.IsAllDay ?? false,
                            OrganizerEmail = graphEvent.Organizer?.EmailAddress?.Address ?? string.Empty,
                            OrganizerName = graphEvent.Organizer?.EmailAddress?.Name ?? string.Empty,
                            LastModifiedDateTime = graphEvent.LastModifiedDateTime?.DateTime ?? DateTime.MinValue
                        });
                        return true;
                    });

                await pageIterator.IterateAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching events for user {UserPrincipalName}", userPrincipalName);
            throw;
        }

        return events;
    }

    public async Task<bool> UserHasEventsAsync(string userPrincipalName)
    {
        var graphClient = GetGraphServiceClient();

        try
        {
            _logger.LogDebug("Checking if user {UserPrincipalName} has events", userPrincipalName);

            var result = await graphClient.Users[userPrincipalName].Calendar.Events
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = new[] { "id" };
                    requestConfiguration.QueryParameters.Top = 1;
                });

            var hasEvents = result?.Value?.Count > 0;

            _logger.LogDebug("User {UserPrincipalName} has events: {HasEvents}", userPrincipalName, hasEvents);

            return hasEvents;
        }
        catch (ServiceException ex) when (ex.ResponseStatusCode == (int)HttpStatusCode.NotFound)
        {
            _logger.LogDebug("User {UserPrincipalName} not found or has no calendar", userPrincipalName);
            return false;
        }
        catch (ServiceException ex) when (ex.ResponseStatusCode == (int)HttpStatusCode.Forbidden)
        {
            _logger.LogWarning("No permission to access calendar for user {UserPrincipalName}", userPrincipalName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking events for user {UserPrincipalName}", userPrincipalName);
            return false;
        }
    }

    private DateTime GetDateTimeFromDateTimeTimeZone(Microsoft.Graph.Models.DateTimeTimeZone? dateTimeTimeZone)
    {
        if (dateTimeTimeZone == null)
            return DateTime.MinValue;

        if (DateTime.TryParse(dateTimeTimeZone.DateTime, out DateTime result))
            return result;

        return DateTime.MinValue;
    }
}