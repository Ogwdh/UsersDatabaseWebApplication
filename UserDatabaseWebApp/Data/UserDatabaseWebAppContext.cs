using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace UserDatabaseWebApp.Data
{
    public class UserDatabaseWebAppContext : DbContext
    {
        public UserDatabaseWebAppContext() : base("name=UserDatabaseWebAppContext")
        {
        }

        public System.Data.Entity.DbSet<UserDatabaseWebApp.Models.User> Users { get; set; }
    }
}