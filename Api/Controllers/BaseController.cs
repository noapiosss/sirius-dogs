using System;
using System.Threading;
using System.Threading.Tasks;

using Contracts.Http;

using Microsoft.AspNetCore.Mvc;

using Npgsql;

namespace Api.Controllers
{
    public class BaseController : ControllerBase
    {
        protected async Task<IActionResult> SafeExecute(Func<Task<IActionResult>> action, CancellationToken cansellationToken)
        {
            try
            {
                return await action();
            }
            catch (InvalidOperationException ioe) when (ioe.InnerException is NpgsqlException)
            {
                ErrorResponse response = new()
                {
                    Code = ErrorCode.DbFailureError,
                    Message = "DB failure"
                };

                return ToActionResult(response);
            }
            catch (Exception)
            {
                ErrorResponse response = new()
                {
                    Code = ErrorCode.InternalServerError,
                    Message = "Unhandled error"
                };

                return ToActionResult(response);
            }
        }

        protected IActionResult ToActionResult(ErrorResponse errorResponse)
        {
            return StatusCode((int)errorResponse.Code / 100, errorResponse);
        }
    }
}