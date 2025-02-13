using System.Diagnostics.CodeAnalysis;

namespace ZoneRV.Services.Production;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract partial class IProductionService
{

#region Check Updates
    public void UpdateCheck(CheckUpdatedData data)
    {
        switch (data.UpdateType)
        {
            case EntityUpdateType.Add:
            {
                if (Checks.TryGetValue(data.Id, out var check))
                {
                    UpdateCheckWithTrigger(check, data);
                }
                else
                {
                    if (Checklists.TryGetValue(data.ChecklistId, out var checklist))
                    {
                        if (data.Name is null || data.IsChecked is null)
                        {
                            Exception ex = new ArgumentNullException(nameof(data), "Check updates need a 'name' and 'isChecked' value to create new Check.");
                            Log.Logger.Error(ex, "Failed creating new check {checkId}", data.Id);
                            break;
                        }
                        
                        CreateCheckWithTrigger(new CheckCreationInfo()
                                   {
                                       Id = data.Id, Name = data.Name, IsChecked = data.IsChecked.Value, LasUpdated = data.DateUpdated
                                   },
                                   checklist);
                    }
                }

                break;
            }

            case EntityUpdateType.Remove:
                TryRemoveCheckWithTrigger(data.Id, out _);
                break;
            
            case EntityUpdateType.Update:
            {
                if (Checks.TryGetValue(data.Id, out var check))
                {
                    if (check.Checklist.Id != data.ChecklistId)
                    {
                        TryRemoveCheckWithTrigger(data.Id, out _);
                        
                        if(Checklists.TryGetValue(data.ChecklistId, out var newChecklist))
                        {
                            CreateCheckWithTrigger(new CheckCreationInfo()
                            {
                                Id = check.Id, 
                                IsChecked = check.IsChecked, 
                                LasUpdated = check.LastModified, 
                                Name = check.Name
                            }, newChecklist);
                        }
                    }
                    else
                        UpdateCheckWithTrigger(check, data);
                }

                break;
            }
            
            case EntityUpdateType.Copy:
                throw new InvalidOperationException("Checks are not copied");
                
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void CreateCheckWithTrigger(CheckCreationInfo check, Checklist checklist)
    {
        BuildCheck(check, checklist);

        // TODO: trigger update for SignalR
    }

    private bool TryRemoveCheckWithTrigger(string checkId, [NotNullWhen(true)] out Check? check)
    {
        if(Checks.TryRemove(checkId, out check))
        {
            check.Checklist.Checks.Remove(check);
                    
            // TODO: trigger update for SignalR

            return true;
        }

        return false;
    }

    private void UpdateCheckWithTrigger(Check check, CheckUpdatedData data)
    {
        if (data.Name is not null)
            check.Name = data.Name;

        if (data.IsChecked is not null)
            check.UpdateStatus(data.IsChecked.Value, data.DateUpdated);

        // TODO: trigger update for SignalR
    }
    
#endregion

#region Checklist Updates

    public async Task UpdateChecklist(ChecklistUpdatedData data)
    {
        switch (data.UpdateType)
        {
            case EntityUpdateType.Add:
            {
                if (Checklists.TryGetValue(data.Id, out var checklist))
                {
                    UpdateChecklistWithTrigger(checklist, data);
                }
                else
                {
                    if (data.Name is null)
                    {
                        Exception ex = new ArgumentNullException(nameof(data), "Checklist updates need a 'name' value to create new Checklist.");
                        Log.Logger.Error(ex, "Failed creating new checklist {checklistId}", data.Id);
                        break;
                    }
                    
                    if (!TryGetCardFromId(data.CardId, out var card))
                        break;
                    
                    CreateChecklistWithTrigger(new ChecklistCreationInfo()
                                           {
                                               Id = data.Id, Name = data.Name, CheckInfos = []
                                           },
                                           card);
                }

                break;
            }
            
            case EntityUpdateType.Remove:
                TryRemoveChecklistWithTrigger(data.Id, out _);
                break;
            
            case EntityUpdateType.Update:
            {
                if (Checklists.TryGetValue(data.Id, out var checklist))
                {
                    if (checklist.Card.Id != data.CardId)
                    {
                        checklist.Card.Checklists.Remove(checklist);
                        // TODO: Trigger SignalR
                        
                        if (TryGetCardFromId(data.CardId, out var newCard))
                        {
                            checklist.Card = newCard;
                            newCard.Checklists.Add(checklist);
                            
                            // TODO: Trigger SignalR
                        }
                    }
                    else
                        UpdateChecklistWithTrigger(checklist, data);
                }
                else if (TryGetCardFromId(data.CardId, out var newCard))
                {
                    var info = await GetChecklistFromSource(data.Id);

                    CreateChecklistWithTrigger(info, newCard);
                }
                break;
            }
            
            case EntityUpdateType.Copy:
            {
                throw new NotImplementedException(); // Not too sure how this works will need to look into once webhooks are working.
                break;
            }
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private bool TryRemoveChecklistWithTrigger(string checklistId, [NotNullWhen(true)] out Checklist? checklist)
    {
        if (Checklists.TryRemove(checklistId, out checklist))
        {
            checklist.Card.Checklists.Remove(checklist);
                    
            // TODO: trigger update for SignalR

            return true;
        }

        return false;
    }

    private void UpdateChecklistWithTrigger(Checklist checklist, ChecklistUpdatedData data)
    {
        if (data.Name is not null)
            checklist.Name = data.Name;
        
        // TODO: trigger update for SignalR
    }

    private void CreateChecklistWithTrigger(ChecklistCreationInfo checklist, Card card)
    {
        BuildChecklist(checklist, card);
        
        // TODO Trigger signalR
    }
    
#endregion    


}

public abstract record BaseUpdate(DateTimeOffset DateUpdated);

public record CheckUpdatedData(DateTimeOffset DateUpdated, string Id, string ChecklistId, EntityUpdateType UpdateType, string? Name = null, bool? IsChecked = null) : BaseUpdate(DateUpdated);

public record ChecklistUpdatedData(DateTimeOffset DateUpdated, string Id, string CardId, EntityUpdateType UpdateType, string? Name = null) : BaseUpdate(DateUpdated);

public record CommentUpdatedData(DateTimeOffset DateUpdated, string Id) : BaseUpdate(DateUpdated);

public record CardUpdatedData(DateTimeOffset DateUpdated, string Id) : BaseUpdate(DateUpdated);

public record JobCardUpdatedData(DateTimeOffset DateUpdated, string Id) : BaseUpdate(DateUpdated);

public record RedCardUpdatedData(DateTimeOffset DateUpdated, string Id) : BaseUpdate(DateUpdated);

public record YellowCardUpdatedData(DateTimeOffset DateUpdated, string Id) : BaseUpdate(DateUpdated);

public record SalesOrderUpdatedData(DateTimeOffset DateUpdated, string Id) : BaseUpdate(DateUpdated);
public record UserUpdatedData(DateTimeOffset DateUpdated, string Id) : BaseUpdate(DateUpdated);