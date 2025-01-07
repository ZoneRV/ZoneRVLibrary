using Serilog;
using TrelloDotNet.Model;
using TrelloDotNet.Model.Actions;
using ZoneRV.Models;
using ZoneRV.Models.DB;
using ZoneRV.Models.Enums;
using ZoneRV.Models.Location;
using ZoneRV.Models.Van;
using Attachment = TrelloDotNet.Model.Attachment;
using Checklist = TrelloDotNet.Model.Checklist;

namespace ZoneRV.Services.TrelloProduction;

public static class TrelloUtils
{
    internal static CachedTrelloAction ToCachedAction(this TrelloAction action)
    {
        return new CachedTrelloAction()
        {
            ActionId = action.Id,
            BoardId = action.Data.Board.Id,
            CardId = action.Data.Card.Id,
            DateOffset = action.Date,
            ActionType = action.Type,
            MemberId = action.MemberCreatorId,
            Content = action.Data.Text,
            CheckId = action.Data.CheckItem?.Id,
            DueDate = action.Data.Card.Due
        };
    }

    internal static JobCardInfo ToJobCardInfo(this TrelloDotNet.Model.Card trelloCard, IEnumerable<CachedTrelloAction> cardActions, IEnumerable<CustomField> customFields)
    {
        var cachedTrelloActions = cardActions.ToList();
        var enumerable = customFields.ToList();

        return new JobCardInfo()
        {
            Id = trelloCard.Id,
            Name = trelloCard.Name,
            Url = trelloCard.Url,
            TaskTime = GetTaskTime(trelloCard, enumerable),
            CardStatus = ToCardStatus(trelloCard, enumerable, cachedTrelloActions, out var lastUpdated),
            CardStatusLastUpdated = lastUpdated,
            ChecklistInfos = trelloCard.Checklists.Select(x => x.ToChecklistInfo(cachedTrelloActions)),
            CommentInfos = cachedTrelloActions
                .Where(x => x.ActionType == "commentCard")
                .Select(x => x.ToCommentInfo()).ToList(),
            AttachmentInfos = trelloCard.Attachments
                .Where(x => x.MimeType.StartsWith("image/"))
                .Select(x => x.ToAttachmentInfo()),
        };
    }

    internal static RedCardInfo ToRedCardInfo(this TrelloDotNet.Model.Card trelloCard, IEnumerable<CachedTrelloAction> cardActions, IEnumerable<CustomField> customFields)
    {
        var cachedTrelloActions = cardActions.ToList();
        var enumerable = customFields.ToList();

        return new RedCardInfo()
        {
            Id = trelloCard.Id,
            Name = trelloCard.Name,
            Url = trelloCard.Url,
            CreationDate = trelloCard.Created.HasValue ? trelloCard.Created.Value : null,
            RedFlagIssue = ToRedFlagIssue(trelloCard, enumerable),
            CardStatus = ToCardStatus(trelloCard, enumerable, cachedTrelloActions, out var lastUpdated),
            CardStatusLastUpdated = lastUpdated,
            ChecklistInfos = trelloCard.Checklists.Select(x => x.ToChecklistInfo(cachedTrelloActions)),
            CommentInfos = cachedTrelloActions
                .Where(x => x.ActionType == "commentCard")
                .Select(x => x.ToCommentInfo()).ToList(),
            AttachmentInfos = trelloCard.Attachments
                .Where(x => x.MimeType.StartsWith("image/"))
                .Select(x => x.ToAttachmentInfo()),
        };
    }

    internal static YellowCardInfo ToYellowCardInfo(this TrelloDotNet.Model.Card trelloCard, IEnumerable<CachedTrelloAction> cardActions, IEnumerable<CustomField> customFields)
    {
        var cachedTrelloActions = cardActions.ToList();
        var enumerable = customFields.ToList();

        return new YellowCardInfo()
        {
            Id = trelloCard.Id,
            Name = trelloCard.Name,
            Url = trelloCard.Url,
            CreationDate = trelloCard.Created.HasValue ? trelloCard.Created.Value : null,
            CardStatus = ToCardStatus(trelloCard, enumerable, cachedTrelloActions, out var lastUpdated),
            CardStatusLastUpdated = lastUpdated,
            ChecklistInfos = trelloCard.Checklists.Select(x => x.ToChecklistInfo(cachedTrelloActions)),
            CommentInfos = cachedTrelloActions
                .Where(x => x.ActionType == "commentCard")
                .Select(x => x.ToCommentInfo()).ToList(),
            AttachmentInfos = trelloCard.Attachments
                .Where(x => x.MimeType.StartsWith("image/"))
                .Select(x => x.ToAttachmentInfo()),
        };
    }

