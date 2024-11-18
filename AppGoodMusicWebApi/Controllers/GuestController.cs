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
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.RegularExpressions;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AppWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class GuestController : ControllerBase
    {
        readonly IMusicService _service = null;
        readonly ILogger<GuestController> _logger = null;

        [HttpGet()]
        [ActionName("Info")]
        [ProducesResponseType(200, Type = typeof(GstUsrInfoAllDto))]
        public async Task<IActionResult> Info()
        {
            try {
                var info = await _service.InfoAsync();
                
                _logger.LogInformation($"{nameof(Info)}: {info}");
                return Ok(info);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{nameof(Info)}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        #region constructors
        public GuestController(IMusicService service, ILogger<GuestController> logger)
        {
            _service = service;
            _logger = logger;
        }
        #endregion
    }
}

