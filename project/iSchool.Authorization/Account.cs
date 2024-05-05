using Dapper;
using iSchool.Authorization.Lib;
using iSchool.Authorization.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace iSchool.Authorization
{
    public class Account : DataAccess
    {
        public AdminInfo Info(HttpContext context)
        {
            AdminInfo model = MemcachedHelper.Get<AdminInfo>("dbo.adminInfo-" + context.User.Identity.Name);
            //if (model == null)
            {
                string strSql = @"select admin_info.*, character.*, [function].id, [function].name, [function].platformID, [function].controller, [function].action, platform.* from admin_info
left join admin_character on admin_info.id=admin_character.adminID
left join character on character.id = admin_character.characterID
left join character_function on character_function.characterid=character.id
left join [function] on [function].id = character_function.functionID
left join platform on platform.id = [function].platformid
where admin_info.id=@id
";
                var lookup_a = new Dictionary<Guid, AdminInfo>();
                var lookup_c = new Dictionary<Guid, Character>();
                var result = connection.Query<AdminInfo, Character, Function, Platform, AdminInfo>(strSql, (a,c,f,p)=> {
                    AdminInfo tmp_a;
                    if (!lookup_a.TryGetValue(a.Id, out tmp_a))
                    {
                        tmp_a = a;
                        lookup_a.Add(a.Id, tmp_a);
                    }
                    if (c != null)
                    {
                        Character tmp_c;
                        if (!lookup_c.TryGetValue(c.Id, out tmp_c))
                        {
                            tmp_c = c;
                            lookup_c.Add(c.Id, tmp_c);
                            tmp_a.Character.Add(tmp_c);
                        }
                        if (f != null)
                        {
                            f.Platform = p;
                            tmp_c.Function.Add(f);
                        }
                    }
                    return a;
                }, new { id = context.User.Identity.Name }, splitOn:"id" );
                if (lookup_a.Count > 0)
                {
                    model = lookup_a.Values.AsList()[0];
                    MemcachedHelper.Set("dbo.adminInfo-" + model.Id, model);
                }
            }
            return model;
        }
        public List<AdminInfo> GetAdmins(List<Guid> ids)
        {
            List<Guid> idsCopy = ids.Distinct().AsList();
            List<AdminInfo> result = connection.Query<AdminInfo>("select id, name, displayName from admin_info where id in @ids", new { ids = idsCopy }).AsList();
            List<AdminInfo> OutputResult = new List<AdminInfo>();
            foreach(Guid id in ids)
            {
                var r = result.Find(a => a.Id == id);
                if (r == null)
                {
                    OutputResult.Add(new AdminInfo() { Id = id });
                }
                else
                {
                    OutputResult.Add(r);
                }
            }
            return OutputResult;
        }
        public List<AdminInfo> GetAdmins(Guid id, IDType type)
        {
            string sql = string.Empty;
            if (type == IDType.CharacterID)
            {
                sql = @"select admin_info.id, admin_info.name, admin_info.displayname from admin_info 
left join admin_character on admin_info.id=adminID 
left join character on admin_character.characterID=character.id where 
admin_character.characterID=@id order by regTime";
            }
            else if(type == IDType.FunctionID)
            {
                sql = @"select admin_info.id, admin_info.name, admin_info.displayname from admin_info
left join admin_character on admin_info.id=admin_character.adminID
left join character_function on admin_character.characterID=character_function.characterID
where functionID=@id
group by admin_info.id, admin_info.name, admin_info.displayname, regTime
order by regTime";
            }
            return connection.Query<AdminInfo>(sql, new { id }).AsList();
        }
        public enum IDType
        {
            CharacterID,
            FunctionID
        }
    }
}
