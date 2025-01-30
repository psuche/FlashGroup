using API.Controllers;
using API.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests
{
    public class SanitizeWordsControllerTests
    {
        private readonly Mock<ISensitiveWordsRepository> _mockSensitiveWordsRepository;
        private readonly IMemoryCache _cache;
        //private readonly Mock<IMemoryCache> _mockCache;
        private readonly ILogger<SanitizeWordsController> _logger;
        private readonly SanitizeWordsController _sanitizeWordsController;

        public SanitizeWordsControllerTests()
        {
            _mockSensitiveWordsRepository = new Mock<ISensitiveWordsRepository>();
            //_mockCache = new Mock<IMemoryCache>();
            _cache = new MemoryCache(new MemoryCacheOptions());
            _logger = new LoggerFactory().CreateLogger<SanitizeWordsController>();
            _sanitizeWordsController = new SanitizeWordsController(_mockSensitiveWordsRepository.Object, _cache, _logger);

        }
        
        [Fact]
        public async Task Sanitize_ReturnsBadRequest_WhenInputIsEmpty()
        {
            // Act
            var result = await _sanitizeWordsController.Sanitize(string.Empty);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Input cannot be empty", badRequestResult.Value);
        }

        [Fact]
        public async Task Sanitize_ReturnsBadRequest_WhenInputIsNull()
        {
            // Act
            var result = await _sanitizeWordsController.Sanitize(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Input cannot be empty", badRequestResult.Value);
        }

        [Fact]
        public async Task Sanitize_ReturnsOkResult_WithSanitizedString()
        {
            // Arrange
            var input = "Pran test string sElect * frOm when I add things here";
            var words = new List<string> { "seleCt * from", "aDd" };
            _mockSensitiveWordsRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(words);

            // Act
            var result = await _sanitizeWordsController.Sanitize(input);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<string>(okResult.Value);
            Assert.Equal("Pran test string ************* when I *** things here", returnValue);
        }

        [Fact]
        public async Task Sanitize_ReturnsOkResult_WhenNoSensitiveWordsInInput()
        {
            // Arrange
            var input = "This is a clean string";
            var words = new List<string> { "seleCt * from", "aDd" };
            _mockSensitiveWordsRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(words);

            // Act
            var result = await _sanitizeWordsController.Sanitize(input);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<string>(okResult.Value);
            Assert.Equal(input, returnValue);
        }


        [Fact]
        public async Task Sanitize_ReturnsBadRequest_WhenSensitiveWordsListIsEmpty()
        {
            // Arrange
            var input = "Pran test string sElect * frOm when I add things here";
            var words = new List<string>();
            _mockSensitiveWordsRepository.Setup(x => x.GetAllAsync()).ReturnsAsync(words);

            // Act
            var result = await _sanitizeWordsController.Sanitize(input);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No sensitive words found in the repository.", badRequestResult.Value);
        }


        //[Fact]
        //public async Task Sanitize_UsesCache_WhenAvailable()
        //{
        //    // Arrange
        //    var input = "Pran test string sElect * frOm when I add things here";
        //    var words = new List<string> { "seleCt * from", "aDd" };
        //    //_mockCache.Object.Set("SensitiveWords", words);
        //    //_mockCache.Setup(x => x.TryGetValue("SensitiveWords", out words)).Returns(true);
        //    object cacheEntry;
        //    _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheEntry))
        //              .Returns((object key, out object value) =>
        //              {
        //                  value = words;
        //                  return true;
        //              });

        //    // Act
        //    var result = await _sanitizeWordsController.Sanitize(input);

        //    // Assert
        //    var okResult = Assert.IsType<OkObjectResult>(result);
        //    var returnValue = Assert.IsType<string>(okResult.Value);
        //    Assert.Equal("Pran test string ************* when I *** things here", returnValue);
        //}

    }
}
