namespace ZaloCommunityDev.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addprofilea11 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AutoPostToFriendSessionConfig", "OnlySpamPeopleNames", c => c.String(unicode: false));
            AddColumn("dbo.AutoPostToFriendSessionConfig", "ExceptSpamPeopleNames", c => c.String(unicode: false));
            AddColumn("dbo.AutoPostToFriendSessionConfig", "NumberOfSpamMail", c => c.String(unicode: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AutoPostToFriendSessionConfig", "NumberOfSpamMail");
            DropColumn("dbo.AutoPostToFriendSessionConfig", "ExceptSpamPeopleNames");
            DropColumn("dbo.AutoPostToFriendSessionConfig", "OnlySpamPeopleNames");
        }
    }
}
