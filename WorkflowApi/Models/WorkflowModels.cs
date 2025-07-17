using System;
using System.Collections.Generic;

namespace WorkflowApi.Models
{
    public record State(
        string Id,
        bool IsInitial,
        bool IsFinal,
        bool Enabled
    );

    public record ActionDef(
        string Id,
        bool Enabled,
        List<string> FromStates,
        string ToState
    );

    public record WorkflowDef(
        string Id,
        List<State> States,
        List<ActionDef> Actions
    );

    public record WorkflowInst(
        string Id,
        string DefId,
        string CurrentState,
        List<(string ActionId, DateTime Timestamp)> History
    );
}

