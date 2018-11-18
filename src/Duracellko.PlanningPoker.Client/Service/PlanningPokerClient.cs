﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Duracellko.PlanningPoker.Service
{
    /// <summary>
    /// Objects provides operations of Planning Poker service.
    /// </summary>
    public class PlanningPokerClient : IPlanningPokerClient
    {
        private const string BaseUri = "api/PlanningPokerService/";
        private readonly HttpClient _client;
        private readonly UrlEncoder _urlEncoder = UrlEncoder.Default;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanningPokerClient"/> class.
        /// </summary>
        /// <param name="client">HttpClient used for HTTP communication with server.</param>
        public PlanningPokerClient(HttpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Creates new Scrum team with specified team name and Scrum master name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="scrumMasterName">Name of the Scrum master.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Created Scrum team.
        /// </returns>
        public Task<ScrumTeam> CreateTeam(string teamName, string scrumMasterName, CancellationToken cancellationToken)
        {
            var encodedTeamName = _urlEncoder.Encode(teamName);
            var encodedScrumMasterName = _urlEncoder.Encode(scrumMasterName);
            var uri = $"CreateTeam?teamName={encodedTeamName}&scrumMasterName={encodedScrumMasterName}";

            return GetJsonAsync<ScrumTeam>(uri, cancellationToken);
        }

        /// <summary>
        /// Connects member or observer with specified name to the Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member or observer.</param>
        /// <param name="asObserver">If set to <c>true</c> then connects as observer; otherwise as member.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// The Scrum team the member or observer joined to.
        /// </returns>
        public Task<ScrumTeam> JoinTeam(string teamName, string memberName, bool asObserver, CancellationToken cancellationToken)
        {
            var encodedTeamName = _urlEncoder.Encode(teamName);
            var encodedMemberName = _urlEncoder.Encode(memberName);
            var encodedAsObserver = asObserver.ToString(CultureInfo.InvariantCulture);
            var uri = $"JoinTeam?teamName={encodedTeamName}&memberName={encodedMemberName}&asObserver={encodedAsObserver}";

            return GetJsonAsync<ScrumTeam>(uri, cancellationToken);
        }

        /// <summary>
        /// Reconnects member with specified name to the Scrum team with specified name.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// The Scrum team the member or observer reconnected to.
        /// </returns>
        /// <remarks>
        /// This operation is used to resynchronize client and server. Current status of ScrumTeam is returned and message queue for the member is cleared.
        /// </remarks>
        public Task<ReconnectTeamResult> ReconnectTeam(string teamName, string memberName, CancellationToken cancellationToken)
        {
            var encodedTeamName = _urlEncoder.Encode(teamName);
            var encodedMemberName = _urlEncoder.Encode(memberName);
            var uri = $"ReconnectTeam?teamName={encodedTeamName}&memberName={encodedMemberName}";

            return GetJsonAsync<ReconnectTeamResult>(uri, cancellationToken);
        }

        /// <summary>
        /// Disconnects member from the Scrum team.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Asynchronous operation.
        /// </returns>
        public Task DisconnectTeam(string teamName, string memberName, CancellationToken cancellationToken)
        {
            var encodedTeamName = _urlEncoder.Encode(teamName);
            var encodedMemberName = _urlEncoder.Encode(memberName);
            var uri = $"DisconnectTeam?teamName={encodedTeamName}&memberName={encodedMemberName}";

            return GetJsonAsync<ReconnectTeamResult>(uri, cancellationToken);
        }

        /// <summary>
        /// Signal from Scrum master to starts the estimation.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Asynchronous operation.
        /// </returns>
        public Task StartEstimation(string teamName, CancellationToken cancellationToken)
        {
            var encodedTeamName = _urlEncoder.Encode(teamName);
            var uri = $"StartEstimation?teamName={encodedTeamName}";

            return SendAsync(uri, cancellationToken);
        }

        /// <summary>
        /// Signal from Scrum master to cancels the estimation.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Asynchronous operation.
        /// </returns>
        public Task CancelEstimation(string teamName, CancellationToken cancellationToken)
        {
            var encodedTeamName = _urlEncoder.Encode(teamName);
            var uri = $"CancelEstimation?teamName={encodedTeamName}";

            return SendAsync(uri, cancellationToken);
        }

        /// <summary>
        /// Submits the estimation for specified team member.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="estimation">The estimation the member is submitting.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// Asynchronous operation.
        /// </returns>
        public Task SubmitEstimation(string teamName, string memberName, double estimation, CancellationToken cancellationToken)
        {
            var encodedTeamName = _urlEncoder.Encode(teamName);
            var encodedMemberName = _urlEncoder.Encode(memberName);
            var encodedEstimation = _urlEncoder.Encode(estimation.ToString(CultureInfo.InvariantCulture));
            var uri = $"SubmitEstimation?teamName={encodedTeamName}&memberName={encodedMemberName}&estimation={encodedEstimation}";

            return SendAsync(uri, cancellationToken);
        }

        /// <summary>
        /// Begins to get messages of specified member asynchronously.
        /// </summary>
        /// <param name="teamName">Name of the Scrum team.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="lastMessageId">ID of last message the member received.</param>
        /// <param name="cancellationToken">The cancellation token to cancel operation.</param>
        /// <returns>
        /// List of messages.
        /// </returns>
        public async Task<IList<Message>> GetMessages(string teamName, string memberName, long lastMessageId, CancellationToken cancellationToken)
        {
            var encodedTeamName = _urlEncoder.Encode(teamName);
            var encodedMemberName = _urlEncoder.Encode(memberName);
            var encodedLastMessageId = _urlEncoder.Encode(lastMessageId.ToString(CultureInfo.InvariantCulture));
            var uri = $"GetMessages?teamName={encodedTeamName}&memberName={encodedMemberName}&lastMessageId={encodedLastMessageId}";

            return await GetJsonAsync<List<Message>>(uri, cancellationToken);
        }

        private async Task<T> GetJsonAsync<T>(string requestUri, CancellationToken cancellationToken)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, BaseUri + requestUri))
                {
                    using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken))
                    {
                        if (response.StatusCode == HttpStatusCode.BadRequest && response.Content != null)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            throw new PlanningPokerException(content);
                        }
                        else if (!response.IsSuccessStatusCode)
                        {
                            throw new PlanningPokerException(Client.Resources.PlanningPokerService_UnexpectedError);
                        }

                        var responseContent = await response.Content.ReadAsStringAsync();
                        return Json.Deserialize<T>(responseContent);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new PlanningPokerException(Client.Resources.PlanningPokerService_ConnectionError, ex);
            }
        }

        private async Task SendAsync(string requestUri, CancellationToken cancellationToken)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, BaseUri + requestUri))
                {
                    using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken))
                    {
                        if (response.StatusCode == HttpStatusCode.BadRequest && response.Content != null)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            throw new PlanningPokerException(content);
                        }
                        else if (!response.IsSuccessStatusCode)
                        {
                            throw new PlanningPokerException(Client.Resources.PlanningPokerService_UnexpectedError);
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                throw new PlanningPokerException(Client.Resources.PlanningPokerService_ConnectionError, ex);
            }
        }
    }
}
