using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Models;
using Models.DTO;
using Services;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]

    public class AdminController : ControllerBase
    {
        readonly IMusicService _service = null;
        readonly ILogger<AdminController> _logger;
        readonly DatabaseConnections _dbConnections;

        //GET: api/guest/info
        [HttpGet()]
        [ActionName("Info")]
        [ProducesResponseType(200, Type = typeof(DatabaseConnections.SetupInformation))]
        public IActionResult Info()
        {
            var info = _dbConnections.SetupInfo;

            _logger.LogInformation($"{nameof(Info)}:\n{info.AppEnvironment}, {info.DataConnectionServer}, {info.DataConnectionTag}, {info.SecretSource}");
            return Ok(info);
        }

        //GET: api/admin/seed?count={count}
        [HttpGet()]
        [ActionName("Seed")]
        [ProducesResponseType(200, Type = typeof(GstUsrInfoAllDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> Seed(string count = "100")
        {
            try
            {
                int argCount = int.Parse(count);

                _logger.LogInformation($"{nameof(Seed)}: {nameof(argCount)}: {argCount}");
                GstUsrInfoAllDto info = await _service.SeedAsync(argCount);
                return Ok(info);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //GET: api/admin/removeseed
        [HttpGet()]
        [ActionName("RemoveSeed")]
        [ProducesResponseType(200, Type = typeof(GstUsrInfoAllDto))]
        [ProducesResponseType(400, Type = typeof(string))]
        public async Task<IActionResult> RemoveSeed(string seeded = "true")
        {
            try
            {
                bool argSeeded = bool.Parse(seeded);

                _logger.LogInformation($"{nameof(RemoveSeed)}: {nameof(argSeeded)}: {argSeeded}");
                GstUsrInfoAllDto info = await _service.RemoveSeedAsync(argSeeded);
                return Ok(info);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //GET: api/admin/log
        [HttpGet()]
        [ActionName("Log")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<LogMessage>))]
        public async Task<IActionResult> Log([FromServices] ILoggerProvider _loggerProvider)
        {
            //Note the way to get the LoggerProvider, not the logger from Services via DI
            if (_loggerProvider is InMemoryLoggerProvider cl)
            {
                return Ok(await cl.MessagesAsync);
            }
            return Ok("No messages in log");
        }

        #region constructors
        public AdminController(IMusicService service, ILogger<AdminController> logger, DatabaseConnections dbConnections)
        {
            _service = service;
            _logger = logger;
            _dbConnections = dbConnections;
        }
        #endregion
    }
}

