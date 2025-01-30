using API.Repository;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensitiveWordsController : ControllerBase
    {

        private readonly ISensitiveWordsRepository _sensitiveWordsRepository;
        private readonly ILogger<SanitizeWordsController> _logger;

        public SensitiveWordsController(ISensitiveWordsRepository sensitiveWordsRepository)
        {
            _sensitiveWordsRepository = sensitiveWordsRepository;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all sensitive words", Description = "Retrieves a list of all sensitive words.")]
        [SwaggerResponse(200, "OK", typeof(IEnumerable<string>))]
        public async Task<IActionResult> GetAll()
        {
            var wordzz = await _sensitiveWordsRepository.GetAllAsync();
            return Ok(wordzz);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Get a sensitive word by Id", Description = "Retrieves a sensitive word by its Id.")]
        [SwaggerResponse(200, "OK", typeof(string))]
        [SwaggerResponse(404, "Not Found")]
        public async Task<IActionResult> GetSensitiveWordById(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid Id");

            var word = await _sensitiveWordsRepository.GetByIdAsync(id);
            return word == null ? NotFound() : Ok(word);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create a new sensitive word", Description = "Creates a new sensitive word.")]
        [SwaggerResponse(201, "Created", typeof(int))]
        [SwaggerResponse(400, "Bad Request")]
        public async Task<IActionResult> Post([FromBody] string request)
        {
            if (string.IsNullOrEmpty(request))
                return BadRequest("Request cannot be empty");

            var existingWords = await _sensitiveWordsRepository.GetAllAsync();
            if (existingWords.Contains(request))
                return BadRequest("Word already exists");

            var id = await _sensitiveWordsRepository.CreateAsync(request);
            return CreatedAtAction(nameof(GetSensitiveWordById), new { id }, id);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Update a sensitive word", Description = "Updates an existing sensitive word by its ID.")]
        [SwaggerResponse(200, "OK", typeof(int))]
        [SwaggerResponse(400, "Bad Request")]
        [SwaggerResponse(404, "Not Found")]
        public async Task<IActionResult> Put(int id, [FromBody] string request)
        {
            if (id <= 0)
                return BadRequest("Invalid Id");

            if (string.IsNullOrWhiteSpace(request))
                return BadRequest("");

            var existingWord = await _sensitiveWordsRepository.GetByIdAsync(id);
            if (existingWord == null)
                return NotFound();

            var result = await _sensitiveWordsRepository.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Delete a sensitive word", Description = "Deletes a sensitive word by its ID.")]
        [SwaggerResponse(204, "No Content")]
        [SwaggerResponse(404, "Not Found")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid Id");

            var existingWord = await _sensitiveWordsRepository.GetByIdAsync(id);
            if (existingWord == null)
                return NotFound();

            var result = await _sensitiveWordsRepository.DeleteAsync(id);

            return result == 0 ? NotFound() : NoContent();
        }
    }
}
