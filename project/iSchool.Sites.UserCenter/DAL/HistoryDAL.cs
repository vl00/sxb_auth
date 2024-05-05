using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace iSchool.Sites.UserCenter.DAL
{
    public class HistoryDAL
    {
        protected IDbConnection connection = new Library.DataAccess().iSchoolUserConnection();
        public bool AddHistory(Guid userID, Guid dataID, byte dataType)
        {
            return connection.Execute(@"merge into history
using (select 1 as o) t
on history.dataID=@dataID and history.userID=@userID
when not matched then insert 
(dataID, dataType, userID) values (@dataID, @dataType, @userID)
when matched then update
set time=sysdatetime();", new { dataID, dataType, userID }) > 0;
        }
        public bool RemoveHistory(Guid userID, Guid dataID)
        {
            return connection.Execute(@"delete from history where dataID=@dataID and userID=@userID"
, new { dataID, userID }) > 0;
        }
        public List<Guid> GetUserHistory(Guid userID, byte dataType, int page=1, int pageSize=10)
        {
            return connection.Query<Guid>(@"select dataID from history where 
userID=@userID and dataType=@dataType
order by time desc
offset (@page-1)*@pageSize rows fetch next @pageSize row only", new { userID, dataType, page, pageSize }).AsList();
        }
    }
}
