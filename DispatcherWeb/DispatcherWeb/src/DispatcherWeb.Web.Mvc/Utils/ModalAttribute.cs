using System;
using System.Net;
using Abp.UI;
using DispatcherWeb.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DispatcherWeb.Web.Utils
{
    public class ModalAttribute : ActionFilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            var userFriendlyException = filterContext.Exception as UserFriendlyException;

            if (userFriendlyException != null)
            {
                var parameters = (userFriendlyException as ExtendedUserFriendlyException)?.Parameters;

                var result = new ObjectResult(new
                {
                    UserFriendlyException = new
                    {
                        userFriendlyException.Message,
                        userFriendlyException.Details,
                        parameters,
                    },
                })
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                };

                filterContext.Result = result;
            }
        }
    }
}
