﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MoneyTransfer.Database
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MoneyGatewaydbEntities11 : DbContext
    {
        public MoneyGatewaydbEntities11()
            : base("name=MoneyGatewaydbEntities11")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Admin_NotificationTbl> Admin_NotificationTbl { get; set; }
        public virtual DbSet<BalanceTbl> BalanceTbls { get; set; }
        public virtual DbSet<signup_table> signup_table { get; set; }
        public virtual DbSet<tbl_Transaction> tbl_Transaction { get; set; }
        public virtual DbSet<User_Notifications> User_Notifications { get; set; }
        public virtual DbSet<address_details> address_details { get; set; }
        public virtual DbSet<cartItem> cartItems { get; set; }
        public virtual DbSet<tbl_stripeDashboard> tbl_stripeDashboard { get; set; }
        public virtual DbSet<ForgotPassword> ForgotPasswords { get; set; }
        public virtual DbSet<resetExtra> resetExtras { get; set; }
    }
}
