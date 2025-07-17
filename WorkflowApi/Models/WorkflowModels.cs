using System;
using System.Collections.Generic;

namespace WorkflowApi.Models
{
    public record State(
        string id,
        bool isInitial,
        bool isFinal,
        bool enabled
    );

    public record ActionDef(
        string id,
        bool enabled,
        List<string> fromStates,
        string toState
    );

    public record WorkflowDef(
        string id,
        List<State> States,
        List<ActionDef> Actions
    );

    public record WorkflowInst(
        string id,
        string defId,
        string currentState,
        List<(string ActionId, DateTime Timestamp)> History
    );
}

