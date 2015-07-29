using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Ploeh.AutoFixture;

using Xunit;

namespace TwoWayMapper.Test
{
    public class ExampleTest
    {
        private readonly IFixture fixture = new Fixture();

        [Fact]
        public void Map_WhenInvoked_ShouldWork()
        {
            // Arrange
            var mapper = new Mapper<Schedule, ScheduleModel>()
                // Straigt mapping
                .Map(e => e.Title, e => e.Title)
                // Mapping properties with different names
                .Map((e, i) => e.Tasks[i].Title, (e, i) => e.Tasks[i].DisplayName)
                // Mapping string <-> decimal, string-to-primitive-type converter is picked up automatically
                .Map((e, i) => e.Tasks[i].Budget, (e, i) => e.Tasks[i].Budget)
                // Mapping array of convertible types supported too
                .Map((e, i, j) => e.Tasks[i].Points[j], (e, i, j) => e.Tasks[i].Points[j])
                // Two-level nesting is supported too
                .Map((e, i, j) => e.Tasks[i].Responsible[j].Id, (e, i, j) => e.Tasks[i].ResponsibleUsers[j].Id)
                .Map((e, i, j) => e.Tasks[i].Responsible[j].DisplayName, (e, i, j) => e.Tasks[i].ResponsibleUsers[j].Display);

            var schedule = fixture.Create<Schedule>();

            // Act 
            // From entity to model
            var scheduleModel = mapper.Map(schedule);
            // And from model to entity
            var newSchedule = mapper.Map(scheduleModel);


            // Assert
            newSchedule.Title.Should().Be(schedule.Title);
            newSchedule.Tasks.Should().NotBeNull();
            newSchedule.Tasks.Length.Should().Be(schedule.Tasks.Length);

            for (var i = 0; i < newSchedule.Tasks.Length; i++)
            {
                newSchedule.Tasks[i].Budget.Should().Be(schedule.Tasks[i].Budget);
                newSchedule.Tasks[i].Title.Should().Be(schedule.Tasks[i].Title);
                newSchedule.Tasks[i].Points.Length.Should().Be(schedule.Tasks[i].Points.Length);

                for (var j = 0; j < newSchedule.Tasks[i].Points.Length; j++)
                    newSchedule.Tasks[i].Points[j].Should().BeCloseTo(schedule.Tasks[i].Points[j], 1000);


                newSchedule.Tasks[i].Responsible.Count.Should().Be(schedule.Tasks[i].Responsible.Count);
                for (var j = 0; j < newSchedule.Tasks[i].Responsible.Count; j++)
                {
                    newSchedule.Tasks[i].Responsible[j].DisplayName.Should().Be(newSchedule.Tasks[i].Responsible[j].DisplayName);
                    newSchedule.Tasks[i].Responsible[j].Id.Should().Be(newSchedule.Tasks[i].Responsible[j].Id);
                }
            }
        }


        class User
        {
            public int Id { get; set; }

            public string DisplayName { get; set; }
        }

        class UserModel
        {
            public int Id { get; set; }

            public string Display { get; set; }
        }

        class Task
        {
            public string Title { get; set; }

            public decimal Budget { get; set; }

            public DateTime[] Points { get; set; }

            public List<User> Responsible { get; set; }
        }

        class TaskModel
        {
            public string DisplayName { get; set; }

            public string Budget { get; set; }

            public string[] Points { get; set; }

            public UserModel[] ResponsibleUsers { get; set; }
        }


        class Schedule
        {
            public string Title { get; set; }

            public Task[] Tasks { get; set; }
        }

        class ScheduleModel
        {
            public string Title { get; set; }

            public List<TaskModel> Tasks { get; set; }
        }
    }
}