    internal static ChecklistInfo ToChecklistInfo(this Checklist trelloChecklist, IEnumerable<CachedTrelloAction> cardActions)
    {
        var cachedTrelloActions = cardActions.ToList();
        Dictionary<string, DateTimeOffset?> checkUpdates = [];

        foreach (var checkId in cachedTrelloActions.Select(x => x.CheckId).Where(x => !string.IsNullOrEmpty(x)).Distinct())
        {
            checkUpdates.Add(checkId!, cachedTrelloActions.Where(x => x.CheckId == checkId).Max(x => x.DateOffset));
        }
            
        return new ChecklistInfo()
        {
            Name = trelloChecklist.Name,
            Id = trelloChecklist.Id,
            CheckInfos = trelloChecklist.Items
                .Select(x => 
                    x.ToCheckInfo(checkUpdates.TryGetValue(x.Id, out var update) ? update : null))
        };
    }

    internal static CheckInfo ToCheckInfo(this ChecklistItem trelloCheck, DateTimeOffset? lastUpdated)
    {
        return new CheckInfo()
        {
            Name = trelloCheck.Name,
            Id = trelloCheck.Id,
            IsChecked = trelloCheck.State == ChecklistItemState.Complete,
            LasUpdated = lastUpdated
        };
    }

    internal static CommentInfo ToCommentInfo(this CachedTrelloAction action)
    {
        return new CommentInfo()
        {
            AuthorId = action.MemberId,
            Content = action.Content ?? "",
            DateCreated = action.DateOffset,
            Id = action.ActionId
        };
    }

    internal static AttachmentInfo ToAttachmentInfo(this Attachment attachment)
    {
        return new AttachmentInfo()
        {
            Id = attachment.Id,
            Url = attachment.Url,
            FileName = attachment.FileName
        };
    }

    internal static IEnumerable<CachedTrelloAction> ToCachedTrelloActions(this IEnumerable<TrelloAction> actions)
        => actions.Select(x => x.ToCachedAction());
    
    public static readonly string[] RedCardListNames =
        ["RED CARDS TO BE ACTIONED", "RED FLAG CARDS COMPLETED", "RED FLAG CARDS COMPLETEDI\u2076", "Design Issues"];

    public static readonly string[] YellowCardListNames =
        ["Yellow Cards___________ (Due to out of stock parts!)"];

    public static readonly string[] IgnoredRedAndYellowCardNames =
    [
        "IMPORTANT STEP's ON - How to raise a red card",
        "STEPS TO CLOSE OUT A RED CARD WHEN COMPLETED",
        "DAMAGE & WORKMANSHIP >>>>>ONLY<<<<<",
        "COMPLETED YELLOW & RED-CARDs",
        "HOW TO Create a Yellow Card",
        "Part Number/Description.",
        ">>RED CARDS COMPLETED<<"
    ];

    public static readonly string[] IgnoredJobListsNames =
    [
        "PLANS AND SPECS",
        "COMPLIANCE CERTIFICATES",
        "COMPLIANCE    & CERTIFICATES",
        "FINAL SIGN-OFFs___G2",
        "SIGN-OFFs",
        "PICKING PICTURES",
        "STORES - LATE PARTS COMMS",
        "VAN PROGRESS PHOTOS",
        "HANDOVER Day",
        "QC CHECKS",
        "HangarO",
        "DETAILING",
        "Yellow Cards___________ (Due to out of stock parts!)",
        "WELDING",
        "Van Board Creation _____(remove list once done)",
        "PLANS AND SPECS",
        "SIGN-OFFs"
    ];
    
    public static CardType GetCardType(string cardName, string listName)
    {
        if (RedCardListNames.Contains(listName) && !IgnoredRedAndYellowCardNames.Contains(cardName))
            return CardType.RedCard;

        if (YellowCardListNames.Contains(listName) && !IgnoredRedAndYellowCardNames.Contains(cardName))
            return CardType.YellowCard;

        if (!IgnoredJobListsNames.Contains(cardName) && !RedCardListNames.Contains(listName) &&
            !YellowCardListNames.Contains(listName))
            return CardType.JobCard;

        return CardType.None;
    }
        
