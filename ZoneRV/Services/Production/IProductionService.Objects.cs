using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZoneRV.DBContexts;

namespace ZoneRV.Services.Production;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract partial class IProductionService
{
    /// <summary>
    /// Key is the van name
    /// </summary>
    protected ConcurrentDictionary<string, SalesOrder> Vans { get; } = [];
    
    protected ConcurrentDictionary<string, Check> Checks { get; } = [];
    protected ConcurrentDictionary<string, Checklist> Checklists { get; } = [];
    protected ConcurrentDictionary<string, JobCard> JobCards { get; } = [];
    protected ConcurrentDictionary<string, RedCard> RedCards { get; } = [];
    protected ConcurrentDictionary<string, YellowCard> YellowCards { get; } = [];
    protected ConcurrentDictionary<string, Comment> Comments { get; } = [];
    protected ConcurrentDictionary<string, Attachment> Attachments { get; } = [];
    protected ConcurrentDictionary<string, User> Users { get; } = [];

    public async Task<ProductionWorkspace> CreateProductionWorkspace(string name, string? description)
    {
        if (Workspaces.Any(x => x.Name.ToLower() == name.ToLower()))
            throw new DuplicateNameException($"Workspace with name {name} already exists.");

        using (var scope = ScopeFactory.CreateScope())
        {
            var productionContext = scope.ServiceProvider.GetRequiredService<ProductionContext>();

            var newWorkspace =
                productionContext.Workspaces.Add(new ProductionWorkspace()
                {
                    Name = name, 
                    Description = description
                });

            await productionContext.SaveChangesAsync();
            
            Workspaces.Add(newWorkspace.Entity);

            return newWorkspace.Entity;
        }
    }

    public async Task<ProductionLine> CreateProductionLine(ProductionWorkspace workspace, string name, string? description)
    {
        if (ProductionLines.Any(x => x.Name.ToLower() == name.ToLower() && x.Workspace.Id == workspace.Id))
            throw new DuplicateNameException($"Cannot create Production line with name {name} in workspace {workspace.Name}, Already exists.");
        
        using (var scope = ScopeFactory.CreateScope())
        {
            var productionContext = scope.ServiceProvider.GetRequiredService<ProductionContext>();

            var line = new ProductionLine()
            {
                Workspace = workspace, Name = name, Description = description
            };
            
            workspace.Lines.Add(line);

            productionContext.Workspaces.Update(workspace);
            
            await productionContext.SaveChangesAsync();
            
            // TODO load line on creation
            
            return line;
        }
    }

    public async Task<AreaOfOrigin> CreateAreaOfOrigin(ProductionLine line, string name)
    {
        if (line.AreaOfOrigins.Any(x => x.Name.ToLower() == name.ToLower()))
            throw new DuplicateNameException("Cannot create Area of Origin with name {name}, Already exists.");
        
        using (var scope = ScopeFactory.CreateScope())
        {
            var productionContext = scope.ServiceProvider.GetRequiredService<ProductionContext>();

            var area =
                new AreaOfOrigin()
                {
                    Name = name, Line = line
                };
            
            line.AreaOfOrigins.Add(area);

            productionContext.Update(line);
            
            await productionContext.SaveChangesAsync();
            
            MarkSOsUnloaded(x => x.Model.LineId == line.Id);
            
            return area;
        }
    }

    protected JobCard BuildJobCard(SalesOrder van, JobCardCreationInfo info, AreaOfOrigin? areaOfOrigin, OrderedLineLocation location)
    {
        var jobcard = new JobCard(van, info, areaOfOrigin, location);

        foreach (var commentInfo in info.CommentInfos)
        {
            BuildComment(van, commentInfo, jobcard);
        }

        foreach (var checklistInfo in info.ChecklistInfos)
        {
            BuildChecklist(checklistInfo, jobcard);
        }

        foreach (var attachment in info.AttachmentInfos)
        {
            BuildAttachment(van, attachment, jobcard);
        }

        JobCards.TryAdd(info.Id, jobcard);
        van.JobCards.Add(jobcard);

        return jobcard;
    }

     protected RedCard BuildRedCard(SalesOrder van, RedCardCreationInfo info, AreaOfOrigin? areaOfOrigin)
     {
         var redCard = new RedCard(van, info, areaOfOrigin);

        foreach (var commentInfo in info.CommentInfos)
        {
            BuildComment(van, commentInfo, redCard);
        }

        foreach (var checklistInfo in info.ChecklistInfos)
        {
            BuildChecklist(checklistInfo, redCard);
        }

        foreach (var attachmentInfo in info.AttachmentInfos)
        {
            BuildAttachment(van, attachmentInfo, redCard);
        }

        RedCards.TryAdd(info.Id, redCard);
        van.RedCards.Add(redCard);

        return redCard;
    }

     protected YellowCard BuildYellowCard(SalesOrder van, YellowCardInfo info, AreaOfOrigin? areaOfOrigin)
     {
         var yellowCard = new YellowCard(van, info, areaOfOrigin);

        foreach (var commentInfo in info.CommentInfos)
        {
            BuildComment(van, commentInfo, yellowCard);
        }

        foreach (var checklistInfo in info.ChecklistInfos)
        {
            BuildChecklist(checklistInfo, yellowCard);
        }

        foreach (var attachmentInfo in info.AttachmentInfos)
        {
            BuildAttachment(van, attachmentInfo, yellowCard);
        }

        YellowCards.TryAdd(info.Id, yellowCard);
        van.YellowCards.Add(yellowCard);

        return yellowCard;
    }

    protected Checklist BuildChecklist(ChecklistCreationInfo info, Card card)
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
            BuildCheck(checkInfo, checklist);
        }
        
        Checklists.TryAdd(info.Id, checklist);
        card.Checklists.Add(checklist);

        return checklist;
    }
    
    protected Check BuildCheck(CheckCreationInfo info, Checklist checklist)
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

    protected Comment BuildComment(SalesOrder van, CommentInfo info, Card card)
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

    protected Attachment BuildAttachment(SalesOrder van, AttachmentInfo info, Card card)
    {
        var attachment = new Attachment()
        {
            Card = card,
            Id = info.Id,
            Url = info.Url
        };

        Attachments.TryAdd(info.Id, attachment);
        card.Attachments.Add(attachment);

        return attachment;
    }
}