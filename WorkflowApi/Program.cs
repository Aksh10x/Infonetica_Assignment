using WorkflowApi.Models;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Workflow API", Version = "v1" });
});

var defs = new Dictionary<string, WorkflowDef>();
var insts = new Dictionary<string, WorkflowInst>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Workflow API v1"));
}


// ROUTES


// ENDPOINT: create a new workflow definition [POST /defs]
app.MapPost("/defs", (WorkflowDef wf) =>
{
    // check if workflow definition already exists
    if (defs.ContainsKey(wf.Id))
        return Results.BadRequest($"Definition '{wf.Id}' already exists.");

    // only one initial state allowed
    if (wf.States.Count(s => s.IsInitial) !=1)
        return Results.BadRequest("Exactly one state must be marked IsInitial.");

    // check for duplicate state ids
    var stateIds = wf.States.Select(s => s.Id).ToList();
    if (stateIds.Count != stateIds.Distinct().Count())
        return Results.BadRequest("State IDs must be unique.");

    // check for duplicate action ids
    var actionIds = wf.Actions.Select(a => a.Id).ToList();
    if (actionIds.Count != actionIds.Distinct().Count())
        return Results.BadRequest("Action IDs must be unique.");

    // validate each action and ensure it references existing states
    foreach (var act in wf.Actions)
    {
        // ToState must be defined
        if (!stateIds.Contains(act.ToState))
            return Results.BadRequest(
                $"Action '{act.Id}' has unknown ToState '{act.ToState}'."
            );

        // FromStates must all be defined
        var invalidFrom = act.FromStates.Except(stateIds).ToList();
        if (invalidFrom.Any())
            return Results.BadRequest(
                $"Action '{act.Id}' has invalid FromStates: {string.Join(", ", invalidFrom)}."
            );
    }

    // add new definition to store
    defs[wf.Id] = wf;
    return Results.Created($"/defs/{wf.Id}", wf);
});


// ENDPOINT: get all workflow definition [GET /defs]
app.MapGet("/defs", () => Results.Ok(defs.Values));


// ENDPOINT: get a single workflow definition by id [GET /defs/{id}]
app.MapGet("/defs/{id}", (string id) =>
    defs.TryGetValue(id, out var wf)
        ? Results.Ok(wf)
        : Results.NotFound($"Definition '{id}' not found.")
);


// ENDPOINT: start new workflow instance [POST /insts?defId={defId}]
app.MapPost("/insts", (string defId) =>
{
    // check if definition exists
    if (!defs.TryGetValue(defId, out var def))
        return Results.NotFound($"Definition '{defId}' not found.");

    // check if initial state exists
    var init = def.States.Single(s => s.IsInitial);

    // create new instance
    var inst = new WorkflowInst(
        Id: Guid.NewGuid().ToString(),
        DefId: defId,
        CurrentState: init.Id,
        History: new List<(string, DateTime)>()
    );

    //add new instance to store
    insts[inst.Id] = inst;
    return Results.Created($"/insts/{inst.Id}", inst);
});


// ENDPOINT: get all worfklow instances [GET /insts]
app.MapGet("/insts", () => Results.Ok(insts.Values));


// ENDPOINT: execute an action [POST /insts/{id}/actions/{actionId}]
app.MapPost("/insts/{id}/actions/{actionId}", (string id, string actionId) =>
{
    // lookup instance
    if (!insts.TryGetValue(id, out var inst))
        return Results.NotFound($"Instance '{id}' not found.");

    // lookup the definition and action
    var def = defs[inst.DefId];
    var act = def.Actions.SingleOrDefault(a => a.Id == actionId);
    if (act is null || !act.Enabled)
        return Results.BadRequest($"Action '{actionId}' is invalid or disabled.");
    if (!act.FromStates.Contains(inst.CurrentState))
        return Results.BadRequest($"Action '{actionId}' not allowed from state '{inst.CurrentState}'.");

    // create a new record with updated state and history
    var updatedInst = inst with
    {
        CurrentState = act.ToState,
        History = inst.History
                         .Append((actionId, DateTime.UtcNow))
                         .ToList()
    };

    // replace the old instance in the store
    insts[id] = updatedInst;

    // return the updated instance
    return Results.Ok(updatedInst);
});


app.UseHttpsRedirection();

app.Run();