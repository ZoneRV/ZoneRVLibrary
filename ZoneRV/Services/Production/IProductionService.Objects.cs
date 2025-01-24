using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace ZoneRV.Services.Production;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract partial class IProductionService
{
    /// <summary>
    /// Key is the van name
    /// </summary>
    protected ConcurrentDictionary<string, SalesProductionInfo> Vans { get; } = [];
    
    protected ConcurrentDictionary<string, Check> Checks { get; } = [];
    protected ConcurrentDictionary<string, Checklist> Checklists { get; } = [];
    protected ConcurrentDictionary<string, JobCard> JobCards { get; } = [];
    protected ConcurrentDictionary<string, RedCard> RedCards { get; } = [];
    protected ConcurrentDictionary<string, YellowCard> YellowCards { get; } = [];
    protected ConcurrentDictionary<string, Comment> Comments { get; } = [];
    protected ConcurrentDictionary<string, Attachment> Attachments { get; } = [];
    protected ConcurrentDictionary<string, User> Users { get; } = [];

    protected JobCard CreateJobCard(SalesProductionInfo van, JobCardCreationInfo info, AreaOfOrigin areaOfOrigin, Location location)
    {
        var jobcard = new JobCard(van, info, areaOfOrigin, location);

        foreach (var commentInfo in info.CommentInfos)
        {
            CreateComment(van, commentInfo, jobcard);
        }

        foreach (var checklistInfo in info.ChecklistInfos)
        {
            CreateChecklist(checklistInfo, jobcard);
        }

        foreach (var attachment in info.AttachmentInfos)
        {
            CreateAttachment(van, attachment, jobcard);
        }

        JobCards.TryAdd(info.Id, jobcard);
        van.JobCards.Add(jobcard);

        return jobcard;
    }

     protected RedCard CreateRedCard(SalesProductionInfo van, RedCardCreationInfo info, AreaOfOrigin areaOfOrigin)
     {
         var redCard = new RedCard(van, info, areaOfOrigin);

        foreach (var commentInfo in info.CommentInfos)
        {
            CreateComment(van, commentInfo, redCard);
        }

        foreach (var checklistInfo in info.ChecklistInfos)
        {
            CreateChecklist(checklistInfo, redCard);
        }

        foreach (var attachmentInfo in info.AttachmentInfos)
        {
            CreateAttachment(van, attachmentInfo, redCard);
        }

        RedCards.TryAdd(info.Id, redCard);
        van.RedCards.Add(redCard);

        return redCard;
    }

     protected YellowCard CreateYellowCard(SalesProductionInfo van, YellowCardInfo info, AreaOfOrigin areaOfOrigin)
     {
         var yellowCard = new YellowCard(van, info, areaOfOrigin);

        foreach (var commentInfo in info.CommentInfos)
        {
            CreateComment(van, commentInfo, yellowCard);
        }

        foreach (var checklistInfo in info.ChecklistInfos)
        {
            CreateChecklist(checklistInfo, yellowCard);
        }

        foreach (var attachmentInfo in info.AttachmentInfos)
        {
            CreateAttachment(van, attachmentInfo, yellowCard);
        }

        YellowCards.TryAdd(info.Id, yellowCard);
        van.YellowCards.Add(yellowCard);

        return yellowCard;
    }

    protected Checklist CreateChecklist(ChecklistCreationInfo info, Card card)
    {
        var checklist = new Checklist()
        {
            Card = card,
            Id = info.Id,
            Name = info.Name,
            Checks = []
        };

        foreach (var checkInfo in info.CheckInfos)
        {
            CreateCheck(checkInfo, checklist);
        }
        
        Checklists.TryAdd(info.Id, checklist);
        card.Checklists.Add(checklist);

        return checklist;
    }
    
    protected Check CreateCheck(CheckCreationInfo info, Checklist checklist)
    {
        var check = new Check()
        {
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

    protected Comment CreateComment(SalesProductionInfo van, CommentInfo info, Card card)
    {
        Users.TryGetValue(info.AuthorId, out var user);

        var comment = new Comment()
        {
            Author = user,
            Card = card,
            Content = info.Content,
            DateCreated = info.DateCreated,
            Id = info.Id
        };

        Comments.TryAdd(info.Id, comment);
        card.Comments.Add(comment);

        return comment;
    }

    protected Attachment CreateAttachment(SalesProductionInfo van, AttachmentInfo info, Card card)
    {
        var attachment = new Attachment()
        {
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