using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DASHServer.Service;
using System.IO;
using System.Xml;

namespace DASHServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly ILogger<VideoController> _logger;
        public VideoController(ILogger<VideoController> logger)
        {
            _logger = logger;
        }

        [HttpGet("Sample")]
        public async Task<IActionResult> Get()
        {
            XMLService xmlservice =  new XMLService();
            XmlDocument xmlfile = xmlservice.getXMLBase64ClipInfo("D:\\Desktop\\Capstone\\Sample.mp4");
            return Ok(xmlfile);

        }
    }

    


}
