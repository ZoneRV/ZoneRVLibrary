using System.Collections.Concurrent;

namespace ZoneRV.Services.Production;

public abstract partial class IProductionService
{
    /// <summary>
    /// Key is the van name
    /// </summary>
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
            CreateComment(van, commentInfo, jobcard);
        }

        foreach (var checklistInfo in info.ChecklistInfos)
        {
            CreateChecklist(van, checklistInfo, jobcard);
        }

        foreach (var attachmentInfo in info.AttachmentInfos)
        {
            CreateAttachment(van, attachmentInfo, jobcard);
        }

        JobCards.TryAdd(info.Id, jobcard);
        van.JobCards.Add(jobcard);

        return jobcard;
    }

     protected RedCard CreateRedCard(VanProductionInfo van, RedCardInfo info, AreaOfOrigin areaOfOrigin)
     {
         var redCard = new RedCard(van, info, areaOfOrigin);

        foreach (var commentInfo in info.CommentInfos)
        {
            CreateComment(van, commentInfo, redCard);
        }

        foreach (var checklistInfo in info.ChecklistInfos)
        {
            CreateChecklist(van, checklistInfo, redCard);
        }

        foreach (var attachmentInfo in info.AttachmentInfos)
        {
            CreateAttachment(van, attachmentInfo, redCard);
        }

        RedCards.TryAdd(info.Id, redCard);
        van.RedCards.Add(redCard);

        return redCard;
    }

     protected YellowCard CreateYellowCard(VanProductionInfo van, YellowCardInfo info, AreaOfOrigin areaOfOrigin)
     {
         var yellowCard = new YellowCard(van, info, areaOfOrigin);

        foreach (var commentInfo in info.CommentInfos)
        {
            CreateComment(van, commentInfo, yellowCard);
        }

        foreach (var checklistInfo in info.ChecklistInfos)
        {
            CreateChecklist(van, checklistInfo, yellowCard);
        }

        foreach (var attachmentInfo in info.AttachmentInfos)
        {
            CreateAttachment(van, attachmentInfo, yellowCard);
        }

        YellowCards.TryAdd(info.Id, yellowCard);
        van.YellowCards.Add(yellowCard);

        return yellowCard;
    }

    protected Checklist CreateChecklist(VanProductionInfo van, ChecklistInfo info, Card card)
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
            CreateCheck(van, checkInfo, checklist);
        }
        
        Checklists.TryAdd(info.Id, checklist);
        card.Checklists.Add(checklist);

        return checklist;
    }
    
    protected Check CreateCheck(VanProductionInfo van, CheckInfo info, Checklist checklist)
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

    protected Comment CreateComment(VanProductionInfo van, CommentInfo info, Card card)
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

    protected Attachment CreateAttachment(VanProductionInfo van, AttachmentInfo info, Card card)
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