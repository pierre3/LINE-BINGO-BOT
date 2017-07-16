using BingoWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BingoWebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/game")]
    public class GameController : Controller
    {
        private IBingoApiRepository _repository;

        public GameController(IBingoApiRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Gets a game status
        /// </summary>
        /// <param name="id">Game ID</param>
        /// <returns>Returns a game status.</returns>
        /// <response code="404">Responses if a specified game is not found.</response>
        /// <response code="200">Returns a game status.</response>
        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(GameStatus), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAsync(int id)
        {

            var game = await _repository.FindGameAsync(id);
            if (game == null)
            {
                return NotFound();
            }
            return new JsonResult(game.ToStatus());
        }

        /// <summary>
        /// Create a new game.
        /// </summary>
        /// <param name="param">Set a keyword necessary for creating a new card.</param>
        /// <returns>Returns a new game status.</returns>
        /// <response code="200">Returns a new game status.</response>
        [HttpPost()]
        [ProducesResponseType(typeof(NewGameResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> PostAsync([FromBody] NewGameParameter param)
        {
            var game = await _repository.CreateGameAsync(param?.Keyword ?? "");
            return new JsonResult(new NewGameResult() { GameId = game.Id, AccessKey = game.AccessKey.ToString() });
        }

        /// <summary>
        /// Draws a next number.
        /// </summary>
        /// <param name="id">Game ID</param>
        /// <param name="param">Sets an accessKey.</param>
        /// <returns>Returns a game status.</returns>
        /// <response code="404">Responses if a specified game is not found.</response>
        /// <response code="401">Responses if an access key is invalid.</response>
        /// <response code="200">Returns a game status.</response>
        [HttpPut("{id}/draw")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(GameStatus), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> PutAsync(int id, [FromBody] GameAccessParameter param)
        {
            try
            {
                var game = await _repository.DrawAsync(id, param.AccessKey);
                if (game == null)
                {
                    return NotFound();
                }
                return new JsonResult(game.ToStatus());
            }
            catch (InvalidAccessKeyException)
            {
                return Unauthorized();
            }
        }

        /// <summary>
        /// Delete a game
        /// </summary>
        /// <param name="id">Game ID</param>
        /// <param name="accessKey">Sets an accessKey.</param>
        /// <response code="401">Responses if an access key is invalid.</response>
        /// <response code="200">OK</response>
        [HttpDelete("{id}")]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteAsync(int id, [FromQuery] string accessKey)
        {
            try
            {
                await _repository.DeleteGameAsync(id, accessKey);
                return Ok();
            }
            catch (InvalidAccessKeyException)
            {
                return Unauthorized();
            }
        }
    }
}
