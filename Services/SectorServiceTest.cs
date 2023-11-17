using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PersonalSectorManager.Models;
using PersonalSectorManager.Service;

namespace PersonalSectorManager.Tests.Services
{
	public class SectorServiceTest
	{
        [Fact]
        public void RetrieveHierarchicalSectors_ShouldReturnHierarchicalList()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ProfileDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase02")
                .Options;

            using (var context = new ProfileDBContext(options))
            {
                // Populate the in-memory database with test data
                context.Sectors.Add(new Sector("Parent1") { SectorId = 1, ParentId = null });
                context.Sectors.Add(new Sector("Parent1-Child1") { SectorId = 2, ParentId = 1 });
                context.Sectors.Add(new Sector("Parent1-Child2") { SectorId = 3, ParentId = 1 });
                context.Sectors.Add(new Sector("Parent1-Child2-Child1") { SectorId = 4, ParentId = 3 });
                context.Sectors.Add(new Sector("Parent2") { SectorId = 5, ParentId = null });
                context.SaveChanges();
            }

            using (var context = new ProfileDBContext(options))
            {
                var mockLogger = new Mock<ILogger<SectorService>>();
                var sectorService = new SectorService(context, mockLogger.Object);

                // Act
                var hierarchicalList = sectorService.RetrieveHierarchicalSectors();

                // Assert
                Assert.NotNull(hierarchicalList);
                Assert.Equal(5, hierarchicalList.Count);

                Assert.Equal("Parent1", hierarchicalList[0].Name);
                Assert.Equal(0, hierarchicalList[0].Level);
                Assert.True(hierarchicalList[0].Disabled);

                Assert.Equal("Parent1-Child1", hierarchicalList[1].Name);
                Assert.Equal(1, hierarchicalList[1].Level);
                Assert.False(hierarchicalList[1].Disabled);

                Assert.Equal("Parent1-Child2", hierarchicalList[2].Name);
                Assert.Equal(1, hierarchicalList[2].Level);
                Assert.True(hierarchicalList[2].Disabled);

                Assert.Equal("Parent1-Child2-Child1", hierarchicalList[3].Name);
                Assert.Equal(2, hierarchicalList[3].Level);
                Assert.False(hierarchicalList[3].Disabled);

                Assert.Equal("Parent2", hierarchicalList[4].Name);
                Assert.Equal(0, hierarchicalList[4].Level);
                Assert.False(hierarchicalList[4].Disabled);
            }
        }
    }
}

