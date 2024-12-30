using System.Collections.Concurrent;

namespace ZoneRV.Services.Production;

public abstract partial class IProductionService
{
    protected ConcurrentDictionary<string, VanProductionInfo> Vans { get; init; } = [];
    
    protected ConcurrentDictionary<string, Check> Checks { get; init; } = [];
    protected ConcurrentDictionary<string, Checklist> Checklists { get; init; } = [];
    protected ConcurrentDictionary<string, JobCard> JobCards { get; init; } = [];
    protected ConcurrentDictionary<string, RedCard> RedCards { get; init; } = [];
    protected ConcurrentDictionary<string, YellowCard> YellowCards { get; init; } = [];
    protected ConcurrentDictionary<string, Comment> Comments { get; init; } = [];
    protected ConcurrentDictionary<string, Attachment> Attachments { get; init; } = [];
    protected ConcurrentDictionary<string, User> Users { get; init; } = [];

    protected JobCard CreateJobCard(VanProductionInfo van, JobCardInfo info, AreaOfOrigin areaOfOrigin, ProductionLocation location)
    {
        var jobcard = new JobCard(van, info, areaOfOrigin, location);

        foreach (var commentInfo in info.CommentInfos)
        {
            CreateComment(van, jobcard, commentInfo);
        }

        foreach (var checklistInfo in info.ChecklistInfos)
        {
            CreateChecklist(van, jobcard, checklistInfo);
        }

        foreach (var attachmentInfo in info.AttachmentInfos)
        {
            CreateAttachment(van, jobcard, attachmentInfo);
        }

        JobCards.TryAdd(info.Id, jobcard);
        van.JobCards.Add(jobcard);

        return jobcard;
    }

     protected RedCard CreateRedCard(VanProductionInfo van, AreaOfOrigin areaOfOrigin, RedCardInfo info)
     {
         var redCard = new RedCard(van, info, areaOfOrigin);

        foreach (var commentInfo in info.CommentInfos)
        {
            CreateComment(van, redCard, commentInfo);
        }

        foreach (var checklistInfo in info.ChecklistInfos)
        {
            CreateChecklist(van, redCard, checklistInfo);
        }

        foreach (var attachmentInfo in info.AttachmentInfos)
        {
            CreateAttachment(van, redCard, attachmentInfo);
        }

        RedCards.TryAdd(info.Id, redCard);
        van.RedCards.Add(redCard);

        return redCard;
    }

     protected YellowCard CreateYellowCard(VanProductionInfo van, AreaOfOrigin areaOfOrigin, YellowCardInfo info)
     {
         var yellowCard = new YellowCard(van, info, areaOfOrigin);

        foreach (var commentInfo in info.CommentInfos)
        {
            CreateComment(van, yellowCard, commentInfo);
        }

        foreach (var checklistInfo in info.ChecklistInfos)
        {
            CreateChecklist(van, yellowCard, checklistInfo);
        }

        foreach (var attachmentInfo in info.AttachmentInfos)
        {
            CreateAttachment(van, yellowCard, attachmentInfo);
        }

        YellowCards.TryAdd(info.Id, yellowCard);
        van.YellowCards.Add(yellowCard);

        return yellowCard;
    }

    protected Checklist CreateChecklist(VanProductionInfo van, Card card, ChecklistInfo info)
    {
        var checklist = new Checklist()
        {
            Van = van,
            Card = card,
            Id = info.Id,
            Name = info.Name,
            Checks = []
        };

        foreach (var checkInfo in info.CheckInfos)
        {
            CreateCheck(van, checklist, checkInfo);
        }
        
        Checklists.TryAdd(info.Id, checklist);
        card.Checklists.Add(checklist);

        return checklist;
    }
    
    protected Check CreateCheck(VanProductionInfo van, Checklist checklist, CheckInfo info)
    {
        var check = new Check()
        {
            Van = van,
            Checklist = checklist,
            Name = info.Name,
            Id = info.Id,
            IsChecked = info.IsChecked,
            LastModified = info.LasUpdated
        };

        Checks.TryAdd(info.Id, check);
        checklist.Checks.Add(check);

        return check;
    }

    protected Comment CreateComment(VanProductionInfo van, Card card, CommentInfo info)
    {
        Users.TryGetValue(info.AuthorId, out var user);

        var comment = new Comment()
        {
            Author = user,
            AuthorId = info.AuthorId,
            Card = card,
            Content = info.Content,
            DateCreated = info.DateCreated,
            Id = info.Id,
            Van = van
        };

        Comments.TryAdd(info.Id, comment);
        card.Comments.Add(comment);

        return comment;
    }

    protected Attachment CreateAttachment(VanProductionInfo van, Card card, AttachmentInfo info)
    {
        var attachment = new Attachment()
        {
            Van = van,
            Card = card,
            FileName = info.FileName,
            Id = info.Id,
            Url = info.Url
        };

        Attachments.TryAdd(info.Id, attachment);
        card.Attachments.Add(attachment);

        return attachment;
    }
}