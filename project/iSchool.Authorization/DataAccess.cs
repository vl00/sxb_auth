using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace iSchool.Authorization
{
    public class DataAccess
    {
        protected const string connectionString = "Data Source=10.1.0.16;Initial Catalog=iSchoolConsole;User ID=iSchool;password=SxbLucas$0769;";
        protected IDbConnection connection = new SqlConnection(connectionString);
    }
}
