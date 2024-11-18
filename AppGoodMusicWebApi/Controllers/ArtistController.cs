using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Configuration;
using Models;
using Models.DTO;

using Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using System.Data;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ArtistController : ControllerBase
    {
        readonly IMusicService _service = null;
        readonly ILogger<ArtistController> _logger = null;


        [HttpGet()]
        [ActionName("Read")]
        [ProducesResponseType(200, Type = typeof(RespPageDto<IArtist>))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> Read(string seeded = "true", string flat = "true",
            string filter = null, string pageNr = "0", string pageSize = "10")
        {
            try
            {
                bool argSeeded = bool.Parse(seeded);
                bool argFlat = bool.Parse(flat);
                int argPageNr = int.Parse(pageNr);
                int argPageSize = int.Parse(pageSize);

                _logger.LogInformation($"{nameof(Read)}: {nameof(argSeeded)}: {argSeeded}, {nameof(argFlat)}: {argFlat}, " +
                    $"{nameof(argPageNr)}: {argPageNr}, {nameof(argPageSize)}: {argPageSize}");
                
                var model = await _service.ReadArtistsAsync(argSeeded, argFlat, filter?.Trim()?.ToLower(), argPageNr, argPageSize);     
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(Read)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet()]
        [ActionName("Readitem")]
        [ProducesResponseType(200, Type = typeof(IArtist))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        public async Task<IActionResult> ReadItem(string id = null, string flat = "false")
        {
            try
            {
                var argId = Guid.Parse(id);
                bool argFlat = bool.Parse(flat);

                _logger.LogInformation($"{nameof(ReadItem)}: {nameof(argId)}: {argId}, {nameof(argFlat)}: {argFlat}");
                
                var item = await _service.ReadArtistAsync(argId, argFlat);
                if (item == null) throw new ArgumentException ($"Item with id {id} does not exist");

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(ReadItem)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200, Type = typeof(IArtist))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> DeleteItem(string id)
        {
            try
            {
                var argId = Guid.Parse(id);

                _logger.LogInformation($"{nameof(DeleteItem)}: {nameof(argId)}: {argId}");
                
                var item = await _service.DeleteArtistAsync(argId);
                if (item == null) throw new ArgumentException ($"Item with id {id} does not exist");
        
                _logger.LogInformation($"item {argId} deleted");
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(DeleteItem)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet()]
        [ActionName("ReadItemDto")]
        [ProducesResponseType(200, Type = typeof(ArtistCUdto))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404, Type = typeof(string))]
        public async Task<IActionResult> ReadItemDto(string id = null)
        {
            try
            {
                var argId = Guid.Parse(id);

                _logger.LogInformation($"{nameof(ReadItemDto)}: {nameof(argId)}: {argId}");

                var item = await _service.ReadArtistAsync(argId, false);
                if (item == null) throw new ArgumentException ($"Item with id {id} does not exist");

                var dto = new ArtistCUdto(item);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(ReadItemDto)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ActionName("UpdateItem")]
        [ProducesResponseType(200, Type = typeof(IArtist))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> UpdateItem(string id, [FromBody] ArtistCUdto item)
        {
            try
            {
                var argId = Guid.Parse(id);

                _logger.LogInformation($"{nameof(UpdateItem)}: {nameof(argId)}: {argId}");
                
                if (item.ArtistId != argId) throw new ArgumentException("Id mismatch");

                var model = await _service.UpdateArtistAsync(item);
                _logger.LogInformation($"item {argId} updated");
               
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(UpdateItem)}: {ex.Message}");
                return BadRequest($"Could not update. Error {ex.Message}");
            }
        }

        [HttpPost()]
        [ActionName("CreateItem")]
        [ProducesResponseType(200, Type = typeof(IArtist))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> CreateItem([FromBody] ArtistCUdto item)
        {
            try
            {
                _logger.LogInformation($"{nameof(CreateItem)}:");
                
                var model = await _service.CreateArtistAsync(item);
                _logger.LogInformation($"item {model.ArtistId} created");

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(CreateItem)}: {ex.Message}");
                return BadRequest($"Could not create. Error {ex.Message}");
            }
        }

        //Upsert = Update or Insert
        [HttpPost()]
        [ActionName("UpsertItem")]
        [ProducesResponseType(200, Type = typeof(IArtist))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> UpsertItem([FromBody] ArtistCUdto item)
        {
            try
            {
                _logger.LogInformation($"{nameof(UpsertItem)}:");

                var model = await _service.UpsertArtistAsync(item);
                 _logger.LogInformation($"item {model.ArtistId} upserted");

                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(UpsertItem)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        #region constructors
        public ArtistController(IMusicService service, ILogger<ArtistController> logger)
        {
            _service = service;
            _logger = logger;
        }
        #endregion
    }
}