    internal static TimeSpan GetTaskTime(TrelloDotNet.Model.Card card, IEnumerable<CustomField> customFields)
    {
        var desiredField = customFields.Single(x => x.Name == "TASK TIME (mins)");

        CustomFieldItem desiredFieldItem;

        if (card.CustomFieldItems.Any(x => x.CustomFieldId == desiredField.Id))
            desiredFieldItem = card.CustomFieldItems.Single(x => x.CustomFieldId == desiredField.Id);
        else
            return TimeSpan.Zero;

        string fieldValue = desiredFieldItem.Value.NumberAsString;

        return TimeSpan.FromMinutes(double.Parse(fieldValue));
    }

    public static CardStatus ToCardStatus(TrelloDotNet.Model.Card card, IEnumerable<CustomField> customFields,
        IEnumerable<CachedTrelloAction> customFieldActions, out DateTimeOffset? dateLastUpdated)
    {
        dateLastUpdated = null;

        var desiredField = customFields.Single(x => x.Name == "STATUS");

        CustomFieldItem desiredFieldItem;

        if (card.CustomFieldItems.Any(x => x.CustomFieldId == desiredField.Id))
            desiredFieldItem = card.CustomFieldItems.Single(x => x.CustomFieldId == desiredField.Id);
        else
            return CardStatus.NotStarted;

        string fieldValue = desiredField.Options.Single(x => x.Id == desiredFieldItem.ValueId).Value.Text;
        customFieldActions = customFieldActions.Where(x => new string(x.ActionId.SkipLast(3).ToArray()) == new string (desiredFieldItem.Id.SkipLast(3).ToArray()));

        if(customFieldActions.Count() > 0)
            dateLastUpdated = customFieldActions.OrderBy(x => x.DateOffset).Last().DateOffset;

        switch (fieldValue)
        {
            case "COMPLETED":
                return CardStatus.Completed;

            case "IN PROGRESS":
                return CardStatus.InProgress;

            case "APPROVED DBL RED LINE":
                return CardStatus.Completed; // TODO Confirm

            case "UNABLE TO COMPLETE":
                return CardStatus.UnableToComplete;

            default:
                Log.Logger.Error("Unknown card status {statusName}", fieldValue);
                return CardStatus.Unknown;
        }
    }
    
    internal static RedFlagIssue ToRedFlagIssue(TrelloDotNet.Model.Card card, IEnumerable<CustomField> customFields) //TODO: Handle multiple custom fields
    {
        var desiredField = customFields.Single(x => x.Name == "Red Flag Issue");

        CustomFieldItem desiredFieldItem;

        if (card.CustomFieldItems.Any(x => x.CustomFieldId == desiredField.Id))
            desiredFieldItem = card.CustomFieldItems.Single(x => x.CustomFieldId == desiredField.Id);
        else
            return RedFlagIssue.Unspecified;

        string fieldValue = desiredField.Options.Single(x => x.Id == desiredFieldItem.ValueId).Value.Text;

        switch (fieldValue)
        {
            case "Workmanship":
                return RedFlagIssue.WorkmanShip;

            case "Non Completed Task":
                return RedFlagIssue.NonCompletedTask;

            case "Damage":
                return RedFlagIssue.Damage;

            case "Out of Stock":
                return RedFlagIssue.OutOfStock;

            case "Faulty Component":
                return RedFlagIssue.FaultyComponent;

            case "Build Process":
                return RedFlagIssue.BuildProcess;

            case "Design Issue":
                return RedFlagIssue.DesignIssue;

            case "Missing Part":
                return RedFlagIssue.MissingPart;

            case "Shortage":
                return RedFlagIssue.Shortage;

            case "BOM":
                return RedFlagIssue.BOM;

            case null:
                return RedFlagIssue.Unspecified;

            default:
                Log.Logger.Error("Red flag issue Not Defined {area}", fieldValue);
                return RedFlagIssue.Unspecified;
        }
    }
    
    internal static AreaOfOrigin ToAreaOfOrigin(TrelloDotNet.Model.Card card, IEnumerable<CustomField> customFields)
    {
        throw new NotImplementedException();
    }
}