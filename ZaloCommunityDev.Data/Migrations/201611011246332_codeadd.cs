namespace ZaloCommunityDev.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class codeadd : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LogMessageSentToFriend", "PhoneNumber", c => c.String(unicode: false));
            AddColumn("dbo.LogMessageSentToStranger", "PhoneNumber", c => c.String(unicode: false));
            AddColumn("dbo.LogRequestAddFriend", "PhoneNumber", c => c.String(unicode: false));
            AddColumn("dbo.Profile", "PhoneNumber", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Profile", "PhoneNumber");
            DropColumn("dbo.LogRequestAddFriend", "PhoneNumber");
            DropColumn("dbo.LogMessageSentToStranger", "PhoneNumber");
            DropColumn("dbo.LogMessageSentToFriend", "PhoneNumber");
        }
    }
}
