using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PersonalSectorManager.Models;
using PersonalSectorManager.Service;
using PersonalSectorManager.ViewModels;

namespace PersonalSectorManager.Tests.Services
{
	public class UserServiceTest
	{
        private readonly ProfileDBContext _dbContext;
        private readonly UserService _userService;


        public UserServiceTest()
        {

            var mockLogger = new Mock<ILogger<UserService>>();
            var options = new DbContextOptionsBuilder<ProfileDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase01")
                .Options;

            _dbContext = new ProfileDBContext(options);
            _userService = new UserService(_dbContext, mockLogger.Object);


        }


        [Fact]
        public void GetUser_WithValidUserId_ShouldReturnUserViewModel()
        {
            // Arrange
            var userId = 10;

            //Saving Sectors to in-memory DB
            _dbContext.Sectors.AddRange(new Sector("Sector 1") { SectorId = 11 },
                new Sector("Sector 2") { SectorId = 22 });
            _dbContext.SaveChanges();

            // Saving User to in-memory DB
            var user = new User("Anjali Nimesha")
            {
                UserId = userId,
                AgreeToTerms = true,
                UserSectors = new List<UserSector>()
                {
                    new UserSector(userId, 11),
                    new UserSector(userId, 22)
                }
            };

            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();

            // Act
            var userViewModel = _userService.GetUser(userId);

            // Assert
            Assert.NotNull(userViewModel);
            Assert.Equal(userId, userViewModel.UserId);
            Assert.Equal("Anjali Nimesha", userViewModel.UserName);
            Assert.True(userViewModel.AgreeToTerms);
            Assert.NotNull(userViewModel.Sectors);
            Assert.Equal(2, userViewModel.Sectors.Count);
            Assert.Equal(11, userViewModel.Sectors[0].Value);
            Assert.Equal("Sector 1", userViewModel.Sectors[0].Name);
            Assert.Equal(22, userViewModel.Sectors[1].Value);
            Assert.Equal("Sector 2", userViewModel.Sectors[1].Name);
        }

        [Fact]
        public void GetUser_WithInvalidUserId_ShouldThrowArgumentNullException()
        {
            // Arrange
            var userId = -1;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _userService.GetUser(userId));
        }

        [Fact]
        public void SaveUser_WithValidUserViewModel_ShouldSaveUserToDatabase()
        {
            // Arrange
            //Saving Sectors to in-memory DB
            _dbContext.Sectors.AddRange(new Sector("Sector 1") { SectorId = 111 },
                new Sector("Sector 2") { SectorId = 222 });
            _dbContext.SaveChanges();

            var userViewModel = new UserViewModel
            {
                UserId = 0,
                UserName = "John Doe",
                AgreeToTerms = true,
                SelectedSectorIds = new List<int> { 111, 222 }
            };

            // Act
            var userId = _userService.SaveUser(userViewModel);

            // Assert
            Assert.NotEqual(0, userId); // User ID should be assigned after saving
            Assert.Equal(userViewModel.UserName, _dbContext.Users.Single(u => u.UserId == userId).UserName);
            Assert.Equal(2, _dbContext.UserSectors.Where(us => us.UserId == userId).ToList().Count);
        }

        [Fact]
        public void SaveUser_WithInValidUserViewModel_ShouldThrowArgumentNullException()
        {
            // Arrange
            var userViewModel = new UserViewModel
            {
                UserId = 0,
                UserName = "John Doe",
                AgreeToTerms = true,
                SelectedSectorIds = new List<int>() // Empty Selected sectors
            };

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _userService.SaveUser(userViewModel));
        }
    }
}

