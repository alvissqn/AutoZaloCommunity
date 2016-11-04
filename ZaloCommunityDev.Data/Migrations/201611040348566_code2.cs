namespace ZaloCommunityDev.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class code2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AddFriendByPhoneConfig", "FilterAgeRangeAcceptIfHidden", c => c.Boolean(nullable: false));
            AddColumn("dbo.AddFriendNearByConfig", "FilterAgeRangeAcceptIfHidden", c => c.Boolean(nullable: false));
            AddColumn("dbo.MessageToFriendConfig", "FilterAgeRangeAcceptIfHidden", c => c.Boolean(nullable: false));
            AddColumn("dbo.MessageToStrangerByPhoneConfig", "FilterAgeRangeAcceptIfHidden", c => c.Boolean(nullable: false));
            AddColumn("dbo.MessageToStrangerNearByConfig", "FilterAgeRangeAcceptIfHidden", c => c.Boolean(nullable: false));
            AddColumn("dbo.Profile", "IsFriend", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Profile", "IsFriend");
            DropColumn("dbo.MessageToStrangerNearByConfig", "FilterAgeRangeAcceptIfHidden");
            DropColumn("dbo.MessageToStrangerByPhoneConfig", "FilterAgeRangeAcceptIfHidden");
            DropColumn("dbo.MessageToFriendConfig", "FilterAgeRangeAcceptIfHidden");
            DropColumn("dbo.AddFriendNearByConfig", "FilterAgeRangeAcceptIfHidden");
            DropColumn("dbo.AddFriendByPhoneConfig", "FilterAgeRangeAcceptIfHidden");
        }
    }
}
