using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NetApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetApi.Controllers
{
    [Route("/api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly NetApiContext context;
        private readonly IWebHostEnvironment env;

        #region  构造函数,注入依赖.
        public UserController(NetApiContext _context, IWebHostEnvironment _env)
        {
            context = _context;
            env = _env;
        }
        #endregion

        /// <summary>
        /// 获取当前环境
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string GetEnv()
        {
            return $"EnvironmentName:{env.EnvironmentName} \n ContentRootPath:{env.ContentRootPath}";
        }

        /// <summary>
        /// GetInventory
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<inventory> GetInventory()
        {
            return context.inventory.ToList();
        }

    }
}
