﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using GitLabApiClient.Http;
using GitLabApiClient.Models.Merges;
using GitLabApiClient.Utilities;
using Newtonsoft.Json;

namespace GitLabApiClient
{
    /// <summary>
    /// Used to query GitLab API to retrieve, modify, accept, create merge requests.
    /// Every API call to merge requests must be authenticated.
    /// <exception cref="GitLabException">Thrown if request to GitLab API does not indicate success</exception>
    /// <exception cref="HttpRequestException">Thrown if request to GitLab API fails</exception>
    /// </summary>
    public sealed class MergeRequestsClient
    {
        private readonly GitLabHttpFacade _httpFacade;
        private readonly MergeRequestsQueryBuilder _mergeRequestsQueryBuilder;
        private readonly ProjectMergeRequestsQueryBuilder _projectMergeRequestsQueryBuilder;

        internal MergeRequestsClient(
            GitLabHttpFacade httpFacade,
            MergeRequestsQueryBuilder mergeRequestsQueryBuilder,
            ProjectMergeRequestsQueryBuilder projectMergeRequestsQueryBuilder)
        {
            _httpFacade = httpFacade;
            _mergeRequestsQueryBuilder = mergeRequestsQueryBuilder;
            _projectMergeRequestsQueryBuilder = projectMergeRequestsQueryBuilder;
        }

        /// <summary>
        /// Retrieves merge request from a project.
        /// By default returns opened merged requests created by anyone.
        /// </summary>
        /// <param name="projectId">Id of the project.</param>
        /// <param name="options">Merge requests retrieval options.</param>
        /// <returns>Merge requests satisfying options.</returns>
        public async Task<IList<MergeRequest>> GetAsync(int projectId, Action<ProjectMergeRequestsQueryOptions> options = null)
        {
            var projectMergeRequestOptions = new ProjectMergeRequestsQueryOptions(projectId);
            options?.Invoke(projectMergeRequestOptions);

            string query = _mergeRequestsQueryBuilder.
                Build($"/projects/{projectId}/merge_requests", projectMergeRequestOptions);

            return await _httpFacade.GetPagedList<MergeRequest>(query);
        }

        /// <summary>
        /// Retrieves merge request from all projects the authenticated user has access to.
        /// By default returns opened merged requests created by anyone.
        /// </summary>
        /// <param name="options">Merge requests retrieval options.</param>
        /// <returns>Merge requests satisfying options.</returns>
        public async Task<IList<MergeRequest>> GetAsync(Action<MergeRequestsQueryOptions> options = null)
        {
            var projectMergeRequestOptions = new MergeRequestsQueryOptions();
            options?.Invoke(projectMergeRequestOptions);

            string query = _projectMergeRequestsQueryBuilder.
                Build("/merge_requests", projectMergeRequestOptions);

            return await _httpFacade.GetPagedList<MergeRequest>(query);
        }

        /// <summary>
        /// Creates merge request.
        /// </summary>
        /// <returns>The newly created merge request.</returns>
        public async Task<MergeRequest> CreateAsync(CreateMergeRequest request) => 
            await _httpFacade.Post<MergeRequest>($"/projects/{request.ProjectId}/merge_requests", request);

        /// <summary>
        /// Updated merge request
        /// </summary>
        /// <returns>The updated merge request</returns>
        public async Task<MergeRequest> UpdateAsync(UpdateMergeRequest request) => 
            await _httpFacade.Put<MergeRequest>($"/projects/{request.ProjectId}/merge_requests/{request.MergeRequestId}", request);

        /// <summary>
        /// Accepts merge request.
        /// <returns>The accepted merge request.</returns>
        /// </summary>
        public async Task<MergeRequest> AcceptAsync(int projectId, int mergeRequestId, string mergeCommitMessage)
        {
            Guard.NotEmpty(mergeCommitMessage, nameof(mergeCommitMessage));

            var commitMessage = new MergeCommitMessage
            {
                Message = mergeCommitMessage
            };

            return await _httpFacade.Put<MergeRequest>(
                $"/projects/{projectId}/merge_requests/{mergeRequestId}/merge", commitMessage);
        }

        /// <summary>
        /// Deletes merge request.
        /// </summary>
        public async Task DeleteAsync(int projectId, int mergeRequestId) =>
            await _httpFacade.Delete($"/projects/{projectId}/merge_requests/{mergeRequestId}");

        private class MergeCommitMessage
        {
            [JsonProperty("merge_commit_message")]
            public string Message { get; set; }
        }
    }
}
