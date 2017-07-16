using BingoWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace BingoWebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Card")]
    public class CardController : Controller
    {
        IBingoApiRepository _repository;

        public CardController(IBingoApiRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Add a new card to specified game.
        /// </summary>
        /// <param name="param">Specify a game ID and a keyword.</param>
        /// <returns>Card ID</returns>
        /// <response code="404">Responses if a specified game is not found.</response>
        /// <response code="403">Responses if a keyword is wrong.</response>
        /// <response code="200">Returns a new card ID.</response>
        [HttpPost()]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        [ProducesResponseType(typeof(int), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> PostAsync([FromBody] AddCardParameter param)
        {
            var game = await _repository.FindGameAsync(param.GameId);
            if (game == null)
            {
                return NotFound();
            }

            try
            {
                var cardId = await _repository.AddCardAsync(game, param.Keyword);
                return new JsonResult(cardId);
            }
            catch (InvalidKeywordException)
            {
                return Forbid();
            }
        }

        /// <summary>
        /// Get a card status.
        /// </summary>
        /// <param name="id">Card ID</param>
        /// <returns>Returns a card status.</returns>
        /// <response code="404">Responses if a specified card is not found.</response>
        /// <response code="200">Returns a card status.</response>
        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(CardStatus),(int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetAsync(int id)
        {
            var card = await _repository.FindCardAsync(id);
            if (card == null)
            {
                return NotFound();
            }
            return new JsonResult(card.ToStatus());
        }
    }
}