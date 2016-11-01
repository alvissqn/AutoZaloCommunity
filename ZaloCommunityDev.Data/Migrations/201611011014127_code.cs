namespace ZaloCommunityDev.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class code : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AddFriendByPhoneConfig",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountName = c.String(unicode: false),
                        ConfigName = c.String(unicode: false),
                        MaleGreeting = c.String(unicode: false),
                        FemaleGreeting = c.String(unicode: false),
                        SentImageForMale = c.String(unicode: false),
                        SentImageForFemale = c.String(unicode: false),
                        FilterAgeRange = c.String(unicode: false),
                        IncludedPeopleNames = c.String(unicode: false),
                        ExcludePeopleNames = c.String(unicode: false),
                        IncludePhoneNumbers = c.String(unicode: false),
                        ExcludePhoneNumbers = c.String(unicode: false),
                        NumberOfAction = c.Int(nullable: false),
                        GenderSelectionText = c.String(unicode: false),
                        Locations = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AddFriendNearByConfig",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountName = c.String(unicode: false),
                        ConfigName = c.String(unicode: false),
                        MaleGreeting = c.String(unicode: false),
                        FemaleGreeting = c.String(unicode: false),
                        SentImageForMale = c.String(unicode: false),
                        SentImageForFemale = c.String(unicode: false),
                        FilterAgeRange = c.String(unicode: false),
                        IncludedPeopleNames = c.String(unicode: false),
                        ExcludePeopleNames = c.String(unicode: false),
                        IncludePhoneNumbers = c.String(unicode: false),
                        ExcludePhoneNumbers = c.String(unicode: false),
                        NumberOfAction = c.Int(nullable: false),
                        GenderSelectionText = c.String(unicode: false),
                        Locations = c.String(unicode: false),
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
            
            CreateTable(
                "dbo.LogMessageSentToFriend",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MessageText = c.String(unicode: false),
                        Name = c.String(unicode: false),
                        BirthdayText = c.String(unicode: false),
                        GenderText = c.String(unicode: false),
                        CreatedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LogMessageSentToStranger",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MessageText = c.String(unicode: false),
                        Name = c.String(unicode: false),
                        BirthdayText = c.String(unicode: false),
                        GenderText = c.String(unicode: false),
                        CreatedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.LogRequestAddFriend",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        MessageText = c.String(unicode: false),
                        Name = c.String(unicode: false),
                        BirthdayText = c.String(unicode: false),
                        GenderText = c.String(unicode: false),
                        CreatedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MessageToFriendConfig",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountName = c.String(unicode: false),
                        ConfigName = c.String(unicode: false),
                        MaleGreeting = c.String(unicode: false),
                        FemaleGreeting = c.String(unicode: false),
                        SentImageForMale = c.String(unicode: false),
                        SentImageForFemale = c.String(unicode: false),
                        FilterAgeRange = c.String(unicode: false),
                        IncludedPeopleNames = c.String(unicode: false),
                        ExcludePeopleNames = c.String(unicode: false),
                        IncludePhoneNumbers = c.String(unicode: false),
                        ExcludePhoneNumbers = c.String(unicode: false),
                        NumberOfAction = c.Int(nullable: false),
                        GenderSelectionText = c.String(unicode: false),
                        Locations = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MessageToStrangerByPhoneConfig",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountName = c.String(unicode: false),
                        ConfigName = c.String(unicode: false),
                        MaleGreeting = c.String(unicode: false),
                        FemaleGreeting = c.String(unicode: false),
                        SentImageForMale = c.String(unicode: false),
                        SentImageForFemale = c.String(unicode: false),
                        FilterAgeRange = c.String(unicode: false),
                        IncludedPeopleNames = c.String(unicode: false),
                        ExcludePeopleNames = c.String(unicode: false),
                        IncludePhoneNumbers = c.String(unicode: false),
                        ExcludePhoneNumbers = c.String(unicode: false),
                        NumberOfAction = c.Int(nullable: false),
                        GenderSelectionText = c.String(unicode: false),
                        Locations = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MessageToStrangerNearByConfig",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AccountName = c.String(unicode: false),
                        ConfigName = c.String(unicode: false),
                        MaleGreeting = c.String(unicode: false),
                        FemaleGreeting = c.String(unicode: false),
                        SentImageForMale = c.String(unicode: false),
                        SentImageForFemale = c.String(unicode: false),
                        FilterAgeRange = c.String(unicode: false),
                        IncludedPeopleNames = c.String(unicode: false),
                        ExcludePeopleNames = c.String(unicode: false),
                        IncludePhoneNumbers = c.String(unicode: false),
                        ExcludePhoneNumbers = c.String(unicode: false),
                        NumberOfAction = c.Int(nullable: false),
                        GenderSelectionText = c.String(unicode: false),
                        Locations = c.String(unicode: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Profile",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(unicode: false),
                        BirthdayText = c.String(unicode: false),
                        GenderText = c.String(unicode: false),
                        CreatedTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Profile");
            DropTable("dbo.MessageToStrangerNearByConfig");
            DropTable("dbo.MessageToStrangerByPhoneConfig");
            DropTable("dbo.MessageToFriendConfig");
            DropTable("dbo.LogRequestAddFriend");
            DropTable("dbo.LogMessageSentToStranger");
            DropTable("dbo.LogMessageSentToFriend");
            DropTable("dbo.LogActivity");
            DropTable("dbo.AddFriendNearByConfig");
            DropTable("dbo.AddFriendByPhoneConfig");
        }
    }
}
