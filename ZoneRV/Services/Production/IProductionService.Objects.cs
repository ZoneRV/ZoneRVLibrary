using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Mysqlx.Crud;
using ZoneRV.DBContexts;

namespace ZoneRV.Services.Production;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract partial class IProductionService
{
    /// <summary>
    /// Key is the Sales Order name
    /// </summary>
    protected ConcurrentDictionary<string, SalesOrder> SalesOrders { get; } = [];
    
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

    public async Task<WorkspaceLocation> CreateWorkspaceLocation(ProductionWorkspace workspace, string name, string? description, ProductionLocationType type)
    {
        if (workspace.Lines.Any(x => x.Name.ToLower() == name.ToLower()))
            throw new DuplicateNameException($"Cannot create workspace location with name {name} in workspace {workspace.Name}, already exists.");
        
        using (var scope = ScopeFactory.CreateScope())
        {
            var productionContext = scope.ServiceProvider.GetRequiredService<ProductionContext>();

            var newLocation = LocationFactory.CreateWorkspaceLocation(workspace, name, description, type);

            productionContext.Workspaces.Update(workspace);

            await productionContext.SaveChangesAsync();

            return newLocation;
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
                Workspace = workspace, Name = name, Description = description, OrderedLineLocations = []
            };
            
            workspace.Lines.Add(line);

            productionContext.Workspaces.Update(workspace);
            
            await productionContext.SaveChangesAsync();
            
            // TODO load line on creation
            
            return line;
        }
    }

    public async Task<OrderedLineLocation> CreateOrderedLocation(ProductionLine line, WorkspaceLocation location, decimal order, string? customName, string? inventoryName)
    {
        using (var scope = ScopeFactory.CreateScope())
        {
            var productionContext = scope.ServiceProvider.GetRequiredService<ProductionContext>();

            var newLocation = LocationFactory.CreateOrderedLineLocation(line, location, order, customName, inventoryName);
            
            location.OrderedLineLocations.Add(newLocation);
            line.OrderedLineLocations.Add(newLocation);

            productionContext.WorkSpaceLocations.Update(location);
            
            await productionContext.SaveChangesAsync();
            
            MarkSalesOrdersUnloaded(x => x.Model.Line.Id == line.Id);
            
            return newLocation;
        }
    }

    public async Task<AreaOfOrigin> CreateAreaOfOrigin(ProductionLine line, string name)
    {
        if (line.AreaOfOrigins.Any(x => x.Name.ToLower() == name.ToLower()))
            throw new DuplicateNameException($"Cannot create Area of Origin with name {name}, Already exists.");
        
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
            
            MarkSalesOrdersUnloaded(x => x.Model.Line.Id == line.Id);
            
            return area;
        }
    }

    protected JobCard BuildJobCard(SalesOrder salesOrder, JobCardCreationInfo info, AreaOfOrigin? areaOfOrigin, OrderedLineLocation location)
    {
        var jobcard = new JobCard(salesOrder, info, areaOfOrigin, location);

        foreach (var commentInfo in info.CommentInfos)
        {
            BuildComment(commentInfo, jobcard);
        }

        foreach (var checklistInfo in info.ChecklistInfos)
        {
            BuildChecklist(checklistInfo, jobcard);
        }

        foreach (var attachment in info.AttachmentInfos)
        {
            BuildAttachment(attachment, jobcard);
        }

        JobCards.TryAdd(info.Id, jobcard);
        salesOrder.JobCards.Add(jobcard);

        return jobcard;
    }

     protected RedCard BuildRedCard(SalesOrder salesOrder, RedCardCreationInfo info, AreaOfOrigin? areaOfOrigin)
     {
         var redCard = new RedCard(salesOrder, info, areaOfOrigin);

        foreach (var commentInfo in info.CommentInfos)
        {
            BuildComment(commentInfo, redCard);
        }

        foreach (var checklistInfo in info.ChecklistInfos)
        {
            BuildChecklist(checklistInfo, redCard);
        }

        foreach (var attachmentInfo in info.AttachmentInfos)
        {
            BuildAttachment(attachmentInfo, redCard);
        }

        RedCards.TryAdd(info.Id, redCard);
        salesOrder.RedCards.Add(redCard);

        return redCard;
    }

     protected YellowCard BuildYellowCard(SalesOrder salesOrder, YellowCardCreationInfo info, AreaOfOrigin? areaOfOrigin)
     {
         var yellowCard = new YellowCard(salesOrder, info, areaOfOrigin);

        foreach (var commentInfo in info.CommentInfos)
        {
            BuildComment(commentInfo, yellowCard);
        }

        foreach (var checklistInfo in info.ChecklistInfos)
        {
            BuildChecklist(checklistInfo, yellowCard);
        }

        foreach (var attachmentInfo in info.AttachmentInfos)
        {
            BuildAttachment(attachmentInfo, yellowCard);
        }

        YellowCards.TryAdd(info.Id, yellowCard);
        salesOrder.YellowCards.Add(yellowCard);

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

    protected Comment BuildComment(CommentCreationInfo info, Card card)
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

    protected Attachment BuildAttachment(AttachmentCreationInfo info, Card card)
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