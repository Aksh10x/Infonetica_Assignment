
# Workflow Engine API

A minimal .NET 8 Web API for defining and executing workflow state machines entirely in-memory.

---

## Quick‑start

1. **Clone the repo**

    ```bash
   git clone https://github.com/Aksh10x/Infonetica_Assignment.git
   cd Infonetica_Assignment
   ```

2. **Build & run**

   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```

3. **Browse Swagger UI** (Development only)

   * Open: `https://localhost:7174/swagger/index.html` or `http://localhost:5195/swagger/index.html`

4. **Sample curl commands**

   ```bash
   # Create a workflow definition according to schema
   curl -k -X POST https://localhost:7174/defs \
     -H "Content-Type: application/json" \
     -d '<workflowDefinitionJson>'

   # List definitions
   curl -k https://localhost:7174/defs

   # Start a workflow instance
   curl -k -X POST "https://localhost:7174/insts?defId=wf1"

   # Execute an action on an instance
   curl -k -X POST https://localhost:7174/insts/{instanceId}/actions/a1
   ```

---

## Assumptions & Shortcuts

* **In-memory storage**: All data lives in RAM; no external database.
* **Single-project solution**: All endpoints and models are defined in `Program.cs` and `Models/WorkflowModels.cs`.
* **No authentication**: API is unsecured for simplicity.
* **No pagination**: List endpoints return all items.
* **Sequential GUID IDs**: Instances use `Guid.NewGuid().ToString()` for IDs.

---

## Known Limitations

* **Volatile data**: Restarting the app clears all definitions & instances.
* **No concurrency control**: Potential race conditions if multiple clients hit the API simultaneously.
* **No unit tests**: Validation and business logic are untested.
* **Basic error handling**: Only manual try/catch in handlers; no global middleware.

---

## Next Steps

* **Persistence**: Swap in EF Core or another data store for durability.
* **Authentication/Authorization** with JWT or OAuth.
* **Unit & integration tests** (e.g., xUnit, FluentAssertions).
* **Improved error-handling middleware**.
* **Pagination & filtering** on list endpoints.
