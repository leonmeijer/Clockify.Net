﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Clockify.Net;
using Clockify.Net.Models.Projects;
using Clockify.Net.Models.TimeEntries;
using Clockify.Tests.Helpers;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;

namespace Clockify.Tests.Tests
{
    public class TimeEntryTests
    {
        private readonly ClockifyClient _client;
        private string _workspaceId;

        public TimeEntryTests()
        {
            _client = new ClockifyClient();
        }

        [OneTimeSetUp]
        public async Task Setup()
        {
            _workspaceId = await SetupHelper.CreateOrFindWorkspaceAsync(_client, "Clockify.NetTestWorkspace");
        }

        // TODO Uncomment when Clockify add deleting workspaces again

        //[OneTimeTearDown]
        //public async Task Cleanup()
        //{
	       // var currentUser = await _client.GetCurrentUserAsync();
	       // var changeResponse =
		      //  await _client.SetActiveWorkspaceFor(currentUser.Data.Id, DefaultWorkspaceFixture.DefaultWorkspaceId);
	       // changeResponse.IsSuccessful.Should().BeTrue();
        //    var workspaceResponse = await _client.DeleteWorkspaceAsync(_workspaceId);
        //    workspaceResponse.IsSuccessful.Should().BeTrue();
        //}

        [Test]
        public async Task FindAllTagsOnWorkspaceAsync_ShouldReturnTagsList()
        {
            var response = await _client.FindAllTagsOnWorkspaceAsync(_workspaceId);
            response.IsSuccessful.Should().BeTrue();
        }

        [Test]
        public async Task CreateTimeEntryAsync_ShouldCreteTimeEntryAndReturnTimeEntryDtoImpl()
        {
            var now = DateTimeOffset.UtcNow;
            var timeEntryRequest = new TimeEntryRequest
            {
                Start = now,
            };
            var createResult = await _client.CreateTimeEntryAsync(_workspaceId, timeEntryRequest);
            createResult.IsSuccessful.Should().BeTrue();
            createResult.Data.Should().NotBeNull();
            createResult.Data.TimeInterval.Start.Should().BeCloseTo(now, 1.Seconds());
        }

        [Test]
        public async Task CreateTimeEntryAsync_NullStart_ShouldThrowArgumentException()
        {
            var timeEntryRequest = new TimeEntryRequest
            {
                Start = null,
            };
            Func<Task> create = () => _client.CreateTimeEntryAsync(_workspaceId, timeEntryRequest);
            await create.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Value cannot be null. (Parameter '{nameof(TimeEntryRequest.Start)}')");
        }

        [Test]
        public async Task GetTimeEntryFromWorkspaceAsync_ShouldReturnTimeEntryDtoImpl()
        {
            var now = DateTimeOffset.UtcNow;
            var timeEntryRequest = new TimeEntryRequest
            {
                Start = now,
                Description = Guid.NewGuid().ToString()
            };
            var createResult = await _client.CreateTimeEntryAsync(_workspaceId, timeEntryRequest);
            createResult.IsSuccessful.Should().BeTrue();
            createResult.Data.Should().NotBeNull();
            var timeEntryId = createResult.Data.Id;

            var getResult = await _client.GetTimeEntryAsync(_workspaceId, timeEntryId);
            getResult.IsSuccessful.Should().BeTrue();
            getResult.Data.Should().NotBeNull();
            getResult.Data.Should().BeEquivalentTo(createResult.Data);
        }

        [Test]
        public async Task UpdateTimeEntryAsync_ShouldUpdateAndTimeEntryDtoImpl()
        {
            var now = DateTimeOffset.UtcNow;
            var timeEntryRequest = new TimeEntryRequest
            {
                Start = now,
            };
            var createResult = await _client.CreateTimeEntryAsync(_workspaceId, timeEntryRequest);
            createResult.IsSuccessful.Should().BeTrue();
            createResult.Data.Should().NotBeNull();
            var timeEntryId = createResult.Data.Id;

            var updateTimeEntryRequest = new UpdateTimeEntryRequest
            {
                Start = now.AddSeconds(-1),
                Billable = true,
            };
            var updateResult = await _client.UpdateTimeEntryAsync(_workspaceId, timeEntryId, updateTimeEntryRequest);
            updateResult.IsSuccessful.Should().BeTrue();
        }

        [Test]
        public async Task UpdateTimeEntryAsync_NullStart_ShouldThrowArgumentException()
        {
            var updateTimeEntryRequest = new UpdateTimeEntryRequest 
            {
                Start = null,
            };
            Func<Task> create = () => _client.UpdateTimeEntryAsync(_workspaceId, "", updateTimeEntryRequest);
            await create.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Value cannot be null. (Parameter '{nameof(TimeEntryRequest.Start)}')");
        }

        [Test]
        public async Task UpdateTimeEntryAsync_NullBillable_ShouldThrowArgumentException()
        {
            var updateTimeEntryRequest = new UpdateTimeEntryRequest 
            {
                Start = DateTimeOffset.UtcNow,
                Billable = null
            };
            Func<Task> create = () => _client.UpdateTimeEntryAsync(_workspaceId, "", updateTimeEntryRequest);
            await create.Should().ThrowAsync<ArgumentException>()
                .WithMessage($"Value cannot be null. (Parameter '{nameof(TimeEntryRequest.Billable)}')");
        }

