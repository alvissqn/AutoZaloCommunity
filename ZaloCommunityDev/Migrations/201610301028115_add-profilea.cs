namespace ZaloCommunityDev.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addprofilea : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AddingFriendConfig",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        AgeRange = c.String(unicode: false),
                        MaleGreeting = c.String(unicode: false),
                        FemaleGreeting = c.String(unicode: false),
                        GenderSelectionText = c.String(unicode: false),
                        WishAddedNumberFriendPerDay = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AutoPostToFriendSessionConfig",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        TextToFemale = c.String(unicode: false),
                        TextToMale = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AutoPostToStrangerSessionConfig",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        TextToFemale = c.String(unicode: false),
                        TextToMale = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.FriendProfileInfo",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        BirthdayText = c.String(unicode: false),
                        GenderSelectionText = c.String(unicode: false),
                        Created = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LogActivity",
                c => new
                    {
                        Date = c.String(nullable: false, maxLength: 10, unicode: false),
                        AddedFriendCount = c.Int(nullable: false),
                        PostFriendCount = c.Int(nullable: false),
                        PostStrangerCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Date);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.LogActivity");
            DropTable("dbo.FriendProfileInfo");
            DropTable("dbo.AutoPostToStrangerSessionConfig");
            DropTable("dbo.AutoPostToFriendSessionConfig");
            DropTable("dbo.AddingFriendConfig");
        }
    }
}
