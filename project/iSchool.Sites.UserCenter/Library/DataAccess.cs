using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace iSchool.Sites.UserCenter.Library
{
    public class ConnectionOptions
    {
        public string iSchool { get; set; }
        public string iSchoolUser { get; set; }
    }
    public class DataAccess
    {
        private static ConnectionOptions _connectionOptions;
        public DataAccess(ConnectionOptions connectionOptions)
        {
            _connectionOptions = connectionOptions;
        }
        public DataAccess() { }
        public IDbConnection iSchoolConnection()
        {
            return new SqlConnection(_connectionOptions.iSchool);
        }
        public IDbConnection iSchoolUserConnection()
        {
            return new SqlConnection(_connectionOptions.iSchoolUser);
        }
    }
}