        [Test]
        public async Task DeleteTimeEntryAsync_ShouldDeleteTimeEntry()
        {
            var now = DateTimeOffset.UtcNow;
            var timeEntryRequest = new TimeEntryRequest
            {
	            Start = now,
                Description = Guid.NewGuid().ToString(),
            };
            var createResult = await _client.CreateTimeEntryAsync(_workspaceId, timeEntryRequest);
            createResult.IsSuccessful.Should().BeTrue();
            var timeEntryId = createResult.Data.Id;

            var deleteResult = await _client.DeleteTimeEntryAsync(_workspaceId, timeEntryId);
            deleteResult.IsSuccessful.Should().BeTrue();
        }

        [Test]
        public async Task FindAllTimeEntriesForUserAsync_ShouldReturnTimeEntryDtoImplList()
        {
            var now = DateTimeOffset.UtcNow;
            var timeEntryRequest = new TimeEntryRequest
            {
                Start = now,
                Description = ""
            };
            var createResult = await _client.CreateTimeEntryAsync(_workspaceId, timeEntryRequest);
            createResult.IsSuccessful.Should().BeTrue();

            var userResponse = await _client.GetCurrentUserAsync();
            userResponse.IsSuccessful.Should().BeTrue();


            var response = await _client.FindAllTimeEntriesForUserAsync(_workspaceId, userResponse.Data.Id, 
	            start: DateTimeOffset.Now.AddDays(-1), 
	            end: DateTimeOffset.Now.AddDays(1));

            response.IsSuccessful.Should().BeTrue();
            response.Data.Should().ContainEquivalentOf(createResult.Data);
        }
        
        [Test]
        public async Task FindAllHydratedTimeEntriesForUserAsync_ShouldReturnHydratedTimeEntryDtoImplList()
        {
            var now = DateTimeOffset.UtcNow;
            var timeEntryRequest = new TimeEntryRequest
            {
                Start = now,
            };
            var createResult = await _client.CreateTimeEntryAsync(_workspaceId, timeEntryRequest);
            createResult.IsSuccessful.Should().BeTrue();

            var userResponse = await _client.GetCurrentUserAsync();
            userResponse.IsSuccessful.Should().BeTrue();


            var response = await _client.FindAllHydratedTimeEntriesForUserAsync(_workspaceId, userResponse.Data.Id, 
                start: DateTimeOffset.Now.AddDays(-1), 
                end: DateTimeOffset.Now.AddDays(1));

            response.IsSuccessful.Should().BeTrue();
            response.Data.Should().Contain(timeEntry => timeEntry.Id.Equals(createResult.Data.Id));
        }

        [Test]
        public async Task FindAllTimeEntriesForProjectAsync_ShouldReturnTimeEntryDtoImplList()
        {
            // Create project
            var projectRequest = new ProjectRequest
            {
                Name = "FindAllTimeEntriesForProjectAsync " + Guid.NewGuid(),
                Color = "#FF00FF"
            };
            var createProject =await _client.CreateProjectAsync(_workspaceId,projectRequest);
            createProject.IsSuccessful.Should().BeTrue();
            createProject.Data.Should().NotBeNull();

            ProjectDtoImpl project = createProject.Data;


            // Create TimeEntries
            var now = DateTimeOffset.UtcNow;
            var timeEntry1Request = new TimeEntryRequest
            {
                ProjectId = project.Id,
                Start = now,
                End = now.AddMinutes(2),
                Description = "TimeEntry1"
            };

            var addTimeEntry1 = await _client.CreateTimeEntryAsync(_workspaceId, timeEntry1Request);
            addTimeEntry1.IsSuccessful.Should().BeTrue();
            addTimeEntry1.Data.Should().NotBeNull();

            TimeEntryDtoImpl timeEntry1 = addTimeEntry1.Data;

            var timeEntry2Request = new TimeEntryRequest
            {
                ProjectId = project.Id,
                Start = now.AddDays(-1),
                End = now.AddMinutes(3),
                Description = "TimeEntry2"
            };


            var addTimeEntry2 = await _client.CreateTimeEntryAsync(_workspaceId, timeEntry2Request);
            addTimeEntry2.IsSuccessful.Should().BeTrue();
            addTimeEntry2.Data.Should().NotBeNull();

            TimeEntryDtoImpl timeEntry2 = addTimeEntry2.Data;


            // Send request

            var response = await _client.FindAllTimeEntriesForProjectAsync(_workspaceId, projectId: project.Id, start: DateTimeOffset.Now.AddDays(-1),
                end: DateTimeOffset.Now.AddDays(1));
            //response.IsSuccessful.Should().BeTrue();
            response.Data.Should().NotBeNull();

            List<TimeEntryDtoImpl> responseContent = response.Data as List<TimeEntryDtoImpl>;

            responseContent.Should().Contain(timeEntry => timeEntry.Id.Equals(addTimeEntry1.Data.Id));
            responseContent.Should().Contain(timeEntry => timeEntry.Id.Equals(addTimeEntry2.Data.Id));


            // Delete created Entities
            
            var deleteTimeEntry1 = await _client.DeleteTimeEntryAsync(_workspaceId, addTimeEntry1.Data.Id);
            var deleteTimeEntry2 = await _client.DeleteTimeEntryAsync(_workspaceId, addTimeEntry2.Data.Id);

            var deleteProject = await _client.DeleteProjectAsync(_workspaceId,createProject.Data.Id);
            
        }
    }
}