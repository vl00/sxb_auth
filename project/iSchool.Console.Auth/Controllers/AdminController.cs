using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using iSchool.Authorization.Lib;
using iSchool.Authorization.Models;
using Microsoft.AspNetCore.Mvc;

namespace iSchool.Console.Auth.Controllers
{
    public class AdminController : BaseController
    {
        #region 管理员管理
        public IActionResult List(string account, Guid? characterID, byte? timeSelect, DateTime? timeStart, DateTime? timeEnd, int? page = 1)
        {
            int pageSize = 10;
            int recordCount = connection.ExecuteScalar<int>("select count(*) from admin_info");
            int pageCount = (int)Math.Ceiling((float)recordCount / pageSize);
            page = page > pageCount ? pageCount : page;

            StringBuilder strSql = new StringBuilder();
            strSql.Append(" select * from admin_info left join admin_character on admin_info.id=adminID left join character on admin_character.characterID=character.id where 1=1");
            if (characterID != null)
            {
                strSql.Append(" and admin_character.characterID=@characterID ");
                ViewBag.CharacterID = characterID.Value;
            }
            if (!string.IsNullOrEmpty(account))
            {
                strSql.Append(" and (admin_info.name=@account or admin_info.displayName=@account) ");
                ViewBag.Account = account;
            }
            if (timeSelect != null)
            {
                string timeField = timeSelect.Value == 0 ? "regTime" : timeSelect.Value == 1 ? "loginTime" : "activeTime";
                ViewBag.TimeSelect = timeSelect.Value;
                if (timeStart != null)
                {
                    strSql.Append(" and " + timeField + " >= @timeStart ");
                    ViewBag.TimeStart = timeStart.Value.ToString("yyyy-MM-dd");
                }
                if (timeEnd != null)
                {
                    strSql.Append(" and " + timeField + " <= @timeEnd+1 ");
                    ViewBag.TimeEnd = timeEnd.Value.ToString("yyyy-MM-dd");
                }
            }
            strSql.Append(" order by regTime ");
            //strSql.Append(" offset (@page-1)*@pageSize rows fetch next @pageSize row only");
            Dictionary<Guid, AdminInfo> lookup_a = new Dictionary<Guid, AdminInfo>();
            var result = connection.Query<AdminInfo, Character, AdminInfo>(strSql.ToString(), (a, c) => {
                AdminInfo tmp_a;
                if (!lookup_a.TryGetValue(a.Id, out tmp_a))
                {
                    tmp_a = a;
                    lookup_a.Add(a.Id, tmp_a);
                }
                tmp_a.Character.Add(c);
                return a;
            }, new { characterID, account, timeStart, timeEnd, page, pageSize }, splitOn: "characterID");
            ViewData["CharacterList"] = connection.Query<Character>("select id, name from character order by time").AsList();

            ViewBag.CurrentPage = page;
            ViewBag.PageCount = pageCount;
            ViewBag.ListURL = "/Amdmin/List/?page={{page}}&" + QueryStringRemoveKey("page");
            return View(lookup_a.Values.AsList());
        }
        public IActionResult SetAdminCharacter(Guid[] characterID, Guid adminID )
        {
            connection.Open();
            IDbTransaction transaction = connection.BeginTransaction();
            try
            {
                connection.Execute("delete from admin_character where adminID=@adminID", new { adminID }, transaction);
                List<AdminCharacter> ac = new List<AdminCharacter>();
                foreach (var character in characterID)
                {
                    ac.Add(new AdminCharacter() { AdminId = adminID, CharacterId = character });
                }
                int result = connection.Execute("insert into admin_character (adminID, characterID) values (@AdminId, @CharacterId)", ac, transaction);
                transaction.Commit();
                connection.Close();
                if (result > 0)
                {
                    MemcachedHelper.RemoveKeys("dbo.adminInfo-" + adminID);
                }
                return Json(new
                {
                    status = result > 0 ? 0 : 1
                });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                connection.Close();
                return Json(new { status = 1, errorDescription = ex.Message });
            }
        }
        public IActionResult SetAdminPsw(Guid id, string password)
        {
            bool result = connection.Execute("update admin_info set password=@password, rspw=1 where id=@id", new { id, password = MemcachedHelper.StrToMD5(password) }) > 0;
            return Json(new { status = result ? 0 : 1 });
        }
        #endregion
        #region 角色管理
        public IActionResult CharacterList()
        {
            string strSql = @"select character.*, [function].id, [function].name, [function].platformID, [function].controller, [function].action, platform.* from character
left join character_function on characterid=character.id
left join [function] on [function].id = functionID
left join platform on platform.id = [function].platformid";
            var lookup = new Dictionary<Guid, Character>();
            var result = connection.Query<Character, Function, Platform, Character>(strSql, (c, f, p) => {
                Character tmp;
                if (!lookup.TryGetValue(c.Id, out tmp))
                {
                    tmp = c;
                    lookup.Add(c.Id, tmp);
                }
                if (f != null)
                {
                    f.Platform = p;
                    tmp.Function.Add(f);
                }
                return c;
            }, splitOn: "id");
            return View(lookup.Values.AsList());
        }
        public IActionResult Character(Guid? id)
        {
            Character model = new Character();
            var lookup = new Dictionary<Guid, Character>();
            var lookup_f = new Dictionary<Guid, Function>();
            if (id != null)
            {
                string strSql = @"select character.*, [function].id, [function].name, [function].platformID, [function].controller, [function].action, platform.*, query.* from character
left join character_function on character_function.characterid=character.id
left join [function] on [function].id = functionID
left join platform on platform.id = [function].platformid
left join character_query on character_query.characterid = character.id
left join query on query.id=character_query.queryid and query.functionID=[function].id
where character.id=@id";
                var result = connection.Query<Character, Function, Platform, Query, Character>(strSql, (c, f, p, q) => {
                    Character tmp;
                    if (!lookup.TryGetValue(c.Id, out tmp))
                    {
                        tmp = c;
                        lookup.Add(c.Id, tmp);
                        lookup_f = new Dictionary<Guid, Function>();
                    }
                    if (f != null)
                    {
                        Function tmp_f;
                        if (!lookup_f.TryGetValue(f.Id, out tmp_f))
                        {
                            tmp_f = f;
                            lookup_f.Add(f.Id, tmp_f);
                        }
                        tmp_f.Platform = p;
                        if (q != null)
                        {
                            tmp_f.Query.Add(q);
                        }
                        tmp.Function.Add(f);
                    }
                    return c;
                }, new { id }, splitOn: "id");
                if (lookup.Count > 0)
                {
                    model = lookup.Values.AsList()[0];
                }
            }
            ViewBag.ID = id;
            lookup_f = new Dictionary<Guid, Function>();
            var functions = connection.Query<Function, Platform, Query, Function>(@"select [function].id, [function].name, [function].platformID, [function].controller, [function].action, platform.*, query.* from [function] 
left join platform on platform.id=platformID 
left join query on query.functionID = [function].id
order by platformID, controller, action, selector", (f, p, q) => {
                Function tmp_f;
                if (!lookup_f.TryGetValue(f.Id, out tmp_f))
                {
                    tmp_f = f;
                    lookup_f.Add(f.Id, tmp_f);
                }
                tmp_f.Platform = p;
                if (q != null)
                {
                    tmp_f.Query.Add(q);
                }
                return f;
            }, splitOn: "id").AsList();
            ViewBag.FunctionList = lookup_f.Values.AsList();
            return View(model);
        }
        [HttpPost]
        public IActionResult UpdateCharacter(Character model, Guid[] function, Guid[] query)
        {
            bool add = false;
            if(model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                model.Time = DateTime.Now;
                add = true;
            }
            List<CharacterFunction> cf = new List<CharacterFunction>();
            foreach (var functionID in function)
            {
                cf.Add(new CharacterFunction() { CharacterId = model.Id, FunctionId = functionID });
            }
            List<CharacterQuery> cq = new List<CharacterQuery>();
            foreach (var queryID in query)
            {
                cq.Add(new CharacterQuery() { CharacterId = model.Id, QueryId = queryID });
            }
            connection.Open();
            IDbTransaction transaction = connection.BeginTransaction();
            try
            {
                if (add)
                {
                    connection.Execute("insert into character (id, name, description, time) values (@id, @name, @description, @time)", model, transaction);
                }
                else
                {
                    connection.Execute("update character set name=@name, description=@description where id=@id", model, transaction);
                }
                connection.Execute("delete from character_function where characterID=@characterID", new { characterID = model.Id }, transaction);
                connection.Execute("insert into character_function (characterID, functionID) values (@characterID, @functionID)", cf, transaction);
                connection.Execute("delete from character_query where characterID=@characterID", new { characterID = model.Id }, transaction);
                connection.Execute("insert into character_query (characterID, queryID) values (@characterID, @queryID)", cq, transaction);
                transaction.Commit();
                connection.Close();
                return Json(new { status = 0 });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                connection.Close();
                return Json(new { status = 1, errorDescription=ex.Message });
            }

        }
        [HttpPost]
        public IActionResult DeleteCharacter(Guid id)
        {
            connection.Execute("delete from character where id=@id", new { id });
            return Json(new { status = 0 });
        }
        #endregion
        public IActionResult FunctionList(string ca, byte? platformID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(" select [function].id, [function].name, [function].platformID, [function].controller, [function].action, platform.* from [function] left join platform on platform.id = [function].platformID where 1=1 ");
            if (platformID != null)
            {
                strSql.Append(" and platformID=@platformID ");
                ViewBag.PlatformID = platformID.Value;
            }
            if (!string.IsNullOrEmpty(ca))
            {
                strSql.Append(" and (controller=@ca or action=@ca or action like '%'+@ca+'%') ");
                ViewBag.CA = ca;
            }
            strSql.Append(" order by platformID, controller, action ");
            var result = connection.Query<Function, Platform, Function>(strSql.ToString(), (f, p) => {
                f.Platform = p;
                return f;
            }, new { ca, platformID }, splitOn: "id");
            ViewData["PlatformList"] = connection.Query<Platform>("select id, name from platform order by id").AsList();
            return View(result.AsList());
        }
        public IActionResult Function(Guid? id)
        {
            Function model = new Function();
            if (id != null)
            {
                string strSql = @"select [function].id, [function].name, [function].platformID, [function].controller, [function].action from [function] where id=@id";
                model = connection.QueryFirst<Function>(strSql, new { id });
            }
            ViewBag.ID = id;
            ViewData["QueryList"] = connection.Query<Query>("select * from query where functionID=@functionID order by selector", new { functionID=id }).AsList();
            ViewData["PlatformList"] = connection.Query<Platform>("select id, name from platform order by id").AsList();
            return View(model);
        }
        [HttpPost]
        public IActionResult UpdateFunction(Function model)
        {
            bool add = false;
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                add = true;
            }
            if (add)
            {
                connection.Execute("insert into [function] (id, name, platformID, controller, action) values (@id, @name, @platformID, @controller, @action)", model);
            }
            else
            {
                connection.Execute("update [function] set name=@name, controller=@controller, action=@action where id=@id", model);
            }
            return Json(new { status = 0, id = model.Id });
        }
        [HttpPost]
        public IActionResult DeleteFunction(Guid id)
        {
            connection.Execute("delete from [function] where id=@id", new { id });
            return Json(new { status = 0 });
        }
        [HttpPost]
        public IActionResult UpdateQuery(Query model)
        {
            bool add = false;
            if (model.Id == Guid.Empty)
            {
                model.Id = Guid.NewGuid();
                add = true;
            }
            if (add)
            {
                connection.Execute("insert into Query (id, functionID, name, selector) values (@id, @functionID, @name, @selector)", model);
            }
            else
            {
                connection.Execute("update Query set name=@name, selector=@selector where id=@id", model);
            }
            return Json(new { status = 0, id = model.Id });
        }
        [HttpPost]
        public IActionResult DeleteQuery(Guid id)
        {
            connection.Execute("delete from Query where id=@id", new { id });
            return Json(new { status = 0 });
        }
        #region Memcached缓存管理
        public IActionResult Memcached()
        {
            return View();
        }
        [HttpPost]
        public IActionResult GetMemcached(string key)
        {
            return Json(new
            {
                status = 0,
                value = Authorization.Lib.MemcachedHelper.Get(key)?.ToString()
            });
        }
        [HttpPost]
        public IActionResult RemoveMemcached(string key)
        {
            Authorization.Lib.MemcachedHelper.RemoveKeys(key);
            return Json(new
            {
                status = 0,
                value = Authorization.Lib.MemcachedHelper.Get(key)?.ToString()
            });
        }
        #endregion
    }
}