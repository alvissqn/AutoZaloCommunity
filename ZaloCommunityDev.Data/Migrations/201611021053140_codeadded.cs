namespace ZaloCommunityDev.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class codeadded : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Account",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Username = c.String(unicode: false),
                    Password = c.String(unicode: false),
                    IsActive = c.Boolean(nullable: false),
                    Region = c.String(unicode: false),
                    Order = c.Int(nullable: false),
                })
                .PrimaryKey(t => t.Id);

            AddColumn("dbo.AddFriendByPhoneConfig", "IgnoreRecentActionBefore", c => c.String(unicode: false));
            AddColumn("dbo.AddFriendNearByConfig", "IgnoreRecentActionBefore", c => c.String(unicode: false));
            AddColumn("dbo.MessageToFriendConfig", "IgnoreRecentActionBefore", c => c.String(unicode: false));
            AddColumn("dbo.MessageToStrangerByPhoneConfig", "IgnoreRecentActionBefore", c => c.String(unicode: false));
            AddColumn("dbo.MessageToStrangerNearByConfig", "IgnoreRecentActionBefore", c => c.String(unicode: false));
        }

        public override void Down()
        {
            DropColumn("dbo.MessageToStrangerNearByConfig", "IgnoreRecentActionBefore");
            DropColumn("dbo.MessageToStrangerByPhoneConfig", "IgnoreRecentActionBefore");
            DropColumn("dbo.MessageToFriendConfig", "IgnoreRecentActionBefore");
            DropColumn("dbo.AddFriendNearByConfig", "IgnoreRecentActionBefore");
            DropColumn("dbo.AddFriendByPhoneConfig", "IgnoreRecentActionBefore");
            DropTable("dbo.Account");
        }
    }
}