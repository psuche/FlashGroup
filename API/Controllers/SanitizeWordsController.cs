using API.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.RegularExpressions;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanitizeWordsController : ControllerBase
    {
        private readonly ISensitiveWordsRepository _sensitiveWordsRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SanitizeWordsController> _logger;

        private const string CacheKey = "SensitiveWords";
        private const string NoSensitiveWordsMessage = "No sensitive words found in the repository.";
        private const string EmptyInputMessage = "Input cannot be empty";

        public SanitizeWordsController(ISensitiveWordsRepository sensitiveWordsRepository, IMemoryCache cache, ILogger<SanitizeWordsController> logger)
        {
            _sensitiveWordsRepository = sensitiveWordsRepository;
            _cache = cache;
            _logger = logger;

            _sensitiveWordsRepository.DataChanged += OnSensitiveWordsDataChanged;
        }

        private void OnSensitiveWordsDataChanged(object sender, EventArgs e)
        {
            _logger.LogInformation("Sensitive words data changed, invalidating cache.");
            _cache.Remove(CacheKey);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Sanitize a string", Description = "Sanitizes a string by replacing sensitive words with asterisks.")]
        [SwaggerResponse(200, "OK", typeof(string))]
        [SwaggerResponse(400, "Bad Request")]
        public async Task<IActionResult> Sanitize([FromBody] string request)
        {
            if (string.IsNullOrEmpty(request))
                return BadRequest(EmptyInputMessage);

            //List<string> sensitiveWords;

            //if (!_cache.TryGetValue(cacheKey, out IEnumerable<string> sensitiveWords))
            //if (!_cache.TryGetValue(CacheKey, out List<string> sensitiveWords))
            //if (!_cache.TryGetValue(CacheKey, out HashSet<string> sensitiveWords))
            if (!_cache.TryGetValue(CacheKey, out SortedSet<string> sensitiveWords))
            {
                _logger.LogInformation("Cache miss for sensitive words, fetching from repo.");
                //sensitiveWords = new SortedSet<string>((await _sensitiveWordsRepository.GetAllAsync()).OrderByDescending(x => x.Length));
                sensitiveWords = new SortedSet<string>(await _sensitiveWordsRepository.GetAllAsync(), new LengthComparer());


                //create some options for cache invalidation if you wana be super fancy
                _cache.Set(CacheKey, sensitiveWords);
            }

            if (!sensitiveWords.Any())
            {
                _logger.LogWarning(NoSensitiveWordsMessage);
                return BadRequest(NoSensitiveWordsMessage);
            }

            foreach (var word in sensitiveWords)
            {
                var pattern = $@"\b{Regex.Escape(word)}\b"; //Used chatGPT for regex pattern, always a pain to build these
                request = Regex.Replace(request, pattern, new string('*', word.Length), RegexOptions.IgnoreCase);
            }

            _logger.LogInformation("Sanitize request processed successfully.");
            return Ok(request);
        }

        [HttpPost("batch")]
        [SwaggerOperation(Summary = "Sanitize multiple strings", Description = "Sanitizes multiple strings by replacing sensitive words with asterisks.")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<string>))]
        [SwaggerResponse(400, "Bad Request")]
        public async Task<IActionResult> SanitizeBatch([FromBody] IEnumerable<string> requests)
        {
            if (requests == null || !requests.Any())
                return BadRequest(EmptyInputMessage);

            if (!_cache.TryGetValue(CacheKey, out SortedSet<string> sensitiveWords))
            {
                //sensitiveWords = new HashSet<string>(await _sensitiveWordsRepository.GetAllAsync());
                sensitiveWords = new SortedSet<string>(await _sensitiveWordsRepository.GetAllAsync(), new LengthComparer());
                _cache.Set(CacheKey, sensitiveWords);
            }

            if (!sensitiveWords.Any())
            {
                _logger.LogWarning(NoSensitiveWordsMessage);
                return BadRequest(NoSensitiveWordsMessage);
            }

            var sanitizedInputs = requests.AsParallel().Select(input =>
            {
                foreach (var word in sensitiveWords)
                {
                    var pattern = $@"\b{Regex.Escape(word)}\b";
                    input = Regex.Replace(input, pattern, new string('*', word.Length), RegexOptions.IgnoreCase);
                }
                return input;
            }).ToList();

            _logger.LogInformation("Sanitize batch request processed successfully.");
            return Ok(sanitizedInputs);
        }

        private class LengthComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                int lengthComparison = y.Length.CompareTo(x.Length);
                return lengthComparison != 0 ? lengthComparison : string.Compare(x, y, StringComparison.Ordinal);
            }
        }
    }
}
