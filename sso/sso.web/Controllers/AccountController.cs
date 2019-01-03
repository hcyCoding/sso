using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using sso.web.Filter;
using sso.web.Infrastructure.Status;
using sso.web.Service;

namespace sso.web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [RequestOperationFilter]
        public IActionResult Index()
        {
            return View();
        }

        [RequestOperationFilter]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// 用户登陆
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(string Name, string Password)
        {
            if (string.IsNullOrEmpty(Name))
            {
                return Json(new ResponseModel(ResponseStatus.ErrorParameters, "用户名不可为空！"));
            }
            if (string.IsNullOrEmpty(Password))
            {
                return Json(new ResponseModel(ResponseStatus.ErrorParameters, "密码不可为空！"));
            }

            string redirect = "";
            var result = _accountService.Login(Name, Password, HttpContext, out redirect);
            return Json(result);
        }

        /// <summary>
        /// 验证Token
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [EnableCors("AllowOrigin")]
        [IdentityTokenFilter]
        public ActionResult Identity()
        {
            return Json(new ResponseModel(ResponseStatus.OK, "验证成功！"));
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [EnableCors("AllowOrigin")]
        [IdentityTokenFilter]
        public ActionResult GetUserName(string Token)
        {
            if (string.IsNullOrEmpty(Token))
            {
                return Json(new ResponseModel(ResponseStatus.ErrorParameters, "缺少参数Token！"));
            }
            var result = _accountService.GetUserName(Token, HttpContext);
            return Json(result);
        }
    }
}