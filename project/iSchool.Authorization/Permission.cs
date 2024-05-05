using Dapper;
using iSchool.Authorization.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Linq;

namespace iSchool.Authorization
{
    public class Permission : DataAccess
    {
        public bool Check(ActionExecutingContext context)
        {
            string domain = context.HttpContext.Request.Host.Host;
            string controller = context.RouteData.Values["controller"].ToString();
            string action = context.RouteData.Values["action"].ToString();
            Guid userID = Guid.Parse(context.HttpContext.User.Identity.Name);
            byte? platformID = connection.ExecuteScalar<byte?>("select id from platform where domain=@domain", new { domain });
            if (platformID == null)
            {
                return false;
            }
            bool result = Check(platformID.Value, controller, action, userID, out string query);
            if (!string.IsNullOrEmpty(query))
            {
                context.HttpContext.Items["PageQuery"] = query;
            }
            return result;
        }
        public bool Check(byte platformID, string controller, string action, Guid userID, out string query)
        {
            query = string.Empty;
            bool result = Check(platformID, controller, action, userID, out List<Query> QueryList);
            query = string.Join(",", QueryList.Select(a => a.Selector));
            return result;
        }
        public bool Check(byte platformID, string controller, string action, Guid userID, out List<Query> query)
        {
            query = connection.Query<Query>(@"select query.* from [function]
left join character_function on character_function.functionID = [function].id
left join admin_character on admin_character.characterID = character_function.characterID
left join character_query on character_query.characterID = admin_character.characterID
left join query on query.id=character_query.queryID and query.functionID = [function].id
where adminID=@userID
and [platformID]=@platformID 
and [controller]=@controller 
and ([action]=@action or [action]='*' or [action] like '%{' + @action + '}%')
group by query.id, query.functionID, query.name, query.selector", new
            {
                platformID = platformID,
                controller = controller,
                action = action,
                userID = userID
            }).AsList();

            if (query.Count == 0)
            {
                return false;
            }
            else
            {
                query = query.Where(a => !string.IsNullOrEmpty(a.Name)).ToList();
                return true;
            }
        }
        public bool Check(byte platformID, string controller, string action, Guid userID)
        {
            return Check(platformID, controller, action, userID, out string query);
        }
    }
}
