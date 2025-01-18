using System.Data;
using Dapper;
using ZoneRV.Services.DB;
using ZoneRV.Services.Trello.Models;

namespace ZoneRV.Services.Trello.DB;

public class TrelloActionData
{
    private readonly SqlDataAccess _db;

    public TrelloActionData(SqlDataAccess db)
    {
        _db = db;
    }

    public async Task InsertTrelloAction(CachedTrelloAction action)
        => await _db.SaveData("dbo.spTrelloAction_Insert", action);

    public async Task<IEnumerable<CachedTrelloAction>> InsertTrelloActions(IEnumerable<CachedTrelloAction> actions)
    {
        var output = new DataTable();
        actions = actions.ToList();

        output.Columns.Add("ActionId",   typeof(string));
        output.Columns.Add("BoardId",    typeof(string));
        output.Columns.Add("CardId",     typeof(string));
        output.Columns.Add("DateOffset", typeof(DateTimeOffset));
        output.Columns.Add("ActionType", typeof(string));
        output.Columns.Add("MemberId",   typeof(string));
        output.Columns.Add("Content",    typeof(string));
        output.Columns.Add("CheckId",    typeof(string));
        output.Columns.Add("DueDate",    typeof(DateTimeOffset));

        foreach (var action in actions)
        {
            output.Rows.Add(
                action.ActionId, 
                action.BoardId, 
                action.CardId, 
                action.DateOffset,
                action.ActionType, 
                action.MemberId, 
                action.Content,
                action.CheckId,
                action.DueDate);
        }

        var a = new
        {
            actions = output.AsTableValuedParameter("TrelloActionUDT")
        };

        await _db.SaveData("dbo.spTrelloAction_InsertSet", a);
        
        return actions;
    }

    public async Task<IEnumerable<CachedTrelloAction>> GetActions(string boardId)
        => await _db.LoadData<CachedTrelloAction, dynamic>("dbo.spTrelloAction_GetAllOnBoard", new { BoardId = boardId });

    public async Task DeleteActions(string boardId)
        => await _db.SaveData("dbo.spTrelloAction_DeleteOnBoard", new { BoardId = boardId });
}