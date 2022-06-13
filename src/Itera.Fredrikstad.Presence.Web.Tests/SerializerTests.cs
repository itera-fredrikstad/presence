using Itera.Fredrikstad.Presence.Web.Api.Models;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using Itera.Fredrikstad.Presence.Core;
using Ardalis.SmartEnum.SystemTextJson;

namespace Itera.Fredrikstad.Presence.Web.Tests
{
    public class SerializerTests
    {
        private readonly JsonSerializerOptions _jsonOptions = new();

        public SerializerTests()
        {
            _jsonOptions.Converters.Add(new SmartEnumNameConverter<DayType, int>());
        }

        [Fact]
        public void SerializeThenDeserializeDaySummary()
        {
            var daySummary = new DaySummary(
                    new DateTime(2022, 08, 06),
                    new List<DayAttendee> {
                        new DayAttendee("hei", DayType.Full, "comment")
                    });

            var serializedString = JsonSerializer.Serialize(daySummary, _jsonOptions);
            serializedString.Should().NotBeNullOrWhiteSpace();
            Console.WriteLine(serializedString);

            var deserializedDaySummary = JsonSerializer.Deserialize<DaySummary>(serializedString, _jsonOptions);
            deserializedDaySummary.Should().NotBeNull();
            deserializedDaySummary.Date.Should().Be(daySummary.Date);
            deserializedDaySummary.Attendees.Should().BeEquivalentTo(daySummary.Attendees);
        }

        [Fact]
        public void SerializeThenDeserializeAttendee()
        {
            var attendee = new DayAttendee("hei", DayType.Full, "comment");

            var serializedAttendee = JsonSerializer.Serialize(attendee, _jsonOptions);
            serializedAttendee.Should().NotBeNullOrWhiteSpace();
            Console.WriteLine(serializedAttendee);

            var deserializedAttendee = JsonSerializer.Deserialize<DayAttendee>(serializedAttendee, _jsonOptions);
            deserializedAttendee.Should().NotBeNull();
            deserializedAttendee.Type.Should().Be(attendee.Type);
            deserializedAttendee.UserId.Should().Be(attendee.UserId);
            deserializedAttendee.Comment.Should().Be(attendee.Comment);
        }
    }
